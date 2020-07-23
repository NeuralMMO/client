#if ENABLE_UNITY_COLLECTIONS_CHECKS
using System.Text;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Profiling;

namespace Unity.Entities
{
    unsafe struct ComponentSafetyHandles
    {
        const int              kMaxTypes = TypeManager.MaximumTypesCount;

        ComponentSafetyHandle* m_ComponentSafetyHandles;
        ushort                 m_ComponentSafetyHandlesCount;
        const int              EntityTypeIndex = 1;

        ushort*                m_TypeArrayIndices;
        const ushort           NullTypeIndex = 0xFFFF;
#if UNITY_2020_1_OR_NEWER
        // Per-component-type Static safety IDs are shared across all Worlds.
        static int* m_StaticSafetyIdsForComponentDataFromEntity;
        static int* m_StaticSafetyIdsForArchetypeChunkArrays;
        static int m_StaticSafetyIdForArchetypeChunkComponentTypeDynamic = 0;
        static int m_StaticSafetyIdForArchetypeChunkEntityType = 0;
        static byte[] m_CustomDeallocatedErrorMessageBytes = Encoding.UTF8.GetBytes("Attempted to access {5} which has been invalidated by a structural change.");
        static byte[] m_CustomDeallocatedFromJobErrorMessageBytes = Encoding.UTF8.GetBytes("Attempted to access the {5} {3} which has been invalidated by a structural change.");
        public void SetCustomErrorMessage(int staticSafetyId, AtomicSafetyErrorType errorType, byte[] messageBytes)
        {
            fixed(byte* pBytes = messageBytes)
            {
                AtomicSafetyHandle.SetCustomErrorMessage(staticSafetyId, errorType, pBytes, messageBytes.Length);
            }
        }

        int CreateStaticSafetyId(string ownerTypeName)
        {
            int staticSafetyId = 0;

            byte[] ownerNameByteArray = Encoding.UTF8.GetBytes(ownerTypeName);
            fixed(byte* ownerTypeNameBytes = ownerNameByteArray)
            {
                staticSafetyId = AtomicSafetyHandle.NewStaticSafetyId(ownerTypeNameBytes, ownerNameByteArray.Length);
            }

            SetCustomErrorMessage(staticSafetyId, AtomicSafetyErrorType.Deallocated, m_CustomDeallocatedErrorMessageBytes);
            SetCustomErrorMessage(staticSafetyId, AtomicSafetyErrorType.DeallocatedFromJob, m_CustomDeallocatedFromJobErrorMessageBytes);

            return staticSafetyId;
        }

        [BurstDiscard]
        void CreateStaticSafetyIdsForType(int typeIndex)
        {
            var typeIndexWithoutFlags = typeIndex & TypeManager.ClearFlagsMask;
            if (m_StaticSafetyIdsForComponentDataFromEntity[typeIndexWithoutFlags] == 0)
            {
                if (TypeManager.IsBuffer(typeIndex))
                {
                    m_StaticSafetyIdsForComponentDataFromEntity[typeIndexWithoutFlags] =
                        CreateStaticSafetyId(
                            "BufferFromEntity<" + TypeManager.GetTypeInfo(typeIndex).Debug.TypeName + ">");
                }
                else
                {
                    m_StaticSafetyIdsForComponentDataFromEntity[typeIndexWithoutFlags] =
                        CreateStaticSafetyId(
                            "ComponentDataFromEntity<" + TypeManager.GetTypeInfo(typeIndex).Debug.TypeName + ">");
                }
            }
            if (m_StaticSafetyIdsForArchetypeChunkArrays[typeIndexWithoutFlags] == 0)
            {
                if (TypeManager.IsBuffer(typeIndex))
                {
                    m_StaticSafetyIdsForArchetypeChunkArrays[typeIndexWithoutFlags] =
                        CreateStaticSafetyId(
                            "ArchetypeChunkBufferType<" + TypeManager.GetTypeInfo(typeIndex).Debug.TypeName + ">");
                }
                else if (TypeManager.IsSharedComponent(typeIndex))
                {
                    m_StaticSafetyIdsForArchetypeChunkArrays[typeIndexWithoutFlags] =
                        CreateStaticSafetyId(
                            "ArchetypeChunkSharedComponentType<" + TypeManager.GetTypeInfo(typeIndex).Debug.TypeName + ">");
                }
                else
                {
                    m_StaticSafetyIdsForArchetypeChunkArrays[typeIndexWithoutFlags] =
                        CreateStaticSafetyId(
                            "ArchetypeChunkComponentType<" + TypeManager.GetTypeInfo(typeIndex).Debug.TypeName + ">");
                }
            }
        }

