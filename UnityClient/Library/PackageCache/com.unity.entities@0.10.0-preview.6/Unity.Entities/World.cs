using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Core;
using UnityEngine.Assertions;

namespace Unity.Entities
{
    /// <summary>
    /// Specify all traits a <see cref="World"/> can have.
    /// </summary>
    [Flags]
    public enum WorldFlags : byte
    {
        /// <summary>
        /// Default WorldFlags value.
        /// </summary>
        None       = 0,

        /// <summary>
        /// The main <see cref="World"/> for a game/application.
        /// This flag is combined with <see cref="Editor"/>, <see cref="Game"/> and <see cref="Simulation"/>.
        /// </summary>
        Live       = 1,

        /// <summary>
        /// Main <see cref="Live"/> <see cref="World"/> running in the Editor.
        /// </summary>
        Editor     = 1 << 1 | Live,

        /// <summary>
        /// Main <see cref="Live"/> <see cref="World"/> running in the Player.
        /// </summary>
        Game       = 1 << 2 | Live,

        /// <summary>
        /// Any additional <see cref="Live"/> <see cref="World"/> running in the application for background processes that
        /// queue up data for other <see cref="Live"/> <see cref="World"/> (ie. physics, AI simulation, networking, etc.).
        /// </summary>
        Simulation = 1 << 3 | Live,

        /// <summary>
        /// <see cref="World"/> on which conversion systems run to transform authoring data to runtime data.
        /// </summary>
        Conversion = 1 << 4,

        /// <summary>
        /// <see cref="World"/> in which temporary results are staged before being moved into a <see cref="Live"/> <see cref="World"/>.
        /// Typically combined with <see cref="Conversion"/> to represent an intermediate step in the full conversion process.
        /// </summary>
        Staging    = 1 << 5,

        /// <summary>
        /// <see cref="World"/> representing a previous state of another <see cref="World"/> typically to compute
        /// a diff of runtime data - for example useful for undo/redo or Live Link.
        /// </summary>
        Shadow     = 1 << 6,

        /// <summary>
        /// Dedicated <see cref="World"/> for managing incoming streamed data to the Player.
        /// </summary>
        Streaming  = 1 << 7,
    }

    /// <summary>
    /// When entering playmode or the game starts in the Player a default world is created.
    /// Sometimes you need multiple worlds to be setup when the game starts or perform some
    /// custom world initialization. This lets you override the bootstrap of game code world creation.
    /// </summary>
    public interface ICustomBootstrap
    {
        // Returns true if the bootstrap has performed initialization.
        // Returns false if default world initialization should be performed.
        bool Initialize(string defaultWorldName);
    }

    /// <summary>
    /// An identifier representing an unmanaged system struct instance in a particular world.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemRefUntyped : IEquatable<SystemRefUntyped>, IComparable<SystemRefUntyped>
    {
        internal ushort m_Slot;
        internal ushort m_Version;
        internal uint m_WorldSeqNo;

        private ulong ToUlong()
        {
            return ((ulong)m_WorldSeqNo << 32) | ((ulong)m_Slot << 16) | (ulong)m_Version;
        }

        internal SystemRefUntyped(ushort slot, ushort version, uint worldSeqNo)
        {
            m_Slot = slot;
            m_Version = version;
            m_WorldSeqNo = worldSeqNo;
        }

        public int CompareTo(SystemRefUntyped other)
        {
            ulong a = ToUlong();
            ulong b = other.ToUlong();
            if (a < b)
                return -1;
            else if (a > b)
                return 1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is SystemRefUntyped foo)
                return Equals(foo);
            return false;
        }

        public bool Equals(SystemRefUntyped other)
        {
            return ToUlong() == other.ToUlong();
        }

        public override int GetHashCode()
        {
            int hashCode = -116238775;
            hashCode = hashCode * -1521134295 + m_Slot.GetHashCode();
            hashCode = hashCode * -1521134295 + m_Version.GetHashCode();
            hashCode = hashCode * -1521134295 + m_WorldSeqNo.GetHashCode();
            return hashCode;
        }

        public static bool operator==(SystemRefUntyped a, SystemRefUntyped b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(SystemRefUntyped a, SystemRefUntyped b)
        {
            return !a.Equals(b);
        }
    }

    /// <summary>
    /// An identifier representing an unmanaged system struct instance in a particular world.
    /// </summary>
    internal struct SystemRef<T> where T : struct, ISystemBase
    {
        internal SystemRefUntyped m_Ref;

