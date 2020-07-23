using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        public JobHandle ExclusiveEntityTransactionDependency
        {
            get
            {
                // Note this can't use read/write checking
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckExistsAndThrow(m_Safety);
                #endif
                return m_EntityDataAccess->DependencyManager->ExclusiveTransactionDependency;
            }
            set
            {
                // Note this can't use read/write checking
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckExistsAndThrow(m_Safety);
                #endif
                m_EntityDataAccess->DependencyManager->ExclusiveTransactionDependency = value;
            }
        }

        /// <summary>
        /// Begins an exclusive entity transaction, which allows you to make structural changes inside a Job.
        /// </summary>
        /// <remarks>
        /// <see cref="ExclusiveEntityTransaction"/> allows you to create & destroy entities from a job. The purpose is
        /// to enable procedural generation scenarios where instantiation on big scale must happen on jobs. As the
        /// name implies it is exclusive to any other access to the EntityManager.
        ///
        /// An exclusive entity transaction should be used on a manually created <see cref="World"/> that acts as a
        /// staging area to construct and setup entities.
        ///
        /// After the job has completed you can end the transaction and use
        /// <see cref="MoveEntitiesFrom(EntityManager)"/> to move the entities to an active <see cref="World"/>.
        /// </remarks>
        /// <returns>A transaction object that provides an functions for making structural changes.</returns>
        public ExclusiveEntityTransaction BeginExclusiveEntityTransaction()
        {
            var access = GetCheckedEntityDataAccess();

            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_JobMode) throw new InvalidOperationException();
            #endif

            access->DependencyManager->BeginExclusiveTransaction();

            var copy = this;

            #if ENABLE_UNITY_COLLECTIONS_CHECKS
            access->m_JobMode = true;
            copy.m_JobMode = true;
            #endif
            return new ExclusiveEntityTransaction(copy);
        }

        /// <summary>
        /// Ends an exclusive entity transaction.
        /// </summary>
        /// <seealso cref="ExclusiveEntityTransaction"/>
        /// <seealso cref="BeginExclusiveEntityTransaction()"/>
        public void EndExclusiveEntityTransaction()
        {
        #if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_JobMode)
                throw new InvalidOperationException("Transactions can only be ended from the main thread");

            AtomicSafetyHandle.CheckExistsAndThrow(m_Safety);

            m_EntityDataAccess->DependencyManager->PreEndExclusiveTransaction();

            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);

            m_EntityDataAccess->DependencyManager->EndExclusiveTransaction();

            m_EntityDataAccess->m_JobMode = false;
        #endif
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

        internal void AllocateConsecutiveEntitiesForLoading(int count)
        {
            EntityComponentStore* s = GetCheckedEntityDataAccess()->EntityComponentStore;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (s->CountEntities() != 0)
                throw new ArgumentException("loading into non-empty entity manager is not supported");
#endif
            s->AllocateConsecutiveEntitiesForLoading(count);
        }

        internal void AddSharedComponent<T>(NativeArray<ArchetypeChunk> chunks, T componentData)
            where T : struct, ISharedComponentData
        {
            ManagedComponentStore mcs = GetCheckedEntityDataAccess()->ManagedComponentStore;
            var componentType = ComponentType.ReadWrite<T>();
            int sharedComponentIndex = mcs.InsertSharedComponent(componentData);
            m_EntityDataAccess->AddSharedComponentData(chunks, sharedComponentIndex, componentType);
            mcs.RemoveReference(sharedComponentIndex);
        }
    }
}
