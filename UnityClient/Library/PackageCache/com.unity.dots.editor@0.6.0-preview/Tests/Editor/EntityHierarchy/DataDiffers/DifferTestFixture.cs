using System;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;

namespace Unity.Entities.Editor.Tests
{
    abstract class DifferTestFixture
    {
        World m_World;

        protected World World => m_World;

        [SetUp]
        public virtual void Setup()
        {
            m_World = new World("TestWorld");
        }

        [TearDown]
        public virtual void Teardown()
        {
            m_World.Dispose();
        }

        protected void CreateEntitiesWithMockSharedComponentData(int count, params ComponentType[] components)
            => CreateEntitiesWithMockSharedComponentData(count, Allocator.TempJob, components).Dispose();

        protected NativeArray<Entity> CreateEntitiesWithMockSharedComponentData(int count, Allocator allocator, params ComponentType[] components)
            => CreateEntitiesWithMockSharedComponentData(count, allocator, null, components);

        protected NativeArray<Entity> CreateEntitiesWithMockSharedComponentData(int count, Allocator allocator, Func<int, int> sharedComponentValueProvider, params ComponentType[] components)
        {
            var archetype = m_World.EntityManager.CreateArchetype(components);
            var entities = m_World.EntityManager.CreateEntity(archetype, count, allocator);

            if (components.Any(t => t == typeof(EcsTestSharedComp)))
            {
                for (var i = 0; i < count; i++)
                {
                    World.EntityManager.SetSharedComponentData(entities[i], new EcsTestSharedComp { value = sharedComponentValueProvider?.Invoke(i) ?? i / 31 });
                }
            }

            return entities;
        }
    }
}