        internal SystemRef(ushort slot, ushort version, uint worldSeqNo)
        {
            m_Ref = new SystemRefUntyped(slot, version, worldSeqNo);
        }

        public static implicit operator SystemRefUntyped(SystemRef<T> self) => self.m_Ref;
    }

    [DebuggerDisplay("{Name} - {Flags} (#{SequenceNumber})")]
    public unsafe partial class World : IDisposable
    {
        internal static readonly List<World> s_AllWorlds = new List<World>();

        public static World DefaultGameObjectInjectionWorld { get; set; }

    #if UNITY_DOTSPLAYER
        [Obsolete("use World.All instead. (RemovedAfter 2020-06-02)")]
        public static World[] AllWorlds => s_AllWorlds.ToArray();
    #else
        [Obsolete("use World.All instead. (RemovedAfter 2020-06-02)")]
        public static System.Collections.ObjectModel.ReadOnlyCollection<World> AllWorlds => new System.Collections.ObjectModel.ReadOnlyCollection<World>(s_AllWorlds);

        Dictionary<Type, ComponentSystemBase> m_SystemLookup = new Dictionary<Type, ComponentSystemBase>();
    #endif
    #if ENABLE_UNITY_COLLECTIONS_CHECKS
        bool m_AllowGetSystem = true;
    #endif
        public static NoAllocReadOnlyCollection<World> All { get; } = new NoAllocReadOnlyCollection<World>(s_AllWorlds);

#pragma warning disable 649
        unsafe struct SystemSlot
        {
            internal ushort m_Version;
            internal ushort m_Padding0;
            internal int m_MetaIndex;
            internal void* m_SystemPointer;
            internal long m_TypeHash;
        }

#pragma warning restore 649

        UnsafeList<SystemSlot> m_UnmanagedSlots;

        // FIXME: This needs to be memory stable.
        // Otherwise things like:
        //   ref Foo = World.Resolve(fooRef);
        //   World.CreateSystem<Bar>();
        // can invalidate references that are in scope.
        UnsafeList<SystemState> m_UnmanagedState;
        UnsafeMultiHashMap<long, ushort> m_UnmanagedSlotByTypeHash;

        List<ComponentSystemBase> m_Systems = new List<ComponentSystemBase>();
        public NoAllocReadOnlyCollection<ComponentSystemBase> Systems { get; }

        EntityManager m_EntityManager;
        readonly ulong m_SequenceNumber;

        static int ms_SystemIDAllocator = 0;

        internal static readonly SharedStatic<ulong> ms_NextSequenceNumber = SharedStatic<ulong>.GetOrCreate<World>();

        public readonly WorldFlags Flags;

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public int Version { get; private set; }

        public EntityManager EntityManager => m_EntityManager;

        public bool IsCreated => m_Systems != null;

        public ulong SequenceNumber => m_SequenceNumber;

        protected TimeData m_CurrentTime;

        public ref TimeData Time => ref m_CurrentTime;

        protected EntityQuery m_TimeSingletonQuery;

        public World(string name) : this(name, WorldFlags.Simulation)
        {}

        private GCHandle m_AliveBool;

