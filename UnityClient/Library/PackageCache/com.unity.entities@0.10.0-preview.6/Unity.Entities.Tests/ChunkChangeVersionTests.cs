using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities.Tests
{
    class ChunkChangeVersionTests : ECSTestsFixture
    {
        const uint OldVersion = 42;
        const uint NewVersion = 43;

        public override void Setup()
        {
            base.Setup();
            m_Manager.Debug.SetGlobalSystemVersion(OldVersion);
        }

        void BumpGlobalSystemVersion()
        {
            m_Manager.Debug.SetGlobalSystemVersion(NewVersion);
        }

        [Test]
        public void VersionWrapAround()
        {
            var firstSystemFrame = 0U;
            var initial = ChangeVersionUtility.InitialGlobalSystemVersion;
            var lastVersion = uint.MaxValue;
            var lastVersionPlus = lastVersion;
            ChangeVersionUtility.IncrementGlobalSystemVersion(ref lastVersionPlus);

            // In order to support wrap around we wrap numbers
            Assert.IsTrue(ChangeVersionUtility.DidChange(initial + 1, initial));
            Assert.IsTrue(ChangeVersionUtility.DidChange(lastVersion / 2 - 10U, initial));
            Assert.IsFalse(ChangeVersionUtility.DidChange(lastVersion / 2 + 10U, initial));
            Assert.IsFalse(ChangeVersionUtility.DidChange(lastVersion, initial));
            Assert.IsFalse(ChangeVersionUtility.DidChange(initial, initial));

            // Wrap around
            Assert.IsTrue(ChangeVersionUtility.DidChange(lastVersionPlus, lastVersion));
            Assert.IsTrue(ChangeVersionUtility.DidChange(lastVersionPlus, lastVersion - 1000));
            Assert.IsFalse(ChangeVersionUtility.DidChange(lastVersionPlus, 10));

            // first frame is always changed
            Assert.IsTrue(ChangeVersionUtility.DidChange(initial, firstSystemFrame));
            Assert.IsTrue(ChangeVersionUtility.DidChange(lastVersion, firstSystemFrame));
            Assert.IsTrue(ChangeVersionUtility.DidChange(lastVersion / 2, firstSystemFrame));
        }

        // Version Change Case 1:
        //   - Component ChangeVersion: All ComponentType(s) in archetype set to GlobalChangeVersion
        //   - Chunk OrderVersion: Destination chunk version set to GlobalChangeVersion.
        //   - Sources:
        //     - AddExistingChunk
        //     - AddEmptyChunk
        //     - Allocate
        //     - AllocateClone
        //     - MoveArchetype
        //     - RemapAllArchetypesJob (direct access GetChangeVersionArrayForType)

        [Test]
        public void Allocate_Via_CreateEntity()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChunkOrderVersion(e0, OldVersion);

            BumpGlobalSystemVersion();

            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));

            AssertSameChunk(e0, e1);
            AssetHasChangeVersion<EcsTestData>(e1, NewVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void Allocate_Via_CreateEntity_ManagedComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestManagedComponent2));
            AssetHasChangeVersion<EcsTestManagedComponent>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestManagedComponent2>(e0, OldVersion);
            AssetHasChunkOrderVersion(e0, OldVersion);

            BumpGlobalSystemVersion();

            var e1 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestManagedComponent2));

            AssertSameChunk(e0, e1);
            AssetHasChangeVersion<EcsTestManagedComponent>(e1, NewVersion);
            AssetHasChangeVersion<EcsTestManagedComponent2>(e1, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#endif

        [Test]
        public void AddEmptyChunk_Via_CreateEntity()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChunkOrderVersion(e0, OldVersion);

            BumpGlobalSystemVersion();

            var e1 = m_Manager.CreateEntity(typeof(EcsTestData3));

            AssetHasChangeVersion<EcsTestData3>(e1, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void AddEmptyChunk_Via_CreateEntity_ManagedComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestManagedComponent2));
            AssetHasChangeVersion<EcsTestManagedComponent>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestManagedComponent2>(e0, OldVersion);
            AssetHasChunkOrderVersion(e0, OldVersion);

            BumpGlobalSystemVersion();

            var e1 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent3));

            AssetHasChangeVersion<EcsTestManagedComponent3>(e1, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#endif

        // Version Change Case 2:
        //   - Component ChangeVersion: Only specified ComponentType(s) set to GlobalChangeVersion
        //   - Chunk OrderVersion: Unchanged.
        //   - Sources:
        //     - GetComponentDataWithTypeRW
        //     - GetComponentDataRW
        //     - SwapComponents
        //     - SetSharedComponentDataIndex

        [Test]
        public void GetComponentRW_Via_SetComponentData()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));

            BumpGlobalSystemVersion();

            m_Manager.SetComponentData(e1, new EcsTestData(1));

            AssertSameChunk(e0, e1);
            AssetHasChangeVersion<EcsTestData>(e0, NewVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChunkOrderVersion(e1, OldVersion);
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void GetComponentRW_Via_SetComponentData_ManagedComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestData2));

            BumpGlobalSystemVersion();

            m_Manager.SetComponentData(e1, new EcsTestManagedComponent {value = "SomeString"});

            AssertSameChunk(e0, e1);
            AssetHasChangeVersion<EcsTestManagedComponent>(e0, NewVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChunkOrderVersion(e1, OldVersion);
        }

        [Test]
        public void GetComponentRW_Via_GetComponentData_ManagedComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestData2));

            m_Manager.SetComponentData(e0, new EcsTestManagedComponent {value = "e0"});
            m_Manager.SetComponentData(e1, new EcsTestManagedComponent {value = "e1"});

            BumpGlobalSystemVersion();

            m_Manager.GetComponentData<EcsTestManagedComponent>(e1).value = "SomeString";

            AssertSameChunk(e0, e1);
            AssetHasChangeVersion<EcsTestManagedComponent>(e0, NewVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChunkOrderVersion(e1, OldVersion);
        }