        [BurstDiscard]
        private void SetStaticSafetyIdForHandle_ArchetypeChunk(ref AtomicSafetyHandle handle, int typeIndex, bool dynamic)
        {
            // Configure safety handle static safety ID for ArchetypeChunk*Type by default
            int typeIndexWithoutFlags = typeIndex & TypeManager.ClearFlagsMask;
            int staticSafetyId = 0;
            if (dynamic)
                staticSafetyId = m_StaticSafetyIdForArchetypeChunkComponentTypeDynamic;
            else if (typeIndex == EntityTypeIndex)
                staticSafetyId = m_StaticSafetyIdForArchetypeChunkEntityType;
            else
                staticSafetyId = m_StaticSafetyIdsForArchetypeChunkArrays[typeIndexWithoutFlags];
            AtomicSafetyHandle.SetStaticSafetyId(ref handle, staticSafetyId);
        }

        [BurstDiscard]
        private void SetStaticSafetyIdForHandle_FromEntity(ref AtomicSafetyHandle handle, int typeIndex)
        {
            // Configure safety handle static safety ID for ArchetypeChunk*Type by default
            int typeIndexWithoutFlags = typeIndex & TypeManager.ClearFlagsMask;
            int staticSafetyId = m_StaticSafetyIdsForComponentDataFromEntity[typeIndexWithoutFlags];
            AtomicSafetyHandle.SetStaticSafetyId(ref handle, staticSafetyId);
        }

#endif
        ushort GetTypeArrayIndex(int typeIndex)
        {
            var typeIndexWithoutFlags = typeIndex & TypeManager.ClearFlagsMask;
            var arrayIndex = m_TypeArrayIndices[typeIndexWithoutFlags];
            if (arrayIndex != NullTypeIndex)
                return arrayIndex;

            arrayIndex = m_ComponentSafetyHandlesCount++;
            m_TypeArrayIndices[typeIndexWithoutFlags] = arrayIndex;
            m_ComponentSafetyHandles[arrayIndex].TypeIndex = typeIndex;
            m_ComponentSafetyHandles[arrayIndex].IsSafetyHandleOwner = typeIndex == EntityTypeIndex || !TypeManager.IsZeroSized(typeIndex);

            if (m_ComponentSafetyHandles[arrayIndex].IsSafetyHandleOwner)
                m_ComponentSafetyHandles[arrayIndex].SafetyHandle = AtomicSafetyHandle.Create();
            else
                m_ComponentSafetyHandles[arrayIndex].SafetyHandle = m_ComponentSafetyHandles[GetTypeArrayIndex(EntityTypeIndex)].SafetyHandle;

            AtomicSafetyHandle.SetAllowSecondaryVersionWriting(m_ComponentSafetyHandles[arrayIndex].SafetyHandle, false);
            m_ComponentSafetyHandles[arrayIndex].BufferHandle = AtomicSafetyHandle.Create();

#if !NET_DOTS // todo: enable when this is supported
            AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(m_ComponentSafetyHandles[arrayIndex].BufferHandle, true);
#endif

#if UNITY_2020_1_OR_NEWER
            // Create static safety IDs for this type if they don't already exist.
            CreateStaticSafetyIdsForType(typeIndex);
            // Set default static safety IDs for handles
            SetStaticSafetyIdForHandle_ArchetypeChunk(ref m_ComponentSafetyHandles[arrayIndex].SafetyHandle, typeIndex, false);
            SetStaticSafetyIdForHandle_ArchetypeChunk(ref m_ComponentSafetyHandles[arrayIndex].BufferHandle, typeIndex, false);
#endif
            return arrayIndex;
        }

        void ClearAllTypeArrayIndices()
        {
            for (int i = 0; i < m_ComponentSafetyHandlesCount; ++i)
                m_TypeArrayIndices[m_ComponentSafetyHandles[i].TypeIndex & TypeManager.ClearFlagsMask] = NullTypeIndex;
            m_ComponentSafetyHandlesCount = 0;
        }