        internal World(string name, WorldFlags flags)
        {
            Systems = new NoAllocReadOnlyCollection<ComponentSystemBase>(m_Systems);

            m_SequenceNumber = ms_NextSequenceNumber.Data++;
            m_AliveBool = GCHandle.Alloc(this, GCHandleType.Weak);

            // Debug.LogError("Create World "+ name + " - " + GetHashCode());
            Name = name;
            Flags = flags;
            s_AllWorlds.Add(this);

            m_EntityManager = default;
            m_EntityManager.Initialize(this, m_AliveBool);
            m_TimeSingletonQuery = EntityManager.CreateEntityQuery(ComponentType.ReadWrite<WorldTime>(),
                ComponentType.ReadWrite<WorldTimeQueue>());

            m_UnmanagedSlots = new UnsafeList<SystemSlot>(32, Allocator.Persistent);
            m_UnmanagedState = new UnsafeList<SystemState>(32, Allocator.Persistent);
            m_UnmanagedSlotByTypeHash = new UnsafeMultiHashMap<long, ushort>(32, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (!IsCreated)
                throw new ArgumentException("The World has already been Disposed.");
            // Debug.LogError("Dispose World "+ Name + " - " + GetHashCode());

            m_EntityManager.PreDisposeCheck();
            s_AllWorlds.Remove(this);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_AllowGetSystem = false;
#endif
            DestroyAllSystemsAndLogException();
            DestroyAllUnmanagedSystemsAndLogException();

            // Destroy EntityManager last
            m_EntityManager.DestroyInstance();
            m_EntityManager = default;

            m_AliveBool.Free();

#if !UNITY_DOTSPLAYER
            m_SystemLookup.Clear();
            m_SystemLookup = null;
#endif

            if (DefaultGameObjectInjectionWorld == this)
                DefaultGameObjectInjectionWorld = null;

            m_UnmanagedSlotByTypeHash.Dispose();
            m_UnmanagedState.Dispose();
            m_UnmanagedSlots.Dispose();
        }

        public static void DisposeAllWorlds()
        {
            while (s_AllWorlds.Count != 0)
            {
                s_AllWorlds[0].Dispose();
            }
        }

        // Time management

        protected Entity TimeSingleton
        {
            get
            {
                if (m_TimeSingletonQuery.IsEmptyIgnoreFilter)
                {
        #if UNITY_EDITOR
                    var entity = EntityManager.CreateEntity(typeof(WorldTime), typeof(WorldTimeQueue));
                    EntityManager.SetName(entity , "WorldTime");
        #else
                    EntityManager.CreateEntity(typeof(WorldTime), typeof(WorldTimeQueue));
        #endif
                }

                return m_TimeSingletonQuery.GetSingletonEntity();
            }
        }

        public void SetTime(TimeData newTimeData)
        {
            EntityManager.SetComponentData(TimeSingleton, new WorldTime() {Time = newTimeData});
            m_CurrentTime = newTimeData;
        }

        public void PushTime(TimeData newTimeData)
        {
            var queue = EntityManager.GetBuffer<WorldTimeQueue>(TimeSingleton);
            queue.Add(new WorldTimeQueue() { Time = m_CurrentTime });
            SetTime(newTimeData);
        }

        public void PopTime()
        {
            var queue = EntityManager.GetBuffer<WorldTimeQueue>(TimeSingleton);

            Assert.IsTrue(queue.Length > 0, "PopTime without a matching PushTime");

            var prevTime = queue[queue.Length - 1];
            queue.RemoveAt(queue.Length - 1);
            SetTime(prevTime.Time);
        }

        // Internal system management

        ComponentSystemBase CreateSystemInternal(Type type)
        {
            var system = AllocateSystemInternal(type);
            AddSystem_Add_Internal(system);
            AddSystem_OnCreate_Internal(system);
            return system;
        }

        ComponentSystemBase AllocateSystemInternal(Type type)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!m_AllowGetSystem)
                throw new ArgumentException(
                    "During destruction of a system you are not allowed to create more systems.");
#endif
            return TypeManager.ConstructSystem(type);
        }

        ComponentSystemBase GetExistingSystemInternal(Type type)
        {
#if NET_DOTS
            for (int i = 0; i < m_Systems.Count; ++i)
            {
                var mgr = m_Systems[i];
                if (type.IsAssignableFrom(mgr.GetType()))
                    return mgr;
            }
#else
            ComponentSystemBase system;
            if (m_SystemLookup.TryGetValue(type, out system))
                return system;
#endif

            return null;
        }

        void AddTypeLookupInternal(Type type, ComponentSystemBase system)
        {
#if !UNITY_DOTSPLAYER
            while (type != typeof(ComponentSystemBase))
            {
                if (!m_SystemLookup.ContainsKey(type))
                    m_SystemLookup.Add(type, system);

                type = type.BaseType;
            }
#endif
        }

        void AddSystem_Add_Internal(ComponentSystemBase system)
        {
            m_Systems.Add(system);
            AddTypeLookupInternal(system.GetType(), system);
        }

        void AddSystem_OnCreate_Internal(ComponentSystemBase system)
        {
            try
            {
                system.CreateInstance(this);
            }
            catch
            {
                RemoveSystemInternal(system);
                throw;
            }
            ++Version;
        }

