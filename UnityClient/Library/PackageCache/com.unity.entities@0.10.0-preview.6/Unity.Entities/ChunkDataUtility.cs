using System;
using System.Diagnostics;
using Unity.Assertions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Entities
{
    // ChunkDataUtility
    //
    // [x] Step 1: Firewall version changes to reduce test vectors
    //     - Everything that could potentially change versions is firewalled in here.
    //     - Anything that takes `ref EntityComponentStore` represents a problem for multi-threading sub-chunk access.
    // [ ] Step 2: Remove EntityComponentStore references
    //

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
    //
    // Version Change Case 2:
    //   - Component ChangeVersion: Only specified ComponentType(s) set to GlobalChangeVersion
    //   - Chunk OrderVersion: Unchanged.
    //   - Sources:
    //     - GetComponentDataWithTypeRW
    //     - GetComponentDataRW
    //     - SwapComponents
    //     - SetSharedComponentDataIndex
    //
    // Version Change Case 3:
    //   - Component ChangeVersion: All ComponentType(s) with EntityReference in archetype set to GlobalChangeVersion
    //   - Chunk OrderVersion: Unchanged.
    //   - Sources:
    //     - ClearMissingReferences
    //
    // Version Change Case 4:
    //   - Component ChangeVersion: ComponentTypes(s) that exist in destination archetype but not source archetype set to GlobalChangeVersion
    //   - Chunk OrderVersion: Unchanged.
    //   - Sources:
    //     - CloneChangeVersions via ChangeArchetypeInPlace
    //     - CloneChangeVersions via PatchAndAddClonedChunks
    //     - CloneChangeVersions via Clone
    //
    // Version Change Case 5:
    //   - Component ChangeVersion: Unchanged.
    //   - Chunk OrderVersion: Destination chunk version set to GlobalChangeVersion.
    //   - Sources:
    //     - Deallocate
    //     - Remove

    internal static unsafe class ChunkDataUtility
    {
        public static int GetIndexInTypeArray(Archetype* archetype, int typeIndex)
        {
            var types = archetype->Types;
            var typeCount = archetype->TypesCount;
            for (var i = 0; i != typeCount; i++)
                if (typeIndex == types[i].TypeIndex)
                    return i;

            return -1;
        }

        public static int GetTypeIndexFromType(Archetype* archetype, Type componentType)
        {
            var types = archetype->Types;
            var typeCount = archetype->TypesCount;
            for (var i = 0; i != typeCount; i++)
                if (componentType.IsAssignableFrom(TypeManager.GetType(types[i].TypeIndex)))
                    return types[i].TypeIndex;

            return -1;
        }

        public static void GetIndexInTypeArray(Archetype* archetype, int typeIndex, ref int typeLookupCache)
        {
            var types = archetype->Types;
            var typeCount = archetype->TypesCount;

            if (typeLookupCache >= 0 && typeLookupCache < typeCount && types[typeLookupCache].TypeIndex == typeIndex)
                return;

            for (var i = 0; i != typeCount; i++)
            {
                if (typeIndex != types[i].TypeIndex)
                    continue;

                typeLookupCache = i;
                return;
            }

            typeLookupCache = -1;
        }

        public static int GetSizeInChunk(Chunk* chunk, int typeIndex, ref int typeLookupCache)
        {
            var archetype = chunk->Archetype;
            GetIndexInTypeArray(archetype, typeIndex, ref typeLookupCache);
            var indexInTypeArray = typeLookupCache;

            var sizeOf = archetype->SizeOfs[indexInTypeArray];

            return sizeOf;
        }

        public static void SetSharedComponentDataIndex(Entity entity, Archetype* archetype, in SharedComponentValues sharedComponentValues, int typeIndex)
        {
            var entityComponentStore = archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;

            entityComponentStore->Move(entity, archetype, sharedComponentValues);

            var chunk = entityComponentStore->GetChunk(entity);
            var indexInTypeArray = GetIndexInTypeArray(chunk->Archetype, typeIndex);
            chunk->SetChangeVersion(indexInTypeArray, globalSystemVersion);
        }

        public static byte* GetComponentDataWithTypeRO(Chunk* chunk, int index, int typeIndex, ref int typeLookupCache)
        {
            var archetype = chunk->Archetype;
            GetIndexInTypeArray(archetype, typeIndex, ref typeLookupCache);
            var indexInTypeArray = typeLookupCache;

            var offset = archetype->Offsets[indexInTypeArray];
            var sizeOf = archetype->SizeOfs[indexInTypeArray];

            return chunk->Buffer + (offset + sizeOf * index);
        }

        public static byte* GetComponentDataWithTypeRW(Chunk* chunk, int index, int typeIndex, uint globalSystemVersion,
            ref int typeLookupCache)
        {
            var archetype = chunk->Archetype;
            GetIndexInTypeArray(archetype, typeIndex, ref typeLookupCache);
            var indexInTypeArray = typeLookupCache;

            var offset = archetype->Offsets[indexInTypeArray];
            var sizeOf = archetype->SizeOfs[indexInTypeArray];

            // Write Component to Chunk. ChangeVersion:Yes OrderVersion:No
            chunk->SetChangeVersion(indexInTypeArray, globalSystemVersion);

            return chunk->Buffer + (offset + sizeOf * index);
        }

        public static byte* GetComponentDataWithTypeRO(Chunk* chunk, int index, int typeIndex)
        {
            var indexInTypeArray = GetIndexInTypeArray(chunk->Archetype, typeIndex);

            var offset = chunk->Archetype->Offsets[indexInTypeArray];
            var sizeOf = chunk->Archetype->SizeOfs[indexInTypeArray];

            return chunk->Buffer + (offset + sizeOf * index);
        }

        public static byte* GetComponentDataWithTypeRW(Chunk* chunk, int index, int typeIndex, uint globalSystemVersion)
        {
            var indexInTypeArray = GetIndexInTypeArray(chunk->Archetype, typeIndex);

            var offset = chunk->Archetype->Offsets[indexInTypeArray];
            var sizeOf = chunk->Archetype->SizeOfs[indexInTypeArray];

            // Write Component to Chunk. ChangeVersion:Yes OrderVersion:No
            chunk->SetChangeVersion(indexInTypeArray, globalSystemVersion);

            return chunk->Buffer + (offset + sizeOf * index);
        }

        public static byte* GetComponentDataRO(Chunk* chunk, int index, int indexInTypeArray)
        {
            var offset = chunk->Archetype->Offsets[indexInTypeArray];
            var sizeOf = chunk->Archetype->SizeOfs[indexInTypeArray];

            return chunk->Buffer + (offset + sizeOf * index);
        }

        public static byte* GetComponentDataRW(Chunk* chunk, int index, int indexInTypeArray)
        {
            var offset = chunk->Archetype->Offsets[indexInTypeArray];
            var sizeOf = chunk->Archetype->SizeOfs[indexInTypeArray];
            var entityComponentStore = chunk->Archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;

            // Write Component to Chunk. ChangeVersion:Yes OrderVersion:No
            chunk->SetChangeVersion(indexInTypeArray, globalSystemVersion);

            return chunk->Buffer + (offset + sizeOf * index);
        }

        public static void Copy(Chunk* srcChunk, int srcIndex, Chunk* dstChunk, int dstIndex, int count)
        {
            Assert.IsTrue(srcChunk->Archetype == dstChunk->Archetype);

            var arch = srcChunk->Archetype;
            var srcBuffer = srcChunk->Buffer;
            var dstBuffer = dstChunk->Buffer;
            var offsets = arch->Offsets;
            var sizeOfs = arch->SizeOfs;
            var typesCount = arch->TypesCount;

            for (var t = 0; t < typesCount; t++)
            {
                var offset = offsets[t];
                var sizeOf = sizeOfs[t];
                var src = srcBuffer + (offset + sizeOf * srcIndex);
                var dst = dstBuffer + (offset + sizeOf * dstIndex);

                UnsafeUtility.MemCpy(dst, src, sizeOf * count);
            }
        }

        public static void SwapComponents(Chunk* srcChunk, int srcIndex, Chunk* dstChunk, int dstIndex, int count, uint srcGlobalSystemVersion, uint dstGlobalSystemVersion)
        {
            var srcArch = srcChunk->Archetype;
            var typesCount = srcArch->TypesCount;


#if UNITY_ASSERTIONS
            // This function is used to swap data between different world so assert that the layout is identical if
            // the archetypes dont match
            var dstArch = dstChunk->Archetype;
            if (srcArch != dstArch)
            {
                Assert.AreEqual(typesCount, dstChunk->Archetype->TypesCount);
                for (int i = 0; i < typesCount; ++i)
                {
                    Assert.AreEqual(srcArch->Types[i], dstArch->Types[i]);
                    Assert.AreEqual(srcArch->Offsets[i], dstArch->Offsets[i]);
                    Assert.AreEqual(srcArch->SizeOfs[i], dstArch->SizeOfs[i]);
                }
            }
#endif

            var srcBuffer = srcChunk->Buffer;
            var dstBuffer = dstChunk->Buffer;
            var offsets = srcArch->Offsets;
            var sizeOfs = srcArch->SizeOfs;

            for (var t = 1; t < typesCount; t++) // Only swap component data, not Entity
            {
                var offset = offsets[t];
                var sizeOf = sizeOfs[t];
                var src = srcBuffer + (offset + sizeOf * srcIndex);
                var dst = dstBuffer + (offset + sizeOf * dstIndex);
                Byte* buffer = stackalloc Byte[sizeOf * count];

                dstChunk->SetChangeVersion(t, dstGlobalSystemVersion);
                srcChunk->SetChangeVersion(t, srcGlobalSystemVersion);

                UnsafeUtility.MemCpy(buffer, src, sizeOf * count);
                UnsafeUtility.MemCpy(src, dst, sizeOf * count);
                UnsafeUtility.MemCpy(dst, buffer, sizeOf * count);
            }
        }

        public static void InitializeComponents(Chunk* dstChunk, int dstIndex, int count)
        {
            var arch = dstChunk->Archetype;

            var offsets = arch->Offsets;
            var sizeOfs = arch->SizeOfs;
            var bufferCapacities = arch->BufferCapacities;
            var dstBuffer = dstChunk->Buffer;
            var typesCount = arch->TypesCount;
            var types = arch->Types;

            for (var t = 1; t != typesCount; t++)
            {
                var offset = offsets[t];
                var sizeOf = sizeOfs[t];
                var dst = dstBuffer + (offset + sizeOf * dstIndex);

                if (types[t].IsBuffer)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        BufferHeader.Initialize((BufferHeader*)dst, bufferCapacities[t]);
                        dst += sizeOf;
                    }
                }
                else
                {
                    UnsafeUtility.MemClear(dst, sizeOf * count);
                }
            }
        }

        public static void InitializeBuffersInChunk(byte* p, int count, int stride, int bufferCapacity)
        {
            for (int i = 0; i < count; i++)
            {
                BufferHeader.Initialize((BufferHeader*)p, bufferCapacity);
                p += stride;
            }
        }

        public static void Convert(Chunk* srcChunk, int srcIndex, Chunk* dstChunk, int dstIndex, int count)
        {
            Assert.IsFalse(srcChunk == dstChunk);
            var srcArch = srcChunk->Archetype;
            var dstArch = dstChunk->Archetype;
            var entityComponentStore = dstArch->EntityComponentStore;
            if (srcArch != dstArch)
            {
                Assert.IsFalse(srcArch == null);
            }

            var srcI = srcArch->NonZeroSizedTypesCount - 1;
            var dstI = dstArch->NonZeroSizedTypesCount - 1;

            var sourceTypesToDealloc = stackalloc int[srcI + 1];
            int sourceTypesToDeallocCount = 0;

            while (dstI >= 0)
            {
                var srcType = srcArch->Types[srcI];
                var dstType = dstArch->Types[dstI];

                if (srcType > dstType)
                {
                    //Type in source is not moved so deallocate it
                    sourceTypesToDealloc[sourceTypesToDeallocCount++] = srcI;
                    --srcI;
                    continue;
                }

                var srcStride = srcArch->SizeOfs[srcI];
                var dstStride = dstArch->SizeOfs[dstI];
                var src = srcChunk->Buffer + srcArch->Offsets[srcI] + srcIndex * srcStride;
                var dst = dstChunk->Buffer + dstArch->Offsets[dstI] + dstIndex * dstStride;

                if (srcType == dstType)
                {
                    UnsafeUtility.MemCpy(dst, src, count * srcStride);
                    --srcI;
                    --dstI;
                }
                else
                {
                    if (dstType.IsBuffer)
                        InitializeBuffersInChunk(dst, count, dstStride, dstArch->BufferCapacities[dstI]);
                    else
                        UnsafeUtility.MemClear(dst, count * dstStride);
                    --dstI;
                }
            }

            if (sourceTypesToDeallocCount == 0)
                return;

            sourceTypesToDealloc[sourceTypesToDeallocCount] = 0;

            int iDealloc = 0;
            if (sourceTypesToDealloc[iDealloc] >= srcArch->FirstManagedComponent)
            {
                var freeCommandHandle = entityComponentStore->ManagedChangesTracker.BeginFreeManagedComponentCommand();
                do
                {
                    srcI = sourceTypesToDealloc[iDealloc];
                    var srcStride = srcArch->SizeOfs[srcI];
                    var src = srcChunk->Buffer + srcArch->Offsets[srcI] + srcIndex * srcStride;

                    var a = (int*)src;
                    for (int i = 0; i < count; i++)
                    {
                        var managedComponentIndex = a[i];
                        if (managedComponentIndex == 0)
                            continue;
                        entityComponentStore->FreeManagedComponentIndex(managedComponentIndex);
                        entityComponentStore->ManagedChangesTracker.AddToFreeManagedComponentCommand(managedComponentIndex);
                    }
                }
                while ((sourceTypesToDealloc[++iDealloc] >= srcArch->FirstManagedComponent));
                entityComponentStore->ManagedChangesTracker.EndDeallocateManagedComponentCommand(freeCommandHandle);
            }

            while (sourceTypesToDealloc[iDealloc] >= srcArch->FirstBufferComponent)
            {
                srcI = sourceTypesToDealloc[iDealloc];
                var srcStride = srcArch->SizeOfs[srcI];
                var srcPtr = srcChunk->Buffer + srcArch->Offsets[srcI] + srcIndex * srcStride;
                for (int i = 0; i < count; i++)
                {
                    BufferHeader.Destroy((BufferHeader*)srcPtr);
                    srcPtr += srcStride;
                }
                ++iDealloc;
            }
        }

        public static void MemsetUnusedChunkData(Chunk* chunk, byte value)
        {
            var arch = chunk->Archetype;
            var bufferSize = Chunk.GetChunkBufferSize();
            var buffer = chunk->Buffer;
            var count = chunk->Count;

            for (int i = 0; i < arch->TypesCount - 1; ++i)
            {
                var index = arch->TypeMemoryOrder[i];

                var nextIndex = arch->TypeMemoryOrder[i + 1];
                var componentSize = arch->SizeOfs[index];
                var startOffset = arch->Offsets[index] + count * componentSize;
                var endOffset = arch->Offsets[nextIndex];
                var componentDataType = &arch->Types[index];

                // Start Offset needs to be fixed if we have a Dynamic Buffer
                if (componentDataType->IsBuffer)
                {
                    var elementSize = TypeManager.GetTypeInfo(componentDataType->TypeIndex).ElementSize;
                    var bufferCapacity = arch->BufferCapacities[index];

                    for (int chunkI = 0; chunkI < count; chunkI++)
                    {
                        var bufferHeader = (BufferHeader*)(buffer + arch->Offsets[index] + (chunkI * componentSize));

                        // If bufferHeader->Pointer is not null it means with rely on a dedicated buffer instead of the internal one (that follows the header) to store the elements
                        //  in this case we wipe everything after the header. Otherwise we wipe after the used elements.
                        var elementCountToClean = bufferHeader->Pointer != null ? bufferCapacity : (bufferHeader->Capacity - bufferHeader->Length);
                        var firstElementToClean = bufferHeader->Pointer != null ? 0 : bufferHeader->Length;

                        byte* internalBuffer = (byte*)(bufferHeader + 1);

                        UnsafeUtility.MemSet(internalBuffer + (firstElementToClean * elementSize), value, elementCountToClean * elementSize);
                    }
                }

                UnsafeUtility.MemSet(buffer + startOffset, value, endOffset - startOffset);
            }
            var lastIndex = arch->TypeMemoryOrder[arch->TypesCount - 1];
            var lastStartOffset = arch->Offsets[lastIndex] + count * arch->SizeOfs[lastIndex];
            UnsafeUtility.MemSet(buffer + lastStartOffset, value, bufferSize - lastStartOffset);

            // 0 the sequence number and the chunk header padding zone
            UnsafeUtility.MemClear(40 + (byte*)chunk, 24);    // End of chunk header at 40, we clear the header padding (24) and the Buffer value which is the very first data after the header
        }

        public static bool AreLayoutCompatible(Archetype* a, Archetype* b)
        {
            if ((a == null) || (b == null) ||
                (a->ChunkCapacity != b->ChunkCapacity))
                return false;

            var typeCount = a->NonZeroSizedTypesCount;
            if (typeCount != b->NonZeroSizedTypesCount)
                return false;

            for (int i = 0; i < typeCount; ++i)
            {
                if (a->Types[i] != b->Types[i])
                    return false;
            }

            return true;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void AssertAreLayoutCompatible(Archetype* a, Archetype* b)
        {
            Assert.IsTrue(AreLayoutCompatible(a, b));

            var typeCount = a->NonZeroSizedTypesCount;

            //If types are identical; SizeOfs, Offsets and BufferCapacities should match
            for (int i = 0; i < typeCount; ++i)
            {
                Assert.AreEqual(a->SizeOfs[i], b->SizeOfs[i]);
                Assert.AreEqual(a->Offsets[i], b->Offsets[i]);
                Assert.AreEqual(a->BufferCapacities[i], b->BufferCapacities[i]);
            }
        }

        public static void DeallocateBuffers(Chunk* chunk)
        {
            var archetype = chunk->Archetype;

            var bufferComponentsEnd = archetype->BufferComponentsEnd;
            for (var ti = archetype->FirstBufferComponent; ti < bufferComponentsEnd; ++ti)
            {
                Assert.IsTrue(archetype->Types[ti].IsBuffer);
                var basePtr = chunk->Buffer + archetype->Offsets[ti];
                var stride = archetype->SizeOfs[ti];

                for (int i = 0; i < chunk->Count; ++i)
                {
                    byte* bufferPtr = basePtr + stride * i;
                    BufferHeader.Destroy((BufferHeader*)bufferPtr);
                }
            }
        }

        static void ReleaseChunk(Chunk* chunk)
        {
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;

            // Remove references to shared components
            if (chunk->Archetype->NumSharedComponents > 0)
            {
                var sharedComponentValueArray = chunk->SharedComponentValues;

                for (var i = 0; i < chunk->Archetype->NumSharedComponents; ++i)
                    entityComponentStore->ManagedChangesTracker.RemoveReference(sharedComponentValueArray[i]);
            }

            // this chunk is going away, so it shouldn't be in the empty slot list.
            if (chunk->Count < chunk->Capacity)
                chunk->Archetype->EmptySlotTrackingRemoveChunk(chunk);

            chunk->Archetype->RemoveFromChunkList(chunk);
            chunk->Archetype = null;

            entityComponentStore->FreeChunk(chunk);
        }

        public static void SetChunkCountKeepMetaChunk(Chunk* chunk, int newCount)
        {
            Assert.AreNotEqual(newCount, chunk->Count);

            // Chunk released to empty chunk pool
            if (newCount == 0)
            {
                ReleaseChunk(chunk);
                return;
            }

            var capacity = chunk->Capacity;

            // Chunk is now full
            if (newCount == capacity)
            {
                // this chunk no longer has empty slots, so it shouldn't be in the empty slot list.
                chunk->Archetype->EmptySlotTrackingRemoveChunk(chunk);
            }
            // Chunk is no longer full
            else if (chunk->Count == capacity)
            {
                Assert.IsTrue(newCount < chunk->Count);
                chunk->Archetype->EmptySlotTrackingAddChunk(chunk);
            }

            chunk->Count = newCount;
            chunk->Archetype->Chunks.SetChunkEntityCount(chunk->ListIndex, newCount);
        }

        public static void SetChunkCount(Chunk* chunk, int newCount)
        {
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;

            var metaChunkEntity = chunk->metaChunkEntity;
            if (newCount == 0 && metaChunkEntity != Entity.Null)
                entityComponentStore->DestroyMetaChunkEntity(metaChunkEntity);

            SetChunkCountKeepMetaChunk(chunk, newCount);
        }

        // #todo https://unity3d.atlassian.net/browse/DOTS-1189
        static int AllocateIntoChunk(Chunk* chunk, int count, out int outIndex)
        {
            var allocatedCount = Math.Min(chunk->Capacity - chunk->Count, count);
            outIndex = chunk->Count;
            SetChunkCount(chunk, chunk->Count + allocatedCount);
            chunk->Archetype->EntityCount += allocatedCount;
            return allocatedCount;
        }

        public static void Allocate(Chunk* chunk, int count)
        {
            Allocate(chunk, null, count);
        }

        public static void Allocate(Chunk* chunk, Entity* entities, int count)
        {
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;

            int allocatedIndex;
            var allocatedCount = AllocateIntoChunk(chunk, count, out allocatedIndex);
            entityComponentStore->AllocateEntities(archetype, chunk, allocatedIndex, allocatedCount, entities);
            InitializeComponents(chunk, allocatedIndex, allocatedCount);

            // Add Entities in Chunk. ChangeVersion:Yes OrderVersion:Yes
            chunk->SetAllChangeVersions(globalSystemVersion);
            chunk->SetOrderVersion(globalSystemVersion);
            entityComponentStore->IncrementComponentTypeOrderVersion(archetype);
        }

        public static void Remove(in EntityBatchInChunk batchInChunk)
        {
            var chunk = batchInChunk.Chunk;
            var count = batchInChunk.Count;
            var startIndex = batchInChunk.StartIndex;
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;

            // Fill in moved component data from the end.
            var srcTailIndex = startIndex + count;
            var srcTailCount = chunk->Count - srcTailIndex;
            var fillCount = math.min(count, srcTailCount);
            if (fillCount > 0)
            {
                var fillStartIndex = chunk->Count - fillCount;

                Copy(chunk, fillStartIndex, chunk, startIndex, fillCount);

                var fillEntities = (Entity*)GetComponentDataRO(chunk, startIndex, 0);
                for (int i = 0; i < fillCount; i++)
                {
                    var entity = fillEntities[i];
                    entityComponentStore->SetEntityInChunk(entity, new EntityInChunk { Chunk = chunk, IndexInChunk = startIndex + i });
                }
            }

            chunk->SetOrderVersion(entityComponentStore->GlobalSystemVersion);
            entityComponentStore->IncrementComponentTypeOrderVersion(archetype);
            entityComponentStore->ManagedChangesTracker.IncrementComponentOrderVersion(archetype, chunk->SharedComponentValues);

            SetChunkCount(chunk, chunk->Count - count);
            archetype->EntityCount -= count;
        }

        /// <summary>
        /// Fix-up the chunk to refer to a different (but layout compatible) archetype.
        /// - Should only be called by Move(chunk)
        /// </summary>
        public static void ChangeArchetypeInPlace(Chunk* srcChunk, Archetype* dstArchetype, int* sharedComponentValues)
        {
            var srcArchetype = srcChunk->Archetype;
            var entityComponentStore = dstArchetype->EntityComponentStore;
            AssertAreLayoutCompatible(srcArchetype, dstArchetype);

            var fixupSharedComponentReferences =
                (srcArchetype->NumSharedComponents > 0) || (dstArchetype->NumSharedComponents > 0);
            if (fixupSharedComponentReferences)
            {
                int srcFirstShared = srcArchetype->FirstSharedComponent;
                int dstFirstShared = dstArchetype->FirstSharedComponent;
                int srcCount = srcArchetype->NumSharedComponents;
                int dstCount = dstArchetype->NumSharedComponents;

                int o = 0;
                int n = 0;

                for (; n < dstCount && o < srcCount;)
                {
                    int srcType = srcArchetype->Types[o + srcFirstShared].TypeIndex;
                    int dstType = dstArchetype->Types[n + dstFirstShared].TypeIndex;
                    if (srcType == dstType)
                    {
                        var srcSharedComponentDataIndex = srcChunk->SharedComponentValues[o];
                        var dstSharedComponentDataIndex = sharedComponentValues[n];
                        if (srcSharedComponentDataIndex != dstSharedComponentDataIndex)
                        {
                            entityComponentStore->ManagedChangesTracker.RemoveReference(srcSharedComponentDataIndex);
                            entityComponentStore->ManagedChangesTracker.AddReference(dstSharedComponentDataIndex);
                        }

                        n++;
                        o++;
                    }
                    else if (dstType > srcType) // removed from dstArchetype
                    {
                        var sharedComponentDataIndex = srcChunk->SharedComponentValues[o];
                        entityComponentStore->ManagedChangesTracker.RemoveReference(sharedComponentDataIndex);
                        o++;
                    }
                    else // added to dstArchetype
                    {
                        var sharedComponentDataIndex = sharedComponentValues[n];
                        entityComponentStore->ManagedChangesTracker.AddReference(sharedComponentDataIndex);
                        n++;
                    }
                }

                for (; n < dstCount; n++) // added to dstArchetype
                {
                    var sharedComponentDataIndex = sharedComponentValues[n];
                    entityComponentStore->ManagedChangesTracker.AddReference(sharedComponentDataIndex);
                }

                for (; o < srcCount; o++) // removed from dstArchetype
                {
                    var sharedComponentDataIndex = srcChunk->SharedComponentValues[o];
                    entityComponentStore->ManagedChangesTracker.RemoveReference(sharedComponentDataIndex);
                }
            }

            var count = srcChunk->Count;
            bool hasEmptySlots = count < srcChunk->Capacity;

            if (hasEmptySlots)
                srcArchetype->EmptySlotTrackingRemoveChunk(srcChunk);

            int chunkIndexInSrcArchetype = srcChunk->ListIndex;

            //Change version is overriden below
            dstArchetype->AddToChunkList(srcChunk, sharedComponentValues, 0);
            int chunkIndexInDstArchetype = srcChunk->ListIndex;

            // For unchanged components: Copy versions from src to dst archetype
            // For different components:
            //   - (srcArchetype->Chunks) Remove Component In-Place. ChangeVersion:No OrderVersion:No
            //   - (dstArchetype->Chunks) Add Component In-Place. ChangeVersion:Yes OrderVersion:No

            CloneChangeVersions(srcArchetype, chunkIndexInSrcArchetype, dstArchetype, chunkIndexInDstArchetype);

            srcChunk->ListIndex = chunkIndexInSrcArchetype;
            srcArchetype->RemoveFromChunkList(srcChunk);
            srcChunk->ListIndex = chunkIndexInDstArchetype;

            if (hasEmptySlots)
                dstArchetype->EmptySlotTrackingAddChunk(srcChunk);

            entityComponentStore->SetArchetype(srcChunk, dstArchetype);

            srcArchetype->EntityCount -= count;
            dstArchetype->EntityCount += count;

            if (srcArchetype->MetaChunkArchetype != dstArchetype->MetaChunkArchetype)
            {
                if (srcArchetype->MetaChunkArchetype == null)
                {
                    entityComponentStore->CreateMetaEntityForChunk(srcChunk);
                }
                else if (dstArchetype->MetaChunkArchetype == null)
                {
                    entityComponentStore->DestroyMetaChunkEntity(srcChunk->metaChunkEntity);
                    srcChunk->metaChunkEntity = Entity.Null;
                }
                else
                {
                    var metaChunk = entityComponentStore->GetChunk(srcChunk->metaChunkEntity);
                    entityComponentStore->Move(srcChunk->metaChunkEntity, dstArchetype->MetaChunkArchetype, metaChunk->SharedComponentValues);
                }
            }
        }

        public static void MoveArchetype(Chunk* chunk, Archetype* dstArchetype, SharedComponentValues sharedComponentValues)
        {
            var srcArchetype = chunk->Archetype;
            var entityComponentStore = dstArchetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;

            if (chunk->Count < chunk->Capacity)
                srcArchetype->EmptySlotTrackingRemoveChunk(chunk);
            srcArchetype->RemoveFromChunkList(chunk);
            srcArchetype->EntityCount -= chunk->Count;

            chunk->Archetype = dstArchetype;

            dstArchetype->EntityCount += chunk->Count;
            dstArchetype->AddToChunkList(chunk, sharedComponentValues, globalSystemVersion);
            if (chunk->Count < chunk->Capacity)
                dstArchetype->EmptySlotTrackingAddChunk(chunk);

            entityComponentStore->IncrementComponentTypeOrderVersion(dstArchetype);
            chunk->SetOrderVersion(globalSystemVersion);
        }

        public static void CloneChangeVersions(Archetype* srcArchetype, int chunkIndexInSrcArchetype, Archetype* dstArchetype, int chunkIndexInDstArchetype, bool dstValidExistingVersions = false)
        {
            var dstTypes = dstArchetype->Types;
            var srcTypes = srcArchetype->Types;
            var dstGlobalSystemVersion = dstArchetype->EntityComponentStore->GlobalSystemVersion;
            var srcGlobalSystemVersion = srcArchetype->EntityComponentStore->GlobalSystemVersion;

            for (int isrcType = srcArchetype->TypesCount - 1, idstType = dstArchetype->TypesCount - 1;
                 idstType >= 0;
                 --idstType)
            {
                var dstType = dstTypes[idstType];
                while (srcTypes[isrcType] > dstType)
                    --isrcType;

                var version = dstGlobalSystemVersion;

                // select "newer" version relative to dst EntityComponentStore GlobalSystemVersion
                if (srcTypes[isrcType] == dstType)
                {
                    var srcVersion = srcArchetype->Chunks.GetChangeVersion(isrcType, chunkIndexInSrcArchetype);
                    if (dstValidExistingVersions)
                    {
                        var dstVersion = dstArchetype->Chunks.GetChangeVersion(idstType, chunkIndexInDstArchetype);

                        var srcVersionSinceChange = srcGlobalSystemVersion - srcVersion;
                        var dstVersionSinceChange = dstGlobalSystemVersion - dstVersion;

                        if (dstVersionSinceChange < srcVersionSinceChange)
                            version = dstVersion;
                        else
                            version = dstGlobalSystemVersion - srcVersionSinceChange;
                    }
                    else
                    {
                        version = srcVersion;
                    }
                }

                dstArchetype->Chunks.SetChangeVersion(idstType, chunkIndexInDstArchetype, version);
            }
        }

        public static void AllocateClone(Chunk* chunk, Entity* entities, int count, Entity srcEntity)
        {
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;
            var src = entityComponentStore->GetEntityInChunk(srcEntity);

            int allocatedIndex;
            var allocatedCount = AllocateIntoChunk(chunk, count, out allocatedIndex);
            entityComponentStore->AllocateEntities(archetype, chunk, allocatedIndex, allocatedCount, entities);
            ReplicateComponents(src.Chunk, src.IndexInChunk, chunk, allocatedIndex, allocatedCount);

            // Add Entities in Chunk. ChangeVersion:Yes OrderVersion:Yes
            chunk->SetAllChangeVersions(globalSystemVersion);
            chunk->SetOrderVersion(globalSystemVersion);

#if UNITY_EDITOR
            for (var i = 0; i < allocatedCount; ++i)
                entityComponentStore->CopyName(entities[i], srcEntity);
#endif

            entityComponentStore->ManagedChangesTracker.IncrementComponentOrderVersion(archetype, chunk->SharedComponentValues);
            entityComponentStore->IncrementComponentTypeOrderVersion(archetype);
        }

        public static void Deallocate(Chunk* chunk)
        {
            Deallocate(new EntityBatchInChunk {Chunk = chunk, StartIndex = 0, Count = chunk->Count});
        }

        public static void Deallocate(in EntityBatchInChunk batch)
        {
            var chunk = batch.Chunk;
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;
            var startIndex = batch.StartIndex;
            var count = batch.Count;

            entityComponentStore->DeallocateDataEntitiesInChunk(chunk, startIndex, count);
            entityComponentStore->ManagedChangesTracker.IncrementComponentOrderVersion(archetype, chunk->SharedComponentValues);

            // Remove Entities in Chunk. ChangeVersion:No OrderVersion:Yes
            chunk->SetOrderVersion(globalSystemVersion);
            entityComponentStore->IncrementComponentTypeOrderVersion(archetype);

            chunk->Archetype->EntityCount -= count;
            SetChunkCount(chunk, chunk->Count - count);
        }

        public static void Clone(in EntityBatchInChunk srcBatch, Chunk* dstChunk)
        {
            var srcChunk = srcBatch.Chunk;
            var srcChunkIndex = srcBatch.StartIndex;
            var srcCount = srcBatch.Count;
            var dstArchetype = dstChunk->Archetype;
            var srcArchetype = srcChunk->Archetype;
            var entityComponentStore = dstArchetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;

            // Note (srcArchetype == dstArchetype) is valid
            // Archetypes can the the same, but chunks still differ because filter is different (e.g. shared component)

            int dstChunkIndex;
            var dstValidExistingVersions = dstChunk->Count != 0;
            var dstCount = AllocateIntoChunk(dstChunk, srcCount, out dstChunkIndex);
            Assert.IsTrue(dstCount == srcCount);

            Convert(srcChunk, srcChunkIndex, dstChunk, dstChunkIndex, dstCount);

            var dstEntities = (Entity*)ChunkDataUtility.GetComponentDataRO(dstChunk, dstChunkIndex, 0);
            for (int i = 0; i < dstCount; i++)
            {
                var entity = dstEntities[i];

                entityComponentStore->SetArchetype(entity, dstArchetype);
                entityComponentStore->SetEntityInChunk(entity, new EntityInChunk { Chunk = dstChunk, IndexInChunk = dstChunkIndex + i });
            }

            CloneChangeVersions(srcArchetype, srcChunk->ListIndex, dstArchetype, dstChunk->ListIndex, dstValidExistingVersions);

            dstChunk->SetOrderVersion(globalSystemVersion);
            entityComponentStore->IncrementComponentTypeOrderVersion(dstArchetype);
            entityComponentStore->ManagedChangesTracker.IncrementComponentOrderVersion(dstArchetype, dstChunk->SharedComponentValues);

            // Cannot DestroyEntities unless SystemStateCleanupComplete on the entity chunk.
            if (dstChunk->Archetype->SystemStateCleanupComplete)
                entityComponentStore->DestroyEntities(dstEntities, dstCount);
        }

        static void ReplicateComponents(Chunk* srcChunk, int srcIndex, Chunk* dstChunk, int dstBaseIndex, int count)
        {
            var srcArchetype        = srcChunk->Archetype;
            var srcBuffer           = srcChunk->Buffer;
            var dstBuffer           = dstChunk->Buffer;
            var dstArchetype        = dstChunk->Archetype;
            var srcOffsets          = srcArchetype->Offsets;
            var srcSizeOfs          = srcArchetype->SizeOfs;
            var srcBufferCapacities = srcArchetype->BufferCapacities;
            var srcTypes            = srcArchetype->Types;
            var dstTypes            = dstArchetype->Types;
            var dstOffsets          = dstArchetype->Offsets;
            var dstTypeIndex        = 1;

            var nativeComponentsEnd = srcArchetype->NativeComponentsEnd;
            for (var srcTypeIndex = 1; srcTypeIndex != nativeComponentsEnd; srcTypeIndex++)
            {
                var srcType = srcTypes[srcTypeIndex];
                var dstType = dstTypes[dstTypeIndex];
                // Type does not exist in destination. Skip it.
                if (srcType.TypeIndex != dstType.TypeIndex)
                    continue;
                var srcSizeOf = srcSizeOfs[srcTypeIndex];
                var src = srcBuffer + (srcOffsets[srcTypeIndex] + srcSizeOf * srcIndex);
                var dst = dstBuffer + (dstOffsets[dstTypeIndex] + srcSizeOf * dstBaseIndex);

                UnsafeUtility.MemCpyReplicate(dst, src, srcSizeOf, count);
                dstTypeIndex++;
            }

            dstTypeIndex = dstArchetype->FirstBufferComponent;
            var bufferComponentsEnd = srcArchetype->BufferComponentsEnd;
            for (var srcTypeIndex = srcArchetype->FirstBufferComponent; srcTypeIndex != bufferComponentsEnd; srcTypeIndex++)
            {
                var srcType = srcTypes[srcTypeIndex];
                var dstType = dstTypes[dstTypeIndex];
                // Type does not exist in destination. Skip it.
                if (srcType.TypeIndex != dstType.TypeIndex)
                    continue;
                var srcSizeOf = srcSizeOfs[srcTypeIndex];
                var src = srcBuffer + (srcOffsets[srcTypeIndex] + srcSizeOf * srcIndex);
                var dst = dstBuffer + (dstOffsets[dstTypeIndex] + srcSizeOf * dstBaseIndex);

                var srcBufferCapacity = srcBufferCapacities[srcTypeIndex];
                var alignment = 8; // TODO: Need a way to compute proper alignment for arbitrary non-generic types in TypeManager
                var elementSize = TypeManager.GetTypeInfo(srcType.TypeIndex).ElementSize;
                for (int i = 0; i < count; ++i)
                {
                    BufferHeader* srcHdr = (BufferHeader*)src;
                    BufferHeader* dstHdr = (BufferHeader*)dst;
                    BufferHeader.Initialize(dstHdr, srcBufferCapacity);
                    BufferHeader.Assign(dstHdr, BufferHeader.GetElementPointer(srcHdr), srcHdr->Length, elementSize, alignment, false, 0);

                    dst += srcSizeOf;
                }

                dstTypeIndex++;
            }

            if (dstArchetype->NumManagedComponents > 0)
            {
                ReplicateManagedComponents(srcChunk, srcIndex, dstChunk, dstBaseIndex, count);
            }
        }

        static void ReplicateManagedComponents(Chunk* srcChunk, int srcIndex, Chunk* dstChunk, int dstBaseIndex, int count)
        {
            var dstArchetype = dstChunk->Archetype;
            var entityComponentStore = dstArchetype->EntityComponentStore;
            var srcArchetype = srcChunk->Archetype;
            var srcTypes = srcArchetype->Types;
            var dstTypes = dstArchetype->Types;
            var srcOffsets          = srcArchetype->Offsets;
            var dstOffsets          = dstArchetype->Offsets;
            int componentCount = dstArchetype->NumManagedComponents;

            int nonNullManagedComponents = 0;
            int nonNullHybridComponents = 0;
            var componentIndices = stackalloc int[componentCount];
            var componentDstArrayStart = stackalloc IntPtr[componentCount];

            var firstDstManagedComponent = dstArchetype->FirstManagedComponent;
            var dstTypeIndex = firstDstManagedComponent;
            var managedComponentsEnd = srcArchetype->ManagedComponentsEnd;
            var srcBaseAddr = srcChunk->Buffer + sizeof(int) * srcIndex;
            var dstBaseAddr = dstChunk->Buffer + sizeof(int) * dstBaseIndex;

            bool hasHybridComponents = dstArchetype->HasHybridComponents;

            for (var srcTypeIndex = srcArchetype->FirstManagedComponent; srcTypeIndex != managedComponentsEnd; srcTypeIndex++)
            {
                var srcType = srcTypes[srcTypeIndex];
                var dstType = dstTypes[dstTypeIndex];
                // Type does not exist in destination. Skip it.
                if (srcType.TypeIndex != dstType.TypeIndex)
                    continue;
                int srcManagedComponentIndex = *(int*)(srcBaseAddr + srcOffsets[srcTypeIndex]);
                var dstArrayStart = dstBaseAddr + dstOffsets[dstTypeIndex];

                if (srcManagedComponentIndex == 0)
                {
                    UnsafeUtility.MemClear(dstArrayStart, sizeof(int) * count);
                }
                else
                {
                    if (hasHybridComponents && TypeManager.GetTypeInfo(srcType.TypeIndex).Category == TypeManager.TypeCategory.Class)
                    {
                        //Hybrid component, put at end of array
                        var index = componentCount - nonNullHybridComponents - 1;
                        componentIndices[index] = srcManagedComponentIndex;
                        componentDstArrayStart[index] = (IntPtr)dstArrayStart;
                        ++nonNullHybridComponents;
                    }
                    else
                    {
                        componentIndices[nonNullManagedComponents] = srcManagedComponentIndex;
                        componentDstArrayStart[nonNullManagedComponents] = (IntPtr)dstArrayStart;
                        ++nonNullManagedComponents;
                    }
                }

                dstTypeIndex++;
            }

            entityComponentStore->ReserveManagedComponentIndices(count * (nonNullManagedComponents + nonNullHybridComponents));
            entityComponentStore->ManagedChangesTracker.CloneManagedComponentBegin(componentIndices, nonNullManagedComponents, count);
            for (int c = 0; c < nonNullManagedComponents; ++c)
            {
                var dst = (int*)(componentDstArrayStart[c]);
                entityComponentStore->AllocateManagedComponentIndices(dst, count);
                entityComponentStore->ManagedChangesTracker.CloneManagedComponentAddDstIndices(dst, count);
            }

            if (hasHybridComponents)
            {
                var companionLinkIndexInTypeArray = GetIndexInTypeArray(dstArchetype, ManagedComponentStore.CompanionLinkTypeIndex);
                var companionLinkIndices = (companionLinkIndexInTypeArray == -1) ? null : (int*)(dstBaseAddr + dstOffsets[companionLinkIndexInTypeArray]);

                var dstEntities = (Entity*)dstChunk->Buffer + dstBaseIndex;
                entityComponentStore->ManagedChangesTracker.CloneHybridComponentBegin(componentIndices + componentCount - nonNullHybridComponents, nonNullHybridComponents, dstEntities, count, companionLinkIndices);
                for (int c = componentCount - nonNullHybridComponents; c < componentCount; ++c)
                {
                    var dst = (int*)(componentDstArrayStart[c]);
                    entityComponentStore->AllocateManagedComponentIndices(dst, count);
                    entityComponentStore->ManagedChangesTracker.CloneHybridComponentAddDstIndices(dst, count);
                }
            }
        }

        /// <summary>
        /// @TODO NET_DOTS fixed byte Buffer[4] fails to compile when used as a ptr.
        /// </summary>
        public static byte* GetChunkBuffer(Chunk* chunk)
        {
#if !NET_DOTS
            return chunk->Buffer;
#else
            return (byte*)chunk + Chunk.kBufferOffset;
#endif
        }

        public static void ClearMissingReferences(Chunk* chunk)
        {
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;
            var typesCount = archetype->TypesCount;
            var entityCount = chunk->Count;

            for (var typeIndexInArchetype = 1; typeIndexInArchetype < typesCount; typeIndexInArchetype++)
            {
                var componentTypeInArchetype = archetype->Types[typeIndexInArchetype];

                if (!componentTypeInArchetype.HasEntityReferences || componentTypeInArchetype.IsSharedComponent ||
                    componentTypeInArchetype.IsZeroSized)
                {
                    continue;
                }

                var typeInfo = entityComponentStore->GetTypeInfo(componentTypeInArchetype.TypeIndex);
                var typeInChunkPtr = GetChunkBuffer(chunk) + archetype->Offsets[typeIndexInArchetype];
                var typeSizeOf = archetype->SizeOfs[typeIndexInArchetype];

                var changed = false;

                if (componentTypeInArchetype.IsBuffer)
                {
                    for (var entityIndexInChunk = 0; entityIndexInChunk < entityCount; entityIndexInChunk++)
                    {
                        var componentDataPtr = typeInChunkPtr + typeSizeOf * entityIndexInChunk;
                        var bufferHeader = (BufferHeader*)componentDataPtr;
                        var bufferLength = bufferHeader->Length;
                        var bufferPtr = BufferHeader.GetElementPointer(bufferHeader);
                        changed |= ClearEntityReferences(entityComponentStore, typeInfo, bufferPtr, bufferLength);
                    }
                }
                else
                {
                    for (var entityIndexInChunk = 0; entityIndexInChunk < entityCount; entityIndexInChunk++)
                    {
                        var componentDataPtr = typeInChunkPtr + typeSizeOf * entityIndexInChunk;
                        changed |= ClearEntityReferences(entityComponentStore, typeInfo, componentDataPtr, 1);
                    }
                }

                if (changed)
                {
                    chunk->SetChangeVersion(typeIndexInArchetype, globalSystemVersion);
                }
            }
        }

        static bool ClearEntityReferences(EntityComponentStore* entityComponentStore, TypeManager.TypeInfo typeInfo, byte* address, int elementCount)
        {
            var changed = false;

            var offsets = entityComponentStore->GetEntityOffsets(typeInfo);

            for (var elementIndex = 0; elementIndex < elementCount; elementIndex++)
            {
                var elementPtr = address + typeInfo.ElementSize * elementIndex;

                for (var offsetIndex = 0; offsetIndex < typeInfo.EntityOffsetCount; offsetIndex++)
                {
                    var offset = offsets[offsetIndex].Offset;

                    if (entityComponentStore->Exists(*(Entity*)(elementPtr + offset)))
                        continue;

                    *(Entity*)(elementPtr + offset) = Entity.Null;
                    changed = true;
                }
            }

            return changed;
        }

        public static Entity GetEntityFromEntityInChunk(EntityInChunk entityInChunk)
        {
            var chunk = entityInChunk.Chunk;
            var archetype = chunk->Archetype;
            var buffer = GetChunkBuffer(chunk) + archetype->Offsets[0] + entityInChunk.IndexInChunk * archetype->SizeOfs[0];
            return *(Entity*)buffer;
        }

        public static void AddExistingChunk(Chunk* chunk, int* sharedComponentIndices)
        {
            var archetype = chunk->Archetype;
            var entityComponentStore = archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;
            archetype->AddToChunkList(chunk, sharedComponentIndices, globalSystemVersion);
            archetype->EntityCount += chunk->Count;

            for (var i = 0; i < archetype->NumSharedComponents; ++i)
                entityComponentStore->ManagedChangesTracker.AddReference(sharedComponentIndices[i]);

            if (chunk->Count < chunk->Capacity)
                archetype->EmptySlotTrackingAddChunk(chunk);

            entityComponentStore->AddExistingEntitiesInChunk(chunk);
        }

        public static void AddEmptyChunk(Archetype* archetype, Chunk* chunk, SharedComponentValues sharedComponentValues)
        {
            var entityComponentStore = archetype->EntityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;

            chunk->Archetype = archetype;
            chunk->Count = 0;
            chunk->Capacity = archetype->ChunkCapacity;
            chunk->SequenceNumber = entityComponentStore->AssignSequenceNumber(chunk);
            chunk->metaChunkEntity = Entity.Null;

            var numSharedComponents = archetype->NumSharedComponents;

            if (numSharedComponents > 0)
            {
                for (var i = 0; i < archetype->NumSharedComponents; ++i)
                {
                    var sharedComponentIndex = sharedComponentValues[i];
                    entityComponentStore->ManagedChangesTracker.AddReference(sharedComponentIndex);
                }
            }

            archetype->AddToChunkList(chunk, sharedComponentValues, globalSystemVersion);

            Assert.IsTrue(archetype->Chunks.Count != 0);

            // Chunk can't be locked at at construction time
            archetype->EmptySlotTrackingAddChunk(chunk);

            if (numSharedComponents == 0)
            {
                Assert.IsTrue(archetype->ChunksWithEmptySlots.Length != 0);
            }
            else
            {
                Assert.IsTrue(archetype->FreeChunksBySharedComponents.TryGet(chunk->SharedComponentValues,
                    archetype->NumSharedComponents) != null);
            }

            chunk->Flags = 0;
        }
    }
}
