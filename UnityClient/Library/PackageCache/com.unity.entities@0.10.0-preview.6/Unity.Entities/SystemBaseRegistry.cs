using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    internal unsafe struct UnmanagedComponentSystemDelegates
    {
        // The function to call from a burst context to create/update/destroy.
        internal fixed ulong BurstFunctions[3];

        // The function to call from a managed context to create/update/destroy.
        internal fixed ulong ManagedFunctions[3];

        // Maintain a reference to any burst->managed delegate wrapper so they are not collected
        internal fixed ulong GCDefeat1[3];

        internal unsafe void Dispose()
        {
            for (int i = 2; i >= 0; --i)
            {
                if (ManagedFunctions[i] != 0)
                {
                    GCHandle.FromIntPtr((IntPtr)ManagedFunctions[i]).Free();
                }

                if (GCDefeat1[i] != 0)
                {
                    GCHandle.FromIntPtr((IntPtr)GCDefeat1[i]).Free();
                }
            }
        }
    }

    internal unsafe struct UnmanagedSystemTypeRegistryData
    {
        private UnsafeHashMap<long, int> m_TypeHashToIndex;
        private UnsafeList<UnmanagedComponentSystemDelegates> m_Delegates;
        private UnsafeList<FixedString64> m_DebugNames;

        public bool Constructed => m_Delegates.Ptr != null;

        internal void Construct()
        {
            m_TypeHashToIndex = new UnsafeHashMap<long, int>(64, Allocator.Persistent);
            m_Delegates = new UnsafeList<UnmanagedComponentSystemDelegates>(64, Allocator.Persistent);
            m_DebugNames = new UnsafeList<FixedString64>(64, Allocator.Persistent);
        }

        internal void Dispose()
        {
            for (int i = 0; i < m_Delegates.Length; ++i)
            {
                m_Delegates[i].Dispose();
            }

            m_DebugNames.Dispose();
            m_Delegates.Dispose();
            m_TypeHashToIndex.Dispose();

            this = default;
        }

        internal int AddSystemType(long typeHash, FixedString64 debugName, UnmanagedComponentSystemDelegates delegates)
        {
            if (m_TypeHashToIndex.TryGetValue(typeHash, out int index))
            {
                //Debug.Log($"replacing registration for {debugName}");
                if (m_DebugNames[index] != debugName)
                {
                    Debug.LogError($"Type hash {typeHash} for {debugName} collides with {m_DebugNames[index]}. Skipping this type. Rename the type to avoid the collision.");
                    return -1;
                }

                m_Delegates[index] = delegates;
                return index;
            }
            else
            {
                // Debug.Log($"adding registration for {debugName}");
                int newIndex = m_Delegates.Length;
                m_TypeHashToIndex.Add(typeHash, newIndex);
                m_DebugNames.Add(debugName);
                m_Delegates.Add(delegates);
                return newIndex;
            }
        }

        internal bool FindSystemMetaIndex(long typeHash, out int index)
        {
            return m_TypeHashToIndex.TryGetValue(typeHash, out index);
        }

        internal FixedString64 GetSystemDebugName(int index)
        {
            return m_DebugNames[index];
        }

        internal UnmanagedComponentSystemDelegates GetSystemDelegates(int index)
        {
            return m_Delegates[index];
        }
    }

    public static class SystemBaseRegistry
    {
        struct Dummy
        {
        }

        internal readonly static SharedStatic<UnmanagedSystemTypeRegistryData> s_Data = SharedStatic<UnmanagedSystemTypeRegistryData>.GetOrCreate<Dummy>();

        // TODO: Need to dispose this thing when domain reload happens.
        public delegate void ForwardingFunc(IntPtr systemPtr, IntPtr state);

        internal static int GetSystemTypeMetaIndex(long typeHash)
        {
            ref var data = ref s_Data.Data;

            if (!data.Constructed)
            {
                throw new InvalidOperationException("type registry is not initialized");
            }

            if (data.FindSystemMetaIndex(typeHash, out int index))
            {
                return index;
            }

            throw new ArgumentException("System type is not registered");
        }

        public static unsafe void AddUnmanagedSystemType(long typeHash, ForwardingFunc onCreate, ForwardingFunc onUpdate, ForwardingFunc onDestroy, string debugName, int burstCompileBits)
        {
            // Debug.Log($"Adding unmanaged system type {debugName}, bcb={burstCompileBits}");
            ref var data = ref s_Data.Data;

            if (!data.Constructed)
            {
                data.Construct();
               #if !NET_DOTS
                AppDomain.CurrentDomain.DomainUnload += (_, __) =>
                {
                    s_Data.Data.Dispose();
                };
                #endif
            }

            FixedString64 debugNameFixed = new FixedString64(debugName);

            var delegates = default(UnmanagedComponentSystemDelegates);

            ulong* burstCompiledFunctions = stackalloc ulong[3];

#if !NET_DOTS
            burstCompiledFunctions[0] = (burstCompileBits & 1) != 0 ? (ulong)BurstCompiler.CompileFunctionPointer<ForwardingFunc>(onCreate).Value : 0;
            burstCompiledFunctions[1] = (burstCompileBits & 2) != 0 ? (ulong)BurstCompiler.CompileFunctionPointer<ForwardingFunc>(onUpdate).Value : 0;
            burstCompiledFunctions[2] = (burstCompileBits & 4) != 0 ? (ulong)BurstCompiler.CompileFunctionPointer<ForwardingFunc>(onDestroy).Value : 0;
#else
            burstCompiledFunctions[0] = 0;
            burstCompiledFunctions[1] = 0;
            burstCompiledFunctions[2] = 0;
#endif

            void SelectManagedFn(out ulong result, ulong burstFn, ForwardingFunc managedFn)
            {
            #if !NET_DOTS
                if (burstFn != 0)
                {
                    result = (ulong)GCHandle.ToIntPtr(GCHandle.Alloc(new FunctionPointer<ForwardingFunc>((IntPtr)burstFn).Invoke));
                }
                else
            #endif
                {
                    result = (ulong)GCHandle.ToIntPtr(GCHandle.Alloc(managedFn));
                }
            }

            // Select what to call when calling into a system from managed code.
            SelectManagedFn(out delegates.ManagedFunctions[0], burstCompiledFunctions[0], onCreate);
            SelectManagedFn(out delegates.ManagedFunctions[1], burstCompiledFunctions[1], onUpdate);
            SelectManagedFn(out delegates.ManagedFunctions[2], burstCompiledFunctions[2], onDestroy);

            void SelectBurstFn(out ulong result, out ulong defeatGc, ulong burstFn, ForwardingFunc managedFn)
            {
            #if !NET_DOTS
                if (burstFn != default)
                {
                    result = burstFn;
                    defeatGc = default;
                }
                else
                #endif
                {
                    ForwardingFunc wrapper = (IntPtr system, IntPtr state) =>
                    {
                        try
                        {
                            managedFn(system, state);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    };

                    defeatGc = (ulong)GCHandle.ToIntPtr(GCHandle.Alloc(wrapper));
                    result = (ulong)Marshal.GetFunctionPointerForDelegate(wrapper);
                }
            }

            // Select what to call when calling into a system from Burst code.
            SelectBurstFn(out delegates.BurstFunctions[0], out delegates.GCDefeat1[0], burstCompiledFunctions[0], onCreate);
            SelectBurstFn(out delegates.BurstFunctions[1], out delegates.GCDefeat1[1], burstCompiledFunctions[1], onUpdate);
            SelectBurstFn(out delegates.BurstFunctions[2], out delegates.GCDefeat1[2], burstCompiledFunctions[2], onDestroy);

            s_Data.Data.AddSystemType(typeHash, debugNameFixed, delegates);
        }

        [BurstDiscard]
        internal static void CheckBurst(ref bool status)
        {
            status = false;
        }

        internal static unsafe void CallForwardingFunction(SystemState* systemState, int index)
        {
            var metaIndex = systemState->m_UnmanagedMetaIndex;
            var systemPointer = systemState->m_SystemPtr;
            var delegates = s_Data.Data.GetSystemDelegates(metaIndex);
            bool isBurst = true;
            CheckBurst(ref isBurst);

#if !NET_DOTS
            if (isBurst)
            {
                // Burst: we're calling either directly into Burst code, or we are calling into a managed wrapper.
                // In any case, creating the function pointer from the IntPtr is free.
                new FunctionPointer<ForwardingFunc>((IntPtr)delegates.BurstFunctions[index]).Invoke((IntPtr)systemPointer, (IntPtr)systemState);
            }
            else
#endif
            {
                // We're in managed land. We may be calling into either a managed routine, or into Burst code.
                // We have a managed delegate GCHandle ready to go.
                ForwardToManaged((IntPtr)delegates.ManagedFunctions[index], systemState, systemPointer);
            }
        }

        [BurstDiscard]
        private static unsafe void ForwardToManaged(IntPtr delegateIntPtr, SystemState* systemState, void* systemPointer)
        {
            GCHandle h = GCHandle.FromIntPtr(delegateIntPtr);
            ((ForwardingFunc)h.Target)((IntPtr)systemPointer, (IntPtr)systemState);
        }

        internal static unsafe void CallOnCreate(SystemState* systemState)
        {
            CallForwardingFunction(systemState, 0);
        }

        internal static unsafe void CallOnUpdate(SystemState* systemState)
        {
            CallForwardingFunction(systemState, 1);
        }

        internal static unsafe void CallOnDestroy(SystemState* systemState)
        {
            CallForwardingFunction(systemState, 2);
        }

        internal static FixedString64 GetDebugName(int metaIndex)
        {
            return s_Data.Data.GetSystemDebugName(metaIndex);
        }
    }
}