#endif

        [Test]
        public void GetComponentRW_Via_GetBuffer()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsIntElement));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsIntElement));

            BumpGlobalSystemVersion();

            var buffer = m_Manager.GetBuffer<EcsIntElement>(e1);
            buffer.Add(7);

            AssertSameChunk(e0, e1);
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasBufferChangeVersion<EcsIntElement>(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, OldVersion);
        }

        [Test]
        public void SetSharedComponentDataIndex_Via_Entity()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestSharedComp));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestSharedComp));

            BumpGlobalSystemVersion();

            // Individual Entity not changed in place.
            m_Manager.SetSharedComponentData(e1, new EcsTestSharedComp(7));

            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasSharedChangeVersion<EcsTestSharedComp>(e0, OldVersion);
            AssetHasSharedChangeVersion<EcsTestSharedComp>(e1, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

        [Test]
        public void SwapComponents()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestSharedComp));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestSharedComp));

            m_Manager.SetSharedComponentData(e0, new EcsTestSharedComp(1));
            m_Manager.SetSharedComponentData(e1, new EcsTestSharedComp(2));

            var chunk0 = m_Manager.GetChunk(e0);
            var chunk1 = m_Manager.GetChunk(e1);

            Assert.AreNotEqual(chunk0, chunk1);
            BumpGlobalSystemVersion();

            m_Manager.SwapComponents(chunk0, 0, chunk1, 0);

            AssetHasChangeVersion<EcsTestData>(e0, NewVersion);
            AssetHasSharedChangeVersion<EcsTestSharedComp>(e0, NewVersion);
            AssetHasChangeVersion<EcsTestData>(e1, NewVersion);
            AssetHasSharedChangeVersion<EcsTestSharedComp>(e1, NewVersion);

            AssetHasChunkOrderVersion(e0, OldVersion);
            AssetHasChunkOrderVersion(e1, OldVersion);
        }

        // Version Change Case 3:
        //   - Component ChangeVersion: All ComponentType(s) with EntityReference in archetype set to GlobalChangeVersion
        //   - Chunk OrderVersion: Unchanged.
        //   - Sources:
        //     - ClearMissingReferences

        [Test]
        public unsafe void ClearMissingReferences()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestDataEntity));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestDataEntity));
            var e2 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestDataEntity));

            m_Manager.SetComponentData(e1, new EcsTestDataEntity {value0 = 0, value1 = e0});
            m_Manager.SetComponentData(e2, new EcsTestDataEntity {value0 = 0, value1 = e0});
            m_Manager.DestroyEntity(e0);

            var chunk0 = m_Manager.GetChunk(e1);
            var chunk1 = m_Manager.GetChunk(e2);
            Assert.AreEqual(chunk0, chunk1);

            BumpGlobalSystemVersion();

            ChunkDataUtility.ClearMissingReferences(chunk0.m_Chunk);

            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestDataEntity>(e1, NewVersion);
            AssetHasChunkOrderVersion(e1, OldVersion);
            AssetHasChunkOrderVersion(e2, OldVersion);
        }

        // Version Change Case 4:
        //   - Component ChangeVersion: ComponentTypes(s) that exist in destination archetype but not source archetype set to GlobalChangeVersion
        //   - Chunk OrderVersion: Unchanged.
        //   - Sources:
        //     - CloneChangeVersions via ChangeArchetypeInPlace
        //     - CloneChangeVersions via PatchAndAddClonedChunks

        [Test]
        public void CloneChangeVia_AddComponent_Tag_Via_Query()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData));

            BumpGlobalSystemVersion();

            m_Manager.AddComponent(m_Manager.UniversalQuery, typeof(EcsTestTag));

            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestTag>(e0, NewVersion);
            AssetHasChunkOrderVersion(e0, OldVersion);
        }

        [Test]
        public void CloneChangeVia_AddSharedComponent_Via_Query()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData));

            BumpGlobalSystemVersion();

            m_Manager.AddSharedComponentData(m_Manager.UniversalQuery, new SharedData1(5));

            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasSharedChangeVersion<SharedData1>(e0, NewVersion);
            AssetHasChunkOrderVersion(e0, OldVersion);
        }

        // Version Change Case 5:
        //   - Component ChangeVersion: Unchanged.
        //   - Chunk OrderVersion: Destination chunk version set to GlobalChangeVersion.
        //   - Sources:
        //     - Deallocate
        //     - Remove

        [Test]
        public void Remove_Via_AddComponent_Tag_Via_Entity()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData));

            BumpGlobalSystemVersion();

            // Individual Entity not changed in place.
            m_Manager.AddComponent(e0, typeof(EcsTestTag));

            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestTag>(e0, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
        }

        [Test]
        public void Remove_Via_AddSharedComponent_Via_Entity()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData));

            BumpGlobalSystemVersion();

            // Individual Entity not changed in place.
            m_Manager.AddSharedComponentData(e0, new SharedData1(5));

            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasSharedChangeVersion<SharedData1>(e0, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
        }

        [Test]
        public void Remove_Via_AddComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            BumpGlobalSystemVersion();
            m_Manager.AddComponentData(e1, new EcsTestData3(7));
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData3>(e1, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void Remove_Via_AddComponent_ManagedComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            BumpGlobalSystemVersion();
            m_Manager.AddComponentData(e1, new EcsTestManagedComponent {value = "SomeString"});
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestManagedComponent>(e1, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#endif

        [Test]
        public void Remove_Via_AddComponent_DefaultValue()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            BumpGlobalSystemVersion();
            m_Manager.AddComponent<EcsTestData3>(e1);
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData3>(e1, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

        [Test]
        public void Remove_Via_AddComponent_DefaultValue_Via_EntityArray()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));

            var entities = new NativeArray<Entity>(1, Allocator.TempJob);
            entities[0] = e1;

            BumpGlobalSystemVersion();
            m_Manager.AddComponent<EcsTestData3>(entities);

            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData3>(e1, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);

            entities.Dispose();
        }

        [Test]
        public void Remove_Via_AddComponent_Tag()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            BumpGlobalSystemVersion();
            m_Manager.AddComponentData(e1, new EcsTestTag());
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestTag>(e1, NewVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

        [Test]
        public void Remove_Via_AddChunkComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));

            BumpGlobalSystemVersion();

            m_Manager.AddChunkComponentData<EcsTestData3>(e1);
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

        [Test]
        public void Remove_Via_AddSharedComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsTestData2));

            BumpGlobalSystemVersion();

            m_Manager.AddSharedComponentData(e1, new EcsTestSharedComp {value = 2});

            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        [Test]
        public void Remove_Via_AddSharedComponent_With_ManagedComponent()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestData2));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestManagedComponent), typeof(EcsTestData2));

            BumpGlobalSystemVersion();

            m_Manager.AddSharedComponentData(e1, new EcsTestSharedComp {value = 2});

            AssetHasChangeVersion<EcsTestManagedComponent>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e0, OldVersion);
            AssetHasChangeVersion<EcsTestManagedComponent>(e1, OldVersion);
            AssetHasChangeVersion<EcsTestData2>(e1, OldVersion);
            AssetHasChunkOrderVersion(e0, NewVersion);
            AssetHasChunkOrderVersion(e1, NewVersion);
        }