        public void OnCreate()
        {
            m_TypeArrayIndices = (ushort*)UnsafeUtility.Malloc(sizeof(ushort) * kMaxTypes, 16, Allocator.Persistent);
            UnsafeUtility.MemSet(m_TypeArrayIndices, 0xFF, sizeof(ushort) * kMaxTypes);

            m_ComponentSafetyHandles = (ComponentSafetyHandle*)UnsafeUtility.Malloc(sizeof(ComponentSafetyHandle) * kMaxTypes, 16, Allocator.Persistent);
            UnsafeUtility.MemClear(m_ComponentSafetyHandles, sizeof(ComponentSafetyHandle) * kMaxTypes);

            m_TempSafety = AtomicSafetyHandle.Create();
            m_ComponentSafetyHandlesCount = 0;

#if UNITY_2020_1_OR_NEWER
            if (m_StaticSafetyIdsForComponentDataFromEntity == null)
            {
                m_StaticSafetyIdsForComponentDataFromEntity =
                    (int*)UnsafeUtility.Malloc(sizeof(int) * kMaxTypes, 16, Allocator.Persistent);
                UnsafeUtility.MemClear(m_StaticSafetyIdsForComponentDataFromEntity, sizeof(int) * kMaxTypes);
            }
            if (m_StaticSafetyIdsForArchetypeChunkArrays == null)
            {
                m_StaticSafetyIdsForArchetypeChunkArrays =
                    (int*)UnsafeUtility.Malloc(sizeof(int) * kMaxTypes, 16, Allocator.Persistent);
                UnsafeUtility.MemClear(m_StaticSafetyIdsForArchetypeChunkArrays, sizeof(int) * kMaxTypes);
            }

            m_StaticSafetyIdForArchetypeChunkComponentTypeDynamic = AtomicSafetyHandle.NewStaticSafetyId<ArchetypeChunkComponentTypeDynamic>();
            SetCustomErrorMessage(m_StaticSafetyIdForArchetypeChunkComponentTypeDynamic, AtomicSafetyErrorType.Deallocated,
                m_CustomDeallocatedErrorMessageBytes);
            SetCustomErrorMessage(m_StaticSafetyIdForArchetypeChunkComponentTypeDynamic, AtomicSafetyErrorType.DeallocatedFromJob,
                m_CustomDeallocatedFromJobErrorMessageBytes);

            m_StaticSafetyIdForArchetypeChunkEntityType = AtomicSafetyHandle.NewStaticSafetyId<ArchetypeChunkEntityType>();
            SetCustomErrorMessage(m_StaticSafetyIdForArchetypeChunkEntityType, AtomicSafetyErrorType.Deallocated,
                m_CustomDeallocatedErrorMessageBytes);
            SetCustomErrorMessage(m_StaticSafetyIdForArchetypeChunkEntityType, AtomicSafetyErrorType.DeallocatedFromJob,
                m_CustomDeallocatedFromJobErrorMessageBytes);
#endif
        }

        public AtomicSafetyHandle ExclusiveTransactionSafety;

        public void CompleteAllJobsAndInvalidateArrays()
        {
            if (m_ComponentSafetyHandlesCount == 0)
                return;

            Profiler.BeginSample("InvalidateArrays");
            for (var i = 0; i != m_ComponentSafetyHandlesCount; i++)
            {
                AtomicSafetyHandle.CheckDeallocateAndThrow(m_ComponentSafetyHandles[i].SafetyHandle);
                AtomicSafetyHandle.CheckDeallocateAndThrow(m_ComponentSafetyHandles[i].BufferHandle);
            }

            for (var i = 0; i != m_ComponentSafetyHandlesCount; i++)
            {
                if (m_ComponentSafetyHandles[i].IsSafetyHandleOwner)
                    AtomicSafetyHandle.Release(m_ComponentSafetyHandles[i].SafetyHandle);
                AtomicSafetyHandle.Release(m_ComponentSafetyHandles[i].BufferHandle);
            }

            ClearAllTypeArrayIndices();
            Profiler.EndSample();
        }

