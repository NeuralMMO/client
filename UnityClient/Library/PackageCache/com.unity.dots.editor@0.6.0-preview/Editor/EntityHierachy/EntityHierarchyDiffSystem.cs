using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Unity.Entities.Editor
{
    [UsedImplicitly, DisableAutoCreation, ExecuteAlways] // ReSharper disable once RequiredBaseTypesIsNotInherited
    class EntityHierarchyDiffSystem : SystemBase
    {
        readonly HashSet<IEntityHierarchyGroupingStrategy> m_Strategies = new HashSet<IEntityHierarchyGroupingStrategy>();
        readonly Dictionary<ComponentType, DifferMapping> m_DifferMappingByComponentType = new Dictionary<ComponentType, DifferMapping>();
        readonly List<ComponentDataDiffer.ComponentChanges> m_ComponentDataDifferResults = new List<ComponentDataDiffer.ComponentChanges>();
        readonly List<ComponentDataDifferMapping> m_ComponentDataDiffers = new List<ComponentDataDifferMapping>();
        readonly List<SharedComponentDataDifferMapping> m_SharedComponentDataDiffers = new List<SharedComponentDataDifferMapping>();
        EntityDiffer m_EntityDiffer;

        public static void RegisterStrategy(IEntityHierarchyGroupingStrategy strategy)
        {
            var system = strategy.World.GetOrCreateSystem<EntityHierarchyDiffSystem>();
            system.Register(strategy);

            if (system.m_Strategies.Count == 1)
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<InitializationSystemGroup>().AddSystemToUpdateList(system);
        }

        public static void UnregisterStrategy(IEntityHierarchyGroupingStrategy strategy)
        {
            var system = strategy.World.GetExistingSystem<EntityHierarchyDiffSystem>();
            if (system == null)
                return;

            system.Unregister(strategy);
            if (system.m_Strategies.Count == 0)
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<InitializationSystemGroup>().RemoveSystemFromUpdateList(system);
        }

        void Register(IEntityHierarchyGroupingStrategy strategy)
        {
            if (!m_Strategies.Add(strategy))
                return;

            // Create the entity differ and enable the system when the first strategy is registered
            if (m_Strategies.Count == 1)
            {
                m_EntityDiffer = new EntityDiffer(World);
                Enabled = true;
            }

            // Go over requested differs and instantiate the one we're missing
            // And keep track of which strategy needs which differ
            foreach (var componentType in strategy.ComponentsToWatch)
            {
                if (!m_DifferMappingByComponentType.TryGetValue(componentType, out var differ))
                {
                    differ = DifferMapping.FromComponentType(componentType);
                    m_DifferMappingByComponentType.Add(componentType, differ);

                    switch (differ)
                    {
                        case ComponentDataDifferMapping componentDataDifferMapping:
                            m_ComponentDataDiffers.Add(componentDataDifferMapping);
                            break;
                        case SharedComponentDataDifferMapping sharedComponentDataDifferMapping:
                            m_SharedComponentDataDiffers.Add(sharedComponentDataDifferMapping);
                            break;
                    }
                }

                differ.Strategies.Add(strategy);
            }
        }

        void Unregister(IEntityHierarchyGroupingStrategy strategy)
        {
            if (!m_Strategies.Remove(strategy))
                return;

            // Dispose entity differ and disable system when the last system is unregistered
            if (m_Strategies.Count == 0)
            {
                m_EntityDiffer.Dispose();
                Assert.IsTrue(Enabled);
                Enabled = false;
            }

            // Dispose differs we don't need anymore
            foreach (var componentType in strategy.ComponentsToWatch)
            {
                var differ = m_DifferMappingByComponentType[componentType];
                differ.Strategies.Remove(strategy);
                if (differ.Strategies.Count == 0)
                {
                    m_DifferMappingByComponentType.Remove(componentType);
                    switch (differ)
                    {
                        case ComponentDataDifferMapping componentDataDifferMapping:
                            m_ComponentDataDiffers.Remove(componentDataDifferMapping);
                            break;
                        case SharedComponentDataDifferMapping sharedComponentDataDifferMapping:
                            m_SharedComponentDataDiffers.Remove(sharedComponentDataDifferMapping);
                            break;
                    }

                    differ.Dispose();
                }
            }
        }

        protected override void OnUpdate()
        {
            var query = World.EntityManager.UniversalQuery;

            var newEntities = new NativeList<Entity>(Allocator.TempJob);
            var removedEntities = new NativeList<Entity>(Allocator.TempJob);
            var componentDifferResultIndices = new NativeHashMap<ComponentType, int>(m_ComponentDataDiffers.Count, Allocator.Temp);
            var handles = new NativeArray<JobHandle>(m_ComponentDataDiffers.Count + 1, Allocator.Temp);

            for (var i = 0; i < m_ComponentDataDiffers.Count; i++)
            {
                var componentDataDiffer = m_ComponentDataDiffers[i];
                componentDifferResultIndices[componentDataDiffer.ComponentType] = i;
                m_ComponentDataDifferResults.Add(componentDataDiffer.Differ.GatherComponentChangesAsync(query, Allocator.TempJob, out var componentDataDifferHandle));
                handles[i] = componentDataDifferHandle;
            }
            handles[handles.Length - 1] = m_EntityDiffer.GetEntityQueryMatchDiffAsync(query, newEntities, removedEntities);
            JobHandle.CompleteAll(handles);
            handles.Dispose();

            foreach (var strategy in m_Strategies)
            {
                strategy.ApplyEntityChanges(newEntities, removedEntities);
            }

            foreach (var componentDataDiffer in m_ComponentDataDiffers)
            {
                var resultIdx = componentDifferResultIndices[componentDataDiffer.ComponentType];
                var result = m_ComponentDataDifferResults[resultIdx];
                componentDataDiffer.Apply(result);
                result.Dispose();
            }

            foreach (var sharedComponentDataDiffer in m_SharedComponentDataDiffers)
            {
                var result = sharedComponentDataDiffer.Differ.GatherComponentChanges(World.EntityManager, query, Allocator.TempJob);
                sharedComponentDataDiffer.Apply(result);
                result.Dispose();
            }

            newEntities.Dispose();
            removedEntities.Dispose();
            componentDifferResultIndices.Dispose();
            m_ComponentDataDifferResults.Clear();
        }

        abstract class DifferMapping : IDisposable
        {
            public readonly List<IEntityHierarchyGroupingStrategy> Strategies = new List<IEntityHierarchyGroupingStrategy>();

            public static DifferMapping FromComponentType(ComponentType componentType)
            {
                var typeInfo = TypeManager.GetTypeInfo(componentType.TypeIndex);

                if (typeInfo.Category == TypeManager.TypeCategory.ComponentData || UnsafeUtility.IsUnmanaged(componentType.GetManagedType()))
                    return new ComponentDataDifferMapping(componentType, new ComponentDataDiffer(componentType));

                if (typeInfo.Category == TypeManager.TypeCategory.ISharedComponentData)
                    return new SharedComponentDataDifferMapping(new SharedComponentDataDiffer(componentType));

                throw new ArgumentException($"There is no suitable differ available for this category of component {typeInfo.Category} " +
                    $"(is unmanaged: {UnsafeUtility.IsUnmanaged(componentType.GetManagedType())})", nameof(componentType));
            }

            public abstract void Dispose();
        }

        class ComponentDataDifferMapping : DifferMapping
        {
            public readonly ComponentType ComponentType;
            public readonly ComponentDataDiffer Differ;

            public ComponentDataDifferMapping(ComponentType componentType, ComponentDataDiffer differ)
                => (ComponentType, Differ) = (componentType, differ);

            public void Apply(ComponentDataDiffer.ComponentChanges changes)
            {
                foreach (var strategy in Strategies)
                {
                    strategy.ApplyComponentDataChanges(changes);
                }
            }

            public override void Dispose()
                => Differ.Dispose();
        }

        class SharedComponentDataDifferMapping : DifferMapping
        {
            public readonly SharedComponentDataDiffer Differ;

            public SharedComponentDataDifferMapping(SharedComponentDataDiffer differ)
                => Differ = differ;

            public void Apply(SharedComponentDataDiffer.ComponentChanges changes)
            {
                foreach (var strategy in Strategies)
                {
                    strategy.ApplySharedComponentDataChanges(changes);
                }
            }

            public override void Dispose()
                => Differ.Dispose();
        }
    }
}