        void RemoveSystemInternal(ComponentSystemBase system)
        {
            if (!m_Systems.Remove(system))
                throw new ArgumentException($"System does not exist in the world");
            ++Version;

#if !UNITY_DOTSPLAYER
            var type = system.GetType();
            while (type != typeof(ComponentSystemBase))
            {
                if (m_SystemLookup[type] == system)
                {
                    m_SystemLookup.Remove(type);

                    foreach (var otherSystem in m_Systems)
                        if (otherSystem.GetType().IsSubclassOf(type))
                            AddTypeLookupInternal(otherSystem.GetType(), otherSystem);
                }

                type = type.BaseType;
            }
#endif
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckGetOrCreateSystem()
        {
            if (!IsCreated)
            {
                throw new ArgumentException("The World has already been Disposed.");
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!m_AllowGetSystem)
            {
                throw new ArgumentException("You are not allowed to get or create more systems during destruction of a system.");
            }
#endif
        }

        // Public system management

        public T GetOrCreateSystem<T>() where T : ComponentSystemBase
        {
            CheckGetOrCreateSystem();

            var system = GetExistingSystemInternal(typeof(T));
            return (T)(system ?? CreateSystemInternal(typeof(T)));
        }

        public ComponentSystemBase GetOrCreateSystem(Type type)
        {
            CheckGetOrCreateSystem();

            var system = GetExistingSystemInternal(type);
            return system ?? CreateSystemInternal(type);
        }

        public T CreateSystem<T>() where T : ComponentSystemBase, new()
        {
            CheckGetOrCreateSystem();

            return (T)CreateSystemInternal(typeof(T));
        }

        public ComponentSystemBase CreateSystem(Type type)
        {
            CheckGetOrCreateSystem();

            return CreateSystemInternal(type);
        }

        public T AddSystem<T>(T system) where T : ComponentSystemBase
        {
            CheckGetOrCreateSystem();
            if (GetExistingSystemInternal(system.GetType()) != null)
                throw new Exception($"Attempting to add system '{TypeManager.GetSystemName(system.GetType())}' which has already been added to world '{Name}'");

            AddSystem_Add_Internal(system);
            AddSystem_OnCreate_Internal(system);
            return system;
        }

        public T GetExistingSystem<T>() where T : ComponentSystemBase
        {
            CheckGetOrCreateSystem();

            return (T)GetExistingSystemInternal(typeof(T));
        }

        public ComponentSystemBase GetExistingSystem(Type type)
        {
            CheckGetOrCreateSystem();

            return GetExistingSystemInternal(type);
        }

        public void DestroySystem(ComponentSystemBase system)
        {
            CheckGetOrCreateSystem();

            RemoveSystemInternal(system);
            system.DestroyInstance();
        }

        public void DestroyAllSystemsAndLogException()
        {
            if (m_Systems == null)
                return;

            // Systems are destroyed in reverse order from construction, in three phases:
            // 1. Stop all systems from running (if they weren't already stopped), to ensure OnStopRunning() is called.
            // 2. Call each system's OnDestroy() method
            // 3. Actually destroy each system
            for (int i = m_Systems.Count - 1; i >= 0; --i)
            {
                try
                {
                    m_Systems[i].OnBeforeDestroyInternal();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            for (int i = m_Systems.Count - 1; i >= 0; --i)
            {
                try
                {
                    m_Systems[i].OnDestroy_Internal();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            for (int i = m_Systems.Count - 1; i >= 0; --i)
            {
                try
                {
                    m_Systems[i].OnAfterDestroyInternal();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            m_Systems.Clear();
            m_Systems = null;
        }

        internal void DestroyAllUnmanagedSystemsAndLogException()
        {
            for (int i = 0; i < m_UnmanagedSlots.Length; ++i)
            {
                FreeSlot(i);
            }

            m_UnmanagedSlots.Clear();
            m_UnmanagedState.Clear();
        }

        public ComponentSystemBase[] GetOrCreateSystemsAndLogException(Type[] types)
        {
            CheckGetOrCreateSystem();

            var toInitSystems = new ComponentSystemBase[types.Length];
            for (int i = 0; i != types.Length; i++)
            {
                try
                {
                    if (GetExistingSystemInternal(types[i]) != null)
                        continue;

                    var system = AllocateSystemInternal(types[i]);
                    if (system == null)
                        continue;

                    toInitSystems[i] = system;
                    AddSystem_Add_Internal(system);
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc);
                }
            }

            for (int i = 0; i != types.Length; i++)
            {
                if (toInitSystems[i] != null)
                {
                    try
                    {
                        AddSystem_OnCreate_Internal(toInitSystems[i]);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogException(exc);
                    }
                }
            }

            for (int i = 0; i != types.Length; i++)
                toInitSystems[i] = GetExistingSystemInternal(types[i]);

            return toInitSystems;
        }

        internal static int AllocateSystemID()
        {
            return ++ms_SystemIDAllocator;
        }

        public bool QuitUpdate { get; set; }

        public void Update()
        {
            GetExistingSystem<InitializationSystemGroup>()?.Update();
            GetExistingSystem<SimulationSystemGroup>()?.Update();
            GetExistingSystem<PresentationSystemGroup>()?.Update();

        #if ENABLE_UNITY_COLLECTIONS_CHECKS
            Assert.IsTrue(EntityManager.GetBuffer<WorldTimeQueue>(TimeSingleton).Length == 0, "PushTime without matching PopTime");
        #endif
        }

        /// <summary>
        /// Read only collection that doesn't generate garbage when used in a foreach.
        /// </summary>
        public struct NoAllocReadOnlyCollection<T> : IEnumerable<T>
        {
            readonly List<T> m_Source;

            public NoAllocReadOnlyCollection(List<T> source) => m_Source = source;

            public int Count => m_Source.Count;

            public T this[int index] => m_Source[index];

            public List<T>.Enumerator GetEnumerator() => m_Source.GetEnumerator();

            public bool Contains(T item) => m_Source.Contains(item);

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => throw new NotSupportedException($"To avoid boxing, do not cast {nameof(NoAllocReadOnlyCollection<T>)} to IEnumerable<T>.");
            IEnumerator IEnumerable.GetEnumerator()
                => throw new NotSupportedException($"To avoid boxing, do not cast {nameof(NoAllocReadOnlyCollection<T>)} to IEnumerable.");
        }

        //-----------------------------------------------------------------------------
        // Unmanaged stuff

        SystemSlot* ResolveSlot(SystemRefUntyped id)
        {
            // Nothing can resolve while we're shutting down.
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!m_AllowGetSystem)
                return null;
#endif

            int slot = id.m_Slot;
            int version = id.m_Version;

            // System ID is for a different world.
            if (id.m_WorldSeqNo != (uint)m_SequenceNumber)
                return null;

            // System ID is out of bounds.
            if (slot >= m_UnmanagedSlots.Length)
                return null;

            // Finally check version
            var slotPtr = m_UnmanagedSlots.Ptr + slot;
            return version == slotPtr->m_Version ? slotPtr : null;
        }

        // TODO: Make public when ISystemBase is exposed
        internal bool IsSystemValid(SystemRefUntyped id)
        {
            return ResolveSlot(id) != null;
        }

        internal ref T GetExistingUnmanagedSystem<T>() where T : struct, ISystemBase
        {
            if (m_UnmanagedSlotByTypeHash.TryGetFirstValue(BurstRuntime.GetHashCode64<T>(), out ushort slotIndex, out _))
            {
                var slotPtr = m_UnmanagedSlots.Ptr + slotIndex;

                return ref UnsafeUtilityEx.AsRef<T>(slotPtr->m_SystemPointer);
            }

            throw new InvalidOperationException("system does not exist");
        }

        // TODO: Make public when ISystemBase is exposed
        internal ref T ResolveSystem<T>(SystemRef<T> systemRef) where T : struct, ISystemBase
        {
            var ptr = ResolveSlot(systemRef);
            if (ptr == null)
                throw new InvalidOperationException("System reference is not valid");
            return ref UnsafeUtilityEx.AsRef<T>(ptr->m_SystemPointer);
        }

        internal SystemState* ResolveSystemUntyped(SystemRefUntyped r)
        {
            SystemSlot* ptr = ResolveSlot(r);
            if (ptr == null)
                return null;

            int index = (int)(ptr - m_UnmanagedSlots.Ptr);
            return m_UnmanagedState.Ptr + index;
        }

        private static void IncVersion(ref ushort v)
        {
            uint m = v;
            m += 1;
            m = (m >> 16) | m; // Fold overflow bit down to make 0xffff wrap to 0x0001, avoiding zero which is reserved for "unused"
            v = (ushort)m;
        }

        private int PrepareSlot(int slotIndex, int structSize, long typeHash)
        {
            var ptr = m_UnmanagedSlots.Ptr + slotIndex;
            var metaIndex = SystemBaseRegistry.GetSystemTypeMetaIndex(typeHash);

            ptr->m_SystemPointer = UnsafeUtility.Malloc(structSize, 16, Allocator.Persistent);
            UnsafeUtility.MemClear(ptr->m_SystemPointer, structSize);
            ptr->m_MetaIndex = metaIndex;
            ptr->m_TypeHash = typeHash;
            IncVersion(ref ptr->m_Version);

            var statePtr = m_UnmanagedState.Ptr + slotIndex;
            statePtr->Init(this, null);
            statePtr->m_UnmanagedMetaIndex = metaIndex;
            statePtr->m_SystemPtr = ptr->m_SystemPointer;

            ++Version;

            return slotIndex;
        }

        private int AllocSlot(int structSize, long typeHash)
        {
            // TODO: Replace with quicker approach
            var ptr = m_UnmanagedSlots.Ptr;
            for (int i = 0; i < m_UnmanagedSlots.Length; ++i)
            {
                if (ptr[i].m_SystemPointer == null)
                {
                    return PrepareSlot(i, structSize, typeHash);
                }
            }

            if (m_UnmanagedSlots.Length < ushort.MaxValue)
            {
                m_UnmanagedSlots.Add(default);
                m_UnmanagedState.Add(default);

                int index = m_UnmanagedSlots.Length - 1;
                return PrepareSlot(index, structSize, typeHash);
            }

            throw new InvalidOperationException("too many systems allocated");
        }

        private void FreeSlot(int index)
        {
            var ptr = m_UnmanagedSlots.Ptr + index;

            IncVersion(ref ptr->m_Version);

            if (ptr->m_SystemPointer != null)
            {
                try
                {
                    SystemBaseRegistry.CallOnDestroy(m_UnmanagedState.Ptr + index);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                UnsafeUtility.Free(ptr->m_SystemPointer, Allocator.Persistent);
            }

            m_UnmanagedState.Ptr[index].Dispose();

            ptr->m_SystemPointer = null;
            ++Version;
        }

        internal SystemRef<T> CreateSystemUnmanaged<T>(out SystemState* statePtr) where T : struct, ISystemBase
        {
            long typeHash = BurstRuntime.GetHashCode64<T>();
            int slotIndex = AllocSlot(UnsafeUtility.SizeOf<T>(), typeHash);
            var slotPtr = m_UnmanagedSlots.Ptr + slotIndex;

            try
            {
                statePtr = m_UnmanagedState.Ptr + slotIndex;
                SystemBaseRegistry.CallOnCreate(statePtr);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                FreeSlot(slotIndex);
                throw;
            }

            m_UnmanagedSlotByTypeHash.Add(typeHash, (ushort)slotIndex);

            return new SystemRef<T>((ushort)slotIndex, slotPtr->m_Version, (uint)m_SequenceNumber);
        }

        internal void DestroySystemUnmanaged(SystemRefUntyped sysRef)
        {
            SystemSlot* ptr = ResolveSlot(sysRef);
            if (ptr == null)
            {
                throw new InvalidOperationException("system does not exist");
            }

            long typeHash = ptr->m_TypeHash;
            int myIndex = (int)(ptr - m_UnmanagedSlots.Ptr);
            ptr = null;

            FreeSlot(myIndex);

            // TODO: Find other systems of same type in creation order, restore type lookup. Needed?
            m_UnmanagedSlotByTypeHash.Remove(typeHash, (ushort)myIndex);
        }

        internal ref T GetOrCreateUnmanagedSystem<T>() where T : struct, ISystemBase
        {
            if (m_UnmanagedSlotByTypeHash.ContainsKey(BurstRuntime.GetHashCode64<T>()))
            {
                return ref GetExistingUnmanagedSystem<T>();
            }
            else
            {
                CreateSystemUnmanaged<T>(out var ptr);
                return ref UnsafeUtilityEx.AsRef<T>(ptr);
            }
        }
    }

    // TODO: Make methods public once ISystemBase is ready for users
    public static class WorldExtensions
    {
        internal unsafe static SystemRef<T> AddSystem<T>(this World self) where T : struct, ISystemBase
        {
            return self.CreateSystemUnmanaged<T>(out _);
        }

        internal static ref T GetExistingSystem<T>(this World self) where T : struct, ISystemBase
        {
            return ref self.GetExistingUnmanagedSystem<T>();
        }

        internal static ref T GetOrCreateSystem<T>(this World self) where T : struct, ISystemBase
        {
            return ref self.GetOrCreateUnmanagedSystem<T>();
        }

        internal static void DestroySystem(this World self, SystemRefUntyped sysRef)
        {
            self.DestroySystemUnmanaged(sysRef);
        }
    }
}