#endif

        //
        // No version changes
        //

        unsafe struct CollectBufferLength : IJobChunk
        {
            [ReadOnly] public ArchetypeChunkBufferType<EcsIntElement> EcsIntElementType;
            [NativeDisableUnsafePtrRestriction] public int* Count;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var buffer = chunk.GetBufferAccessor(EcsIntElementType);
                *Count = buffer.Length;
            }
        }

        [Test]
        public unsafe void GetComponentRO_Via_GetBuffer()
        {
            var e0 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsIntElement));
            var e1 = m_Manager.CreateEntity(typeof(EcsTestData), typeof(EcsIntElement));

            BumpGlobalSystemVersion();

            var query = m_Manager.CreateEntityQuery(ComponentType.ReadOnly<EcsIntElement>());

            int* count = stackalloc int[1];
            var collectBufferLengthJob = new CollectBufferLength
            {
                EcsIntElementType = m_Manager.GetArchetypeChunkBufferType<EcsIntElement>(true),
                Count = count
            };
            collectBufferLengthJob.Run(query);

            AssertSameChunk(e0, e1);
            AssetHasChangeVersion<EcsTestData>(e0, OldVersion);
            AssetHasBufferChangeVersion<EcsIntElement>(e0, OldVersion);
            AssetHasChunkOrderVersion(e1, OldVersion);
        }
    }
}