        public void Dispose()
        {
            for (var i = 0; i < m_ComponentSafetyHandlesCount; i++)
            {
                EnforceJobResult res0 = EnforceJobResult.AllJobsAlreadySynced;
                if (m_ComponentSafetyHandles[i].IsSafetyHandleOwner)
                    res0 = AtomicSafetyHandle.EnforceAllBufferJobsHaveCompletedAndRelease(m_ComponentSafetyHandles[i].SafetyHandle);
                var res1 = AtomicSafetyHandle.EnforceAllBufferJobsHaveCompletedAndRelease(m_ComponentSafetyHandles[i].BufferHandle);

                if (res0 == EnforceJobResult.DidSyncRunningJobs || res1 == EnforceJobResult.DidSyncRunningJobs)
                    Debug.LogError(
                        "Disposing EntityManager but a job is still running against the ComponentData. It appears the job has not been registered with JobComponentSystem.AddDependency.");
            }

            AtomicSafetyHandle.Release(m_TempSafety);

            UnsafeUtility.Free(m_TypeArrayIndices, Allocator.Persistent);
            UnsafeUtility.Free(m_ComponentSafetyHandles, Allocator.Persistent);
            m_ComponentSafetyHandles = null;
        }

        public void PreDisposeCheck()
        {
            for (var i = 0; i < m_ComponentSafetyHandlesCount; i++)
            {
                var res0 = AtomicSafetyHandle.EnforceAllBufferJobsHaveCompleted(m_ComponentSafetyHandles[i].SafetyHandle);
                var res1 = AtomicSafetyHandle.EnforceAllBufferJobsHaveCompleted(m_ComponentSafetyHandles[i].BufferHandle);
                if (res0 == EnforceJobResult.DidSyncRunningJobs || res1 == EnforceJobResult.DidSyncRunningJobs)
                    Debug.LogError(
                        "Disposing EntityManager but a job is still running against the ComponentData. It appears the job has not been registered with JobComponentSystem.AddDependency.");
            }
        }

        public void CompleteWriteDependency(int type)
        {
            var typeIndexWithoutFlags = type & TypeManager.ClearFlagsMask;
            var arrayIndex = m_TypeArrayIndices[typeIndexWithoutFlags];
            if (arrayIndex == NullTypeIndex)
                return;

            AtomicSafetyHandle.CheckReadAndThrow(m_ComponentSafetyHandles[arrayIndex].SafetyHandle);
            AtomicSafetyHandle.CheckReadAndThrow(m_ComponentSafetyHandles[arrayIndex].BufferHandle);
        }

        public void CompleteReadAndWriteDependency(int type)
        {
            var typeIndexWithoutFlags = type & TypeManager.ClearFlagsMask;
            var arrayIndex = m_TypeArrayIndices[typeIndexWithoutFlags];
            if (arrayIndex == NullTypeIndex)
                return;

            AtomicSafetyHandle.CheckWriteAndThrow(m_ComponentSafetyHandles[arrayIndex].SafetyHandle);
            AtomicSafetyHandle.CheckWriteAndThrow(m_ComponentSafetyHandles[arrayIndex].BufferHandle);
        }

        public AtomicSafetyHandle GetEntityManagerSafetyHandle()
        {
            var handle = m_ComponentSafetyHandles[GetTypeArrayIndex(EntityTypeIndex)].SafetyHandle;
            AtomicSafetyHandle.UseSecondaryVersion(ref handle);
            return handle;
        }

        public AtomicSafetyHandle GetSafetyHandleForComponentDataFromEntity(int type, bool isReadOnly)
        {
            var handle = GetSafetyHandle(type, isReadOnly);
#if UNITY_2020_1_OR_NEWER
            SetStaticSafetyIdForHandle_FromEntity(ref handle, type);
#endif
            return handle;
        }

        public AtomicSafetyHandle GetBufferHandleForBufferFromEntity(int type)
        {
            Assert.IsTrue(TypeManager.IsBuffer(type));
            var handle = GetBufferSafetyHandle(type);
#if UNITY_2020_1_OR_NEWER
            SetStaticSafetyIdForHandle_FromEntity(ref handle, type);
#endif
            return handle;
        }

        public AtomicSafetyHandle GetSafetyHandleForArchetypeChunkComponentType(int type, bool isReadOnly)
        {
            // safety handles are configured with the static safety ID for ArchetypeChunk*Type by default,
            // so no further static safety ID setup is necessary in this path.
            return GetSafetyHandle(type, isReadOnly);
        }

