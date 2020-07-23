using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Core;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
#if !NET_DOTS
using System.Linq;
#endif

namespace Unity.Entities
{
    public unsafe abstract class ComponentSystemGroup : ComponentSystem
    {
        private bool m_systemSortDirty = false;
        private bool m_UnmanagedSystemSortDirty = false;

        internal List<ComponentSystemBase> m_systemsToUpdate = new List<ComponentSystemBase>();
        internal List<ComponentSystemBase> m_systemsToRemove = new List<ComponentSystemBase>();

        internal UnsafeList<SystemRefUntyped> m_UnmanagedSystemsToUpdate;
        internal UnsafeList<SystemRefUntyped> m_UnmanagedSystemsToRemove;

        public virtual IEnumerable<ComponentSystemBase> Systems => m_systemsToUpdate;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_UnmanagedSystemsToUpdate = new UnsafeList<SystemRefUntyped>(0, Allocator.Persistent);
            m_UnmanagedSystemsToRemove = new UnsafeList<SystemRefUntyped>(0, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            m_UnmanagedSystemsToRemove.Dispose();
            m_UnmanagedSystemsToUpdate.Dispose();
            base.OnDestroy();
        }

        public void AddSystemToUpdateList(ComponentSystemBase sys)
        {
            if (sys != null)
            {
                if (this == sys)
                    throw new ArgumentException($"Can't add {TypeManager.GetSystemName(GetType())} to its own update list");

                // Check for duplicate Systems. Also see issue #1792
                if (m_systemsToUpdate.IndexOf(sys) >= 0)
                    return;

                m_systemsToUpdate.Add(sys);
                m_systemSortDirty = true;
            }
        }

        private int UnmanagedSystemIndex(SystemRefUntyped sysRef)
        {
            int len = m_UnmanagedSystemsToUpdate.Length;
            var ptr = m_UnmanagedSystemsToUpdate.Ptr;
            for (int i = 0; i < len; ++i)
            {
                if (ptr[len] == sysRef)
                {
                    return i;
                }
            }
            return -1;
        }

        internal void AddUnmanagedSystemToUpdateList(SystemRefUntyped sysRef)
        {
            if (-1 != UnmanagedSystemIndex(sysRef))
                return;

            m_UnmanagedSystemsToUpdate.Add(sysRef);
            m_UnmanagedSystemSortDirty = true;
        }

        public void RemoveSystemFromUpdateList(ComponentSystemBase sys)
        {
            m_systemSortDirty = true;
            m_systemsToRemove.Add(sys);
        }

        internal void RemoveUnmanagedSystemFromUpdateList(SystemRefUntyped sys)
        {
            m_UnmanagedSystemSortDirty = true;
            m_UnmanagedSystemsToRemove.Add(sys);
        }

        public virtual void SortSystemUpdateList()
        {
            if (!m_systemSortDirty)
                return;
            m_systemSortDirty = false;

            if (m_systemsToRemove.Count > 0)
            {
                foreach (var sys in m_systemsToRemove)
                    m_systemsToUpdate.Remove(sys);
                m_systemsToRemove.Clear();
            }

            foreach (var sys in m_systemsToUpdate)
            {
                if (TypeManager.IsSystemAGroup(sys.GetType()))
                {
                    ((ComponentSystemGroup)sys).SortSystemUpdateList();
                }
            }

            ComponentSystemSorter.Sort(m_systemsToUpdate, x => x.GetType(), this.GetType());
        }