        public AtomicSafetyHandle GetSafetyHandleForArchetypeChunkComponentTypeDynamic(int type, bool isReadOnly)
        {
            var handle = GetSafetyHandle(type, isReadOnly);
#if UNITY_2020_1_OR_NEWER
            SetStaticSafetyIdForHandle_ArchetypeChunk(ref handle, type, true);
#endif
            return handle;
        }

        public AtomicSafetyHandle GetSafetyHandleForArchetypeChunkBufferType(int type, bool isReadOnly)
        {
            // safety handles are configured with the static safety ID for ArchetypeChunk*Type by default,
            // so no further static safety ID setup is necessary in this path.
            return GetSafetyHandle(type, isReadOnly);
        }

        public AtomicSafetyHandle GetBufferHandleForArchetypeChunkBufferType(int type)
        {
            Assert.IsTrue(TypeManager.IsBuffer(type));
            // safety handles are configured with the static safety ID for ArchetypeChunk*Type by default,
            // so no further static safety ID setup is necessary in this path.
            return GetBufferSafetyHandle(type);
        }

        public AtomicSafetyHandle GetSafetyHandleForArchetypeChunkSharedComponentType(int type)
        {
            Assert.IsTrue(TypeManager.IsSharedComponent(type));
            var handle = GetEntityManagerSafetyHandle();
#if UNITY_2020_1_OR_NEWER
            // the static safety ID for this type may not exist yet, since this path doesn't call GetTypeArrayIndex()
            // this is fixed by https://github.com/Unity-Technologies/dots/pull/4879
            CreateStaticSafetyIdsForType(type);
            SetStaticSafetyIdForHandle_ArchetypeChunk(ref handle, type, false);
#endif
            return handle;
        }

        public AtomicSafetyHandle GetSafetyHandleForArchetypeChunkEntityType()
        {
            var handle = GetEntityManagerSafetyHandle();
            // The EntityTypeIndex safety handle is pre-configured with a static safety ID for ArchetypeChunkEntityType,
            // so no further configuration is necessary here.
            return handle;
        }

        public AtomicSafetyHandle GetSafetyHandle(int type, bool isReadOnly)
        {
            var arrayIndex = GetTypeArrayIndex(type);
            var handle = m_ComponentSafetyHandles[arrayIndex].SafetyHandle;
            if (isReadOnly)
                AtomicSafetyHandle.UseSecondaryVersion(ref handle);
            return handle;
        }

        public AtomicSafetyHandle GetBufferSafetyHandle(int type)
        {
            Assert.IsTrue(TypeManager.IsBuffer(type));
            var arrayIndex = GetTypeArrayIndex(type);
            return m_ComponentSafetyHandles[arrayIndex].BufferHandle;
        }

        public void BeginExclusiveTransaction()
        {
            for (var i = 0; i != m_ComponentSafetyHandlesCount; i++)
            {
                AtomicSafetyHandle.CheckDeallocateAndThrow(m_ComponentSafetyHandles[i].SafetyHandle);
                AtomicSafetyHandle.CheckDeallocateAndThrow(m_ComponentSafetyHandles[i].BufferHandle);
            }

            for (var i = 0; i != m_ComponentSafetyHandlesCount; i++)
            {
                if (m_ComponentSafetyHandles[i].IsSafetyHandleOwner)
                    AtomicSafetyHandle.Release(m_ComponentSafetyHandles[i].SafetyHandle);
                AtomicSafetyHandle.Release(m_ComponentSafetyHandles[i].BufferHandle);
            }

            ExclusiveTransactionSafety = AtomicSafetyHandle.Create();
            ClearAllTypeArrayIndices();
        }

        public void EndExclusiveTransaction()
        {
            var res = AtomicSafetyHandle.EnforceAllBufferJobsHaveCompletedAndRelease(ExclusiveTransactionSafety);
            if (res != EnforceJobResult.AllJobsAlreadySynced)
                //@TODO: Better message
                Debug.LogError("ExclusiveEntityTransaction job has not been registered");
        }

        struct ComponentSafetyHandle
        {
            public AtomicSafetyHandle SafetyHandle;
            public AtomicSafetyHandle BufferHandle;
            public int                TypeIndex;
            // if false, SafetyHandle is based on a copy of the handle for EntityTypeIndex, and should not be
            // deleted during destroy-all-handles operations. BufferHandle is always a unique handle per-type.
            public bool               IsSafetyHandleOwner;
        }

        AtomicSafetyHandle m_TempSafety;
    }
}
#endif