        internal void SortUnmanagedSystemUpdateList()
        {
            if (!m_UnmanagedSystemSortDirty)
                return;

            m_UnmanagedSystemSortDirty = false;

            if (m_UnmanagedSystemsToRemove.Length > 0)
            {
                // This is O(N^2) and should be rewritten but following the pattern of managed systems for now
                for (int i = 0; i < m_UnmanagedSystemsToRemove.Length; ++i)
                {
                    int index = UnmanagedSystemIndex(m_UnmanagedSystemsToRemove[i]);
                    if (-1 != index)
                    {
                        m_UnmanagedSystemsToUpdate.RemoveAtSwapBack(index);
                    }
                }

                m_UnmanagedSystemsToRemove.Clear();
            }

            /* -- concept of unmanaged group?
            foreach (var sys in m_systemsToUpdate)
            {
                if (TypeManager.IsSystemAGroup(sys.GetType()))
                {
                    ((ComponentSystemGroup)sys).SortSystemUpdateList();
                }
            }
            */

            // TODO: Concept of sorting unmanaged systems according to update order etc
            NativeSortExtension.Sort(m_UnmanagedSystemsToUpdate.Ptr, m_UnmanagedSystemsToUpdate.Length);
        }

#if UNITY_DOTSPLAYER
        public void RecursiveLogToConsole()
        {
            foreach (var sys in m_systemsToUpdate)
            {
                if (sys is ComponentSystemGroup)
                {
                    (sys as ComponentSystemGroup).RecursiveLogToConsole();
                }

                var name = TypeManager.GetSystemName(sys.GetType());
                Debug.Log(name);
            }
        }

#endif

        protected override void OnStopRunning()
        {
        }

        internal override void OnStopRunningInternal()
        {
            OnStopRunning();

            foreach (var sys in m_systemsToUpdate)
            {
                if (sys == null)
                    continue;

                if (sys.m_StatePtr == null)
                    continue;

                if (!sys.m_StatePtr->m_PreviouslyEnabled)
                    continue;

                sys.m_StatePtr->m_PreviouslyEnabled = false;
                sys.OnStopRunningInternal();
            }

            for (int i = 0; i < m_UnmanagedSystemsToUpdate.Length; ++i)
            {
                var sys = World.ResolveSystemUntyped(m_UnmanagedSystemsToUpdate[i]);

                if (sys == null || !sys->m_PreviouslyEnabled)
                    continue;

                sys->m_PreviouslyEnabled = false;

                // Optional callback here
            }
        }

        /// <summary>
        /// An optional callback.  If set, this group's systems will be updated in a loop while
        /// this callback returns true.  This can be used to implement custom processing before/after
        /// update (first call should return true, second should return false), or to run a group's
        /// systems multiple times (return true more than once).
        ///
        /// The group is passed as the first parameter.
        /// </summary>
        public Func<ComponentSystemGroup, bool> UpdateCallback;

        protected override void OnUpdate()
        {
            if (UpdateCallback == null)
            {
                UpdateAllSystems();
            }
            else
            {
                while (UpdateCallback(this))
                {
                    UpdateAllSystems();
                }
            }
        }

        unsafe void UpdateAllSystems()
        {
            if (m_systemSortDirty)
                SortSystemUpdateList();

            for (int i = 0; i < m_systemsToUpdate.Count; ++i)
            {
                var sys = m_systemsToUpdate[i];
                try
                {
                    sys.Update();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
#if UNITY_DOTSPLAYER
                    // When in a DOTS Runtime build, throw this upstream -- continuing after silently eating an exception
                    // is not what you'll want, except maybe once we have LiveLink.  If you're looking at this code
                    // because your LiveLink dots runtime build is exiting when you don't want it to, feel free
                    // to remove this block, or guard it with something to indicate the player is not for live link.
                    throw;
#endif
                }

                if (World.QuitUpdate)
                    break;
            }

            for (int i = 0; i < m_UnmanagedSystemsToUpdate.Length; ++i)
            {
                var sys = World.ResolveSystemUntyped(m_UnmanagedSystemsToUpdate[i]);
                if (sys != null)
                {
                    if (SystemBase.UnmanagedUpdate(sys, out var details))
                    {
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS
                        var metaIndex = sys->m_UnmanagedMetaIndex;
                        var systemDebugName = SystemBaseRegistry.GetDebugName(metaIndex);
                        var errorString = details.FormatToString(systemDebugName);
                        Debug.LogError(errorString);
                        #endif
                    }
                }
            }
        }
    }

    public static class ComponentSystemGroupExtensions
    {
        internal static void AddSystemToUpdateList(this ComponentSystemGroup self, SystemRefUntyped sysRef)
        {
            self.AddUnmanagedSystemToUpdateList(sysRef);
        }

        internal static void RemoveSystemFromUpdateList(this ComponentSystemGroup self, SystemRefUntyped sysRef)
        {
            self.RemoveUnmanagedSystemFromUpdateList(sysRef);
        }
    }
}
