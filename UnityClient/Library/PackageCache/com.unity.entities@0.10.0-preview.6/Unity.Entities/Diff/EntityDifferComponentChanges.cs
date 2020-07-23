using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
#if !NET_DOTS
using Unity.Properties;
#endif

namespace Unity.Entities
{
    static unsafe partial class EntityDiffer
    {
        struct DeferredSharedComponentChange
        {
            public EntityGuid EntityGuid;
            public int TypeIndex;
            public int BeforeSharedComponentIndex;
            public int AfterSharedComponentIndex;
        }

        struct DeferredManagedComponentChange
        {
            public EntityGuid EntityGuid;
            public int TypeIndex;
            public int AfterManagedComponentIndex;
            public int BeforeManagedComponentIndex;
        }

        readonly struct ComponentChanges : IDisposable
        {
            public readonly PackedCollection<EntityGuid> Entities;
            public readonly PackedCollection<ComponentTypeHash> ComponentTypes;
            public readonly NativeList<PackedComponent> AddComponents;
            public readonly NativeList<PackedComponent> RemoveComponents;
            public readonly NativeList<PackedComponentDataChange> SetComponents;
            public readonly NativeList<LinkedEntityGroupChange> LinkedEntityGroupAdditions;
            public readonly NativeList<LinkedEntityGroupChange> LinkedEntityGroupRemovals;
            public readonly NativeList<EntityReferenceChange> EntityReferenceChanges;
            public readonly NativeList<BlobAssetReferenceChange> BlobAssetReferenceChanges;
            public readonly NativeList<byte> ComponentData;
            public readonly NativeList<DeferredSharedComponentChange> SharedComponentChanges;
            public readonly NativeList<DeferredManagedComponentChange> ManagedComponentChanges;

            public readonly bool IsCreated;

            public ComponentChanges(Allocator allocator)
            {
                Entities = new PackedCollection<EntityGuid>(1, allocator);
                ComponentTypes = new PackedCollection<ComponentTypeHash>(1, allocator);
                AddComponents = new NativeList<PackedComponent>(1, allocator);
                RemoveComponents = new NativeList<PackedComponent>(1, allocator);
                SetComponents = new NativeList<PackedComponentDataChange>(1, allocator);
                LinkedEntityGroupAdditions = new NativeList<LinkedEntityGroupChange>(1, allocator);
                LinkedEntityGroupRemovals = new NativeList<LinkedEntityGroupChange>(1, allocator);
                EntityReferenceChanges = new NativeList<EntityReferenceChange>(1, allocator);
                BlobAssetReferenceChanges = new NativeList<BlobAssetReferenceChange>(1, allocator);
                ComponentData = new NativeList<byte>(1, allocator);
                SharedComponentChanges = new NativeList<DeferredSharedComponentChange>(1, allocator);
                ManagedComponentChanges = new NativeList<DeferredManagedComponentChange>(1, allocator);
                IsCreated = true;
            }

            public void Dispose()
            {
                Entities.Dispose();
                ComponentTypes.Dispose();
                AddComponents.Dispose();
                RemoveComponents.Dispose();
                SetComponents.Dispose();
                LinkedEntityGroupAdditions.Dispose();
                LinkedEntityGroupRemovals.Dispose();
                EntityReferenceChanges.Dispose();
                BlobAssetReferenceChanges.Dispose();
                ComponentData.Dispose();
                SharedComponentChanges.Dispose();
                ManagedComponentChanges.Dispose();
            }
        }

        struct PackedCollection<T> : IDisposable where T : unmanaged, IEquatable<T>
        {
            public NativeList<T> List;
            public NativeHashMap<T, int> Lookup;

            public PackedCollection(int capacity, Allocator label)
            {
                List = new NativeList<T>(capacity, label);
                Lookup = new NativeHashMap<T, int>(capacity, label);
            }

            public void Dispose()
            {
                List.Dispose();
                Lookup.Dispose();
            }

            public int GetOrAdd(T value)
            {
                if (Lookup.TryGetValue(value, out var index))
                {
                    return index;
                }
                index = List.Length;
                List.Add(value);
                Lookup.TryAdd(value, index);
                return index;
            }
        }

        [BurstCompile]
        struct GatherComponentChanges : IJob
        {
            public int EntityGuidTypeIndex;
            public int LinkedEntityGroupTypeIndex;

            [NativeDisableUnsafePtrRestriction] public TypeManager.TypeInfo* TypeInfo;
            [NativeDisableUnsafePtrRestriction] public TypeManager.EntityOffsetInfo* EntityOffsets;
            [NativeDisableUnsafePtrRestriction] public TypeManager.EntityOffsetInfo* BlobAssetRefOffsets;
            [NativeDisableUnsafePtrRestriction] public EntityComponentStore* AfterEntityComponentStore;
            [NativeDisableUnsafePtrRestriction] public EntityComponentStore* BeforeEntityComponentStore;

            public NativeList<CreatedEntity> CreatedEntities;
            public NativeList<ModifiedEntity> ModifiedEntities;

            public PackedCollection<EntityGuid> Entities;
            public PackedCollection<ComponentTypeHash> ComponentTypes;

            [WriteOnly] public NativeList<PackedComponent> AddComponents;
            [WriteOnly] public NativeList<PackedComponentDataChange> SetComponents;
            [WriteOnly] public NativeList<PackedComponent> RemoveComponents;
            [WriteOnly] public NativeList<EntityReferenceChange> EntityReferencePatches;
            [WriteOnly] public NativeList<BlobAssetReferenceChange> BlobAssetReferenceChanges;
            [WriteOnly] public NativeList<LinkedEntityGroupChange> LinkedEntityGroupAdditions;
            [WriteOnly] public NativeList<LinkedEntityGroupChange> LinkedEntityGroupRemovals;
            [WriteOnly] public NativeList<byte> ComponentData;
            [WriteOnly] public NativeList<DeferredSharedComponentChange> SharedComponentChanges;
            [WriteOnly] public NativeList<DeferredManagedComponentChange> ManagedComponentChanges;

            [NativeDisableContainerSafetyRestriction] public NativeHashMap<BlobAssetReferencePtr, BlobAssetPtr> AfterBlobAssetRemap;
            [NativeDisableContainerSafetyRestriction] public NativeHashMap<BlobAssetReferencePtr, BlobAssetPtr> BeforeBlobAssetRemap;

            public void Execute()
            {
                for (var i = 0; i < CreatedEntities.Length; ++i)
                {
                    var entityGuid = CreatedEntities[i].EntityGuid;
                    var afterEntity = CreatedEntities[i].AfterEntityInChunk;
                    var afterChunk = afterEntity.Chunk;
                    var afterArchetype = afterChunk->Archetype;
                    var afterTypesCount = afterArchetype->TypesCount;

                    for (var afterIndexInTypeArray = 1; afterIndexInTypeArray < afterTypesCount; afterIndexInTypeArray++)
                    {
                        var afterTypeInArchetype = afterArchetype->Types[afterIndexInTypeArray];

                        if (afterTypeInArchetype.IsSystemStateComponent)
                            continue;

                        AddComponentData(
                            afterChunk,
                            afterArchetype,
                            afterTypeInArchetype,
                            afterIndexInTypeArray,
                            afterEntity.IndexInChunk,
                            entityGuid);
                    }
                }

                for (var i = 0; i < ModifiedEntities.Length; ++i)
                {
                    var modification = ModifiedEntities[i];
                    var entityGuid = modification.EntityGuid;

                    var afterEntity = modification.AfterEntityInChunk;
                    var afterChunk = afterEntity.Chunk;
                    var afterArchetype = afterChunk->Archetype;
                    var afterTypesCount = afterArchetype->TypesCount;

                    var beforeEntity = modification.BeforeEntityInChunk;
                    var beforeChunk = beforeEntity.Chunk;
                    var beforeArchetype = beforeChunk->Archetype;
                    var beforeTypesCount = beforeArchetype->TypesCount;

                    for (var afterIndexInTypeArray = 1; afterIndexInTypeArray < afterTypesCount; afterIndexInTypeArray++)
                    {
                        var afterTypeInArchetype = afterArchetype->Types[afterIndexInTypeArray];

                        if (afterTypeInArchetype.IsSystemStateComponent || afterTypeInArchetype.IsChunkComponent)
                        {
                            continue;
                        }

                        var typeIndex = afterTypeInArchetype.TypeIndex;
                        var beforeIndexInTypeArray = ChunkDataUtility.GetIndexInTypeArray(beforeArchetype, typeIndex);

                        // This type is missing in the before entity.
                        // This means we are dealing with a newly added component.
                        if (-1 == beforeIndexInTypeArray)
                        {
                            // This type does not exist on the before world. This was a newly added component.
                            AddComponentData(
                                afterChunk,
                                afterArchetype,
                                afterTypeInArchetype,
                                afterIndexInTypeArray,
                                afterEntity.IndexInChunk,
                                entityGuid
                            );

                            continue;
                        }

                        if (!afterTypeInArchetype.IsManagedComponent && modification.CanCompareChunkVersions && afterChunk->GetChangeVersion(afterIndexInTypeArray) == beforeChunk->GetChangeVersion(beforeIndexInTypeArray))
                        {
                            continue;
                        }

                        SetComponentData(
                            afterChunk,
                            afterArchetype,
                            afterTypeInArchetype,
                            afterIndexInTypeArray,
                            afterEntity.IndexInChunk,
                            beforeChunk,
                            beforeArchetype,
                            beforeIndexInTypeArray,
                            beforeEntity.IndexInChunk,
                            entityGuid);
                    }

                    for (var beforeTypeIndexInArchetype = 1; beforeTypeIndexInArchetype < beforeTypesCount; beforeTypeIndexInArchetype++)
                    {
                        var beforeComponentTypeInArchetype = beforeArchetype->Types[beforeTypeIndexInArchetype];

                        if (beforeComponentTypeInArchetype.IsSystemStateComponent)
                        {
                            continue;
                        }

                        var beforeTypeIndex = beforeComponentTypeInArchetype.TypeIndex;

                        if (-1 == ChunkDataUtility.GetIndexInTypeArray(afterArchetype, beforeTypeIndex))
                        {
                            var packedComponent = PackComponent(entityGuid, beforeTypeIndex);
                            RemoveComponents.Add(packedComponent);
                        }
                    }
                }
            }

            void AddComponentData(
                Chunk* afterChunk,
                Archetype* afterArchetype,
                ComponentTypeInArchetype afterTypeInArchetype,
                int afterIndexInTypeArray,
                int afterEntityIndexInChunk,
                EntityGuid entityGuid)
            {
                var packedComponent = PackComponent(entityGuid, afterTypeInArchetype.TypeIndex);

                AddComponents.Add(packedComponent);

                if (afterTypeInArchetype.IsSharedComponent)
                {
                    var offset = afterIndexInTypeArray - afterChunk->Archetype->FirstSharedComponent;
                    var sharedComponentIndex = afterChunk->GetSharedComponentValue(offset);

                    // No managed objects in burst land. Do what we can a defer the actual unpacking until later.
                    AddendSharedComponentData(entityGuid, afterTypeInArchetype.TypeIndex, sharedComponentIndex);
                    return;
                }

                if (afterTypeInArchetype.IsManagedComponent)
                {
                    var afterManagedComponentIndex  = ((int*)(ChunkDataUtility.GetChunkBuffer(afterChunk) + afterArchetype->Offsets[afterIndexInTypeArray]))[afterEntityIndexInChunk];
                    AppendManagedComponentData(entityGuid, afterTypeInArchetype.TypeIndex, afterManagedComponentIndex);
                    return;
                }

                // IMPORTANT This means `IsZeroSizedInChunk` which is always true for shared components.
                // Always check shared components first.
                if (afterTypeInArchetype.IsZeroSized)
                {
                    return;
                }

                if (afterTypeInArchetype.IsBuffer)
                {
                    var sizeOf = afterArchetype->SizeOfs[afterIndexInTypeArray];
                    var buffer = (BufferHeader*)(ChunkDataUtility.GetChunkBuffer(afterChunk) + afterArchetype->Offsets[afterIndexInTypeArray] + afterEntityIndexInChunk * sizeOf);
                    var length = buffer->Length;

                    if (length == 0)
                    {
                        return;
                    }

                    var elementPtr = BufferHeader.GetElementPointer(buffer);

                    if (afterTypeInArchetype.TypeIndex == LinkedEntityGroupTypeIndex)
                    {
                        // Magic in AddComponent already put a self-reference at the top of the buffer, so there's no need for us to add it.
                        // The rest of the elements should be interpreted as LinkedEntityGroupAdditions.
                        for (var elementIndex = 1; elementIndex < length; elementIndex++)
                        {
                            var childEntity = ((Entity*)elementPtr)[elementIndex];
                            if (!TryGetEntityGuid(AfterEntityComponentStore, childEntity, out var childEntityGuid))
                            {
                                // If the child entity doesn't have a guid, there's no way for us to communicate its identity to the destination world.
                                throw new Exception("LinkedEntityGroup child is missing an EntityGuid component.");
                            }

                            LinkedEntityGroupAdditions.Add(new LinkedEntityGroupChange
                            {
                                RootEntityGuid = entityGuid,
                                ChildEntityGuid = childEntityGuid
                            });
                        }
                    }
                    else
                    {
                        var typeInfo = TypeInfo[afterTypeInArchetype.TypeIndex & TypeManager.ClearFlagsMask];
                        AppendComponentData(packedComponent, elementPtr, typeInfo.ElementSize * length);
                        ExtractPatches(typeInfo, packedComponent, elementPtr, length);
                    }
                }
                else
                {
                    var typeInfo = TypeInfo[afterTypeInArchetype.TypeIndex & TypeManager.ClearFlagsMask];
                    var sizeOf = afterArchetype->SizeOfs[afterIndexInTypeArray];
                    var ptr = ChunkDataUtility.GetChunkBuffer(afterChunk) + afterArchetype->Offsets[afterIndexInTypeArray] + afterEntityIndexInChunk * sizeOf;
                    AppendComponentData(packedComponent, ptr, sizeOf);
                    ExtractPatches(typeInfo, packedComponent, ptr, 1);
                }
            }

            void SetComponentData(
                Chunk* afterChunk,
                Archetype* afterArchetype,
                ComponentTypeInArchetype afterTypeInArchetype,
                int afterIndexInTypeArray,
                int afterEntityIndexInChunk,
                Chunk* beforeChunk,
                Archetype* beforeArchetype,
                int beforeIndexInTypeArray,
                int beforeEntityIndexInChunk,
                EntityGuid entityGuid)
            {
                if (afterTypeInArchetype.IsSharedComponent)
                {
                    var beforeOffset = beforeIndexInTypeArray - beforeChunk->Archetype->FirstSharedComponent;
                    var beforeSharedComponentIndex = beforeChunk->GetSharedComponentValue(beforeOffset);

                    var afterOffset = afterIndexInTypeArray - afterChunk->Archetype->FirstSharedComponent;
                    var afterSharedComponentIndex = afterChunk->GetSharedComponentValue(afterOffset);

                    // No managed objects in burst land. Do what we can and defer the actual unpacking until later.
                    AddendSharedComponentData(entityGuid, afterTypeInArchetype.TypeIndex, afterSharedComponentIndex, beforeSharedComponentIndex);
                    return;
                }

                if (afterTypeInArchetype.IsManagedComponent)
                {
                    var afterManagedComponentIndex  = ((int*)(ChunkDataUtility.GetChunkBuffer(afterChunk) + afterArchetype->Offsets[afterIndexInTypeArray]))[afterEntityIndexInChunk];
                    var beforeManagedComponentIndex  = ((int*)(ChunkDataUtility.GetChunkBuffer(beforeChunk) + beforeArchetype->Offsets[beforeIndexInTypeArray]))[beforeEntityIndexInChunk];

                    AppendManagedComponentData(entityGuid, afterTypeInArchetype.TypeIndex, afterManagedComponentIndex, beforeManagedComponentIndex);
                    return;
                }

                // IMPORTANT This means `IsZeroSizedInChunk` which is always true for shared components.
                // Always check shared components first.
                if (afterTypeInArchetype.IsZeroSized)
                {
                    return;
                }

                if (afterTypeInArchetype.IsBuffer)
                {
                    var beforeBuffer = (BufferHeader*)(ChunkDataUtility.GetChunkBuffer(beforeChunk)
                        + beforeArchetype->Offsets[beforeIndexInTypeArray]
                        + beforeEntityIndexInChunk
                        * beforeArchetype->SizeOfs[beforeIndexInTypeArray]);

                    var beforeElementPtr = BufferHeader.GetElementPointer(beforeBuffer);
                    var beforeLength = beforeBuffer->Length;

                    var afterBuffer = (BufferHeader*)(ChunkDataUtility.GetChunkBuffer(afterChunk)
                        + afterArchetype->Offsets[afterIndexInTypeArray]
                        + afterEntityIndexInChunk
                        * afterArchetype->SizeOfs[afterIndexInTypeArray]);

                    var afterElementPtr = BufferHeader.GetElementPointer(afterBuffer);
                    var afterLength = afterBuffer->Length;

                    if (afterTypeInArchetype.TypeIndex == LinkedEntityGroupTypeIndex)
                    {
                        var beforeLinkedEntityGroups = (LinkedEntityGroup*)beforeElementPtr;
                        var afterLinkedEntityGroups = (LinkedEntityGroup*)afterElementPtr;

                        // Using is not supported by burst.
                        var beforeLinkedEntityGroupEntityGuids = new NativeArray<EntityGuid>(beforeLength, Allocator.Temp);
                        var afterLinkedEntityGroupEntityGuids = new NativeArray<EntityGuid>(afterLength, Allocator.Temp);
                        {
                            for (var i = 0; i < beforeLength; i++)
                            {
                                if (!TryGetEntityGuid(BeforeEntityComponentStore, beforeLinkedEntityGroups[i].Value, out var beforeEntityGuid))
                                {
                                    throw new Exception("LinkedEntityGroup child is missing an EntityGuid component.");
                                }

                                beforeLinkedEntityGroupEntityGuids[i] = beforeEntityGuid;
                            }

                            for (var i = 0; i < afterLength; i++)
                            {
                                if (!TryGetEntityGuid(AfterEntityComponentStore, afterLinkedEntityGroups[i].Value, out var afterEntityGuid))
                                {
                                    throw new Exception("LinkedEntityGroup child is missing an EntityGuid component.");
                                }

                                afterLinkedEntityGroupEntityGuids[i] = afterEntityGuid;
                            }

                            beforeLinkedEntityGroupEntityGuids.Sort();
                            afterLinkedEntityGroupEntityGuids.Sort();

                            var beforeIndex = 0;
                            var afterIndex = 0;

                            while (beforeIndex < beforeLength && afterIndex < afterLength)
                            {
                                var beforeEntityGuid = beforeLinkedEntityGroupEntityGuids[beforeIndex];
                                var afterEntityGuid = afterLinkedEntityGroupEntityGuids[afterIndex];

                                var comparison = beforeEntityGuid.CompareTo(afterEntityGuid);

                                if (comparison == 0)
                                {
                                    // If the entity is in both "before" and "after", then there is no change.
                                    beforeIndex++;
                                    afterIndex++;
                                }
                                else if (comparison > 0)
                                {
                                    // If the entity is in "before" but not "after", it's been removed.
                                    LinkedEntityGroupRemovals.Add(new LinkedEntityGroupChange {RootEntityGuid = entityGuid, ChildEntityGuid = beforeEntityGuid});
                                    beforeIndex++;
                                }
                                else if (comparison < 0)
                                {
                                    // If the entity is in "after" but not "before", it's been added.
                                    LinkedEntityGroupAdditions.Add(new LinkedEntityGroupChange {RootEntityGuid = entityGuid, ChildEntityGuid = afterEntityGuid});
                                    afterIndex++;
                                }
                            }

                            while (beforeIndex < beforeLength)
                            {
                                // If the entity is in "before" but not "after", it's been removed.
                                LinkedEntityGroupRemovals.Add(new LinkedEntityGroupChange
                                    {RootEntityGuid = entityGuid, ChildEntityGuid = beforeLinkedEntityGroupEntityGuids[beforeIndex++]});
                            }

                            while (afterIndex < afterLength)
                            {
                                // If the entity is in "after" but not "before", it's been added.
                                LinkedEntityGroupAdditions.Add(new LinkedEntityGroupChange
                                    {RootEntityGuid = entityGuid, ChildEntityGuid = afterLinkedEntityGroupEntityGuids[afterIndex++]});
                            }
                        }
                    }
                    else
                    {
                        var typeInfo = TypeInfo[afterTypeInArchetype.TypeIndex & TypeManager.ClearFlagsMask];

                        if (afterLength != beforeLength
                            || UnsafeUtility.MemCmp(beforeElementPtr, afterElementPtr, afterLength * typeInfo.ElementSize) != 0
                            || BlobAssetHashesAreDifferent(typeInfo, beforeElementPtr, afterElementPtr, afterLength))
                        {
                            var packedComponent = PackComponent(entityGuid, afterTypeInArchetype.TypeIndex);
                            AppendComponentData(packedComponent, afterElementPtr, typeInfo.ElementSize * afterLength);
                            ExtractPatches(typeInfo, packedComponent, afterElementPtr, afterLength);
                        }
                    }
                }
                else
                {
                    if (beforeArchetype->SizeOfs[beforeIndexInTypeArray] != afterArchetype->SizeOfs[afterIndexInTypeArray])
                    {
                        throw new Exception("Archetype->SizeOfs do not match");
                    }

                    var beforeAddress = ChunkDataUtility.GetChunkBuffer(beforeChunk)
                        + beforeArchetype->Offsets[beforeIndexInTypeArray]
                        + beforeArchetype->SizeOfs[beforeIndexInTypeArray]
                        * beforeEntityIndexInChunk;

                    var afterAddress = ChunkDataUtility.GetChunkBuffer(afterChunk)
                        + afterArchetype->Offsets[afterIndexInTypeArray]
                        + afterArchetype->SizeOfs[afterIndexInTypeArray]
                        * afterEntityIndexInChunk;

                    var typeInfo = TypeInfo[afterTypeInArchetype.TypeIndex & TypeManager.ClearFlagsMask];

                    if (UnsafeUtility.MemCmp(beforeAddress, afterAddress, beforeArchetype->SizeOfs[beforeIndexInTypeArray]) != 0
                        || BlobAssetHashesAreDifferent(typeInfo, beforeAddress, afterAddress, 1))
                    {
                        var packedComponent = PackComponent(entityGuid, afterTypeInArchetype.TypeIndex);
                        AppendComponentData(packedComponent, afterAddress, beforeArchetype->SizeOfs[beforeIndexInTypeArray]);
                        ExtractPatches(typeInfo, packedComponent, afterAddress, 1);
                    }
                }
            }

            /// <summary>
            /// IMPORTANT. This function does *NO* validation. It is assumed to be called after a memcmp == 0
            /// </summary>
            bool BlobAssetHashesAreDifferent(
                TypeManager.TypeInfo typeInfo,
                byte* beforeAddress,
                byte* afterAddress,
                int elementCount)
            {
                if (typeInfo.BlobAssetRefOffsetCount == 0)
                    return false;

                var offsets = BlobAssetRefOffsets + typeInfo.BlobAssetRefOffsetStartIndex;

                var elementOffset = 0;

                for (var elementIndex = 0; elementIndex < elementCount; ++elementIndex)
                {
                    for (var offsetIndex = 0; offsetIndex < typeInfo.BlobAssetRefOffsetCount; ++offsetIndex)
                    {
                        var offset = elementOffset + offsets[offsetIndex].Offset;

                        var beforeBlobAssetReference = (BlobAssetReferenceData*)(beforeAddress + offset);
                        var afterBlobAssetReference = (BlobAssetReferenceData*)(afterAddress + offset);

                        if (GetBlobAssetHash(BeforeBlobAssetRemap, beforeBlobAssetReference) != GetBlobAssetHash(AfterBlobAssetRemap, afterBlobAssetReference))
                            return true;
                    }

                    elementOffset += typeInfo.ElementSize;
                }

                return false;
            }

            void ExtractPatches(
                TypeManager.TypeInfo typeInfo,
                PackedComponent component,
                byte* afterAddress,
                int elementCount)
            {
                ExtractEntityReferencePatches(typeInfo, component, afterAddress, elementCount);
                ExtractBlobAssetReferencePatches(typeInfo, component, afterAddress, elementCount);
            }

            void ExtractEntityReferencePatches(
                TypeManager.TypeInfo typeInfo,
                PackedComponent component,
                byte* afterAddress,
                int elementCount)
            {
                if (typeInfo.EntityOffsetCount == 0)
                {
                    return;
                }

                var offsets = EntityOffsets + typeInfo.EntityOffsetStartIndex;

                var elementOffset = 0;

                for (var elementIndex = 0; elementIndex < elementCount; ++elementIndex)
                {
                    for (var offsetIndex = 0; offsetIndex < typeInfo.EntityOffsetCount; ++offsetIndex)
                    {
                        var offset = elementOffset + offsets[offsetIndex].Offset;
                        var entity = *(Entity*)(afterAddress + offset);

                        // If the entity has no guid, then guid will be null (desired)
                        TryGetEntityGuid(AfterEntityComponentStore, entity, out var entityGuid);

                        EntityReferencePatches.Add(new EntityReferenceChange
                        {
                            Component = component,
                            Offset = offset,
                            Value = entityGuid
                        });
                    }

                    elementOffset += typeInfo.ElementSize;
                }
            }

            void ExtractBlobAssetReferencePatches(
                TypeManager.TypeInfo typeInfo,
                PackedComponent component,
                byte* afterAddress,
                int elementCount)
            {
                if (typeInfo.BlobAssetRefOffsetCount == 0)
                {
                    return;
                }

                var offsets = BlobAssetRefOffsets + typeInfo.BlobAssetRefOffsetStartIndex;

                var elementOffset = 0;

                for (var elementIndex = 0; elementIndex < elementCount; ++elementIndex)
                {
                    for (var offsetIndex = 0; offsetIndex < typeInfo.BlobAssetRefOffsetCount; ++offsetIndex)
                    {
                        var offset = elementOffset + offsets[offsetIndex].Offset;
                        var blobAssetReference = (BlobAssetReferenceData*)(afterAddress + offset);
                        var hash = GetBlobAssetHash(AfterBlobAssetRemap, blobAssetReference);

                        BlobAssetReferenceChanges.Add(new BlobAssetReferenceChange
                        {
                            Component = component,
                            Offset = offset,
                            Value = hash
                        });
                    }

                    elementOffset += typeInfo.ElementSize;
                }
            }

            static ulong GetBlobAssetHash(NativeHashMap<BlobAssetReferencePtr, BlobAssetPtr> remap, BlobAssetReferenceData* blobAssetReferenceData)
            {
                if (blobAssetReferenceData->m_Ptr == null)
                    return 0;

                if (remap.IsCreated && remap.TryGetValue(new BlobAssetReferencePtr(blobAssetReferenceData->m_Ptr), out var header))
                    return header.Hash;

                return blobAssetReferenceData->Header->Hash;
            }

            void AppendComponentData(PackedComponent component, void* ptr, int sizeOf)
            {
                SetComponents.Add(new PackedComponentDataChange
                {
                    Component = component,
                    Offset = 0,
                    Size = sizeOf
                });

                ComponentData.AddRange(ptr, sizeOf);
            }

            void AddendSharedComponentData(EntityGuid entityGuid, int typeIndex, int afterSharedComponentIndex, int beforeSharedComponentIndex = -1)
            {
                SharedComponentChanges.Add(new DeferredSharedComponentChange
                {
                    EntityGuid = entityGuid,
                    TypeIndex = typeIndex,
                    AfterSharedComponentIndex = afterSharedComponentIndex,
                    BeforeSharedComponentIndex = beforeSharedComponentIndex
                });
            }

            void AppendManagedComponentData(EntityGuid entityGuid, int typeIndex, int afterManagedComponentIndex, int beforeManagedComponentIndex = -1)
            {
                ManagedComponentChanges.Add(new DeferredManagedComponentChange
                {
                    EntityGuid = entityGuid,
                    TypeIndex = typeIndex,
                    AfterManagedComponentIndex = afterManagedComponentIndex,
                    BeforeManagedComponentIndex = beforeManagedComponentIndex
                });
            }

            PackedComponent PackComponent(EntityGuid entityGuid, int typeIndex)
            {
                var flags = ComponentTypeFlags.None;

                if ((typeIndex & TypeManager.ChunkComponentTypeFlag) != 0)
                    flags |= ComponentTypeFlags.ChunkComponent;

                var packedEntityIndex = Entities.GetOrAdd(entityGuid);
                var packedTypeIndex = ComponentTypes.GetOrAdd(new ComponentTypeHash
                {
                    StableTypeHash = TypeInfo[typeIndex & TypeManager.ClearFlagsMask].StableTypeHash,
                    Flags = flags
                });

                return new PackedComponent
                {
                    PackedEntityIndex = packedEntityIndex,
                    PackedTypeIndex = packedTypeIndex
                };
            }

            bool TryGetEntityGuid(EntityComponentStore* entityComponentStore, Entity entity, out EntityGuid entityGuid)
            {
                entityGuid = default;

                if (!entityComponentStore->HasComponent(entity, EntityGuidTypeIndex))
                {
                    return false;
                }

                entityGuid = *(EntityGuid*)entityComponentStore->GetComponentDataWithTypeRO(entity, EntityGuidTypeIndex);
                return true;
            }
        }

        static readonly PackedSharedComponentDataChange[] s_EmptySetSharedComponentDiff = new PackedSharedComponentDataChange[0];
        static readonly PackedManagedComponentDataChange[] s_EmptySetManagedComponentDiff = new PackedManagedComponentDataChange[0];

        static ComponentChanges GetComponentChanges(
            EntityInChunkChanges entityChanges,
            NativeHashMap<BlobAssetReferencePtr, BlobAssetPtr> afterBlobAssetRemap,
            NativeHashMap<BlobAssetReferencePtr, BlobAssetPtr> beforeBlobAssetRemap,
            Allocator allocator,
            out JobHandle jobHandle,
            JobHandle dependsOn = default)
        {
            var componentChanges = new ComponentChanges(allocator);

            var gatherComponentChanges = new GatherComponentChanges
            {
                EntityGuidTypeIndex = TypeManager.GetTypeIndex<EntityGuid>(),
                LinkedEntityGroupTypeIndex = TypeManager.GetTypeIndex<LinkedEntityGroup>(),
                TypeInfo = TypeManager.GetTypeInfoPointer(),
                EntityOffsets = TypeManager.GetEntityOffsetsPointer(),
                BlobAssetRefOffsets = TypeManager.GetBlobAssetRefOffsetsPointer(),
                AfterEntityComponentStore = entityChanges.AfterEntityManager.GetCheckedEntityDataAccess()->EntityComponentStore,
                BeforeEntityComponentStore = entityChanges.BeforeEntityManager.GetCheckedEntityDataAccess()->EntityComponentStore,
                CreatedEntities = entityChanges.CreatedEntities,
                ModifiedEntities = entityChanges.ModifiedEntities,
                Entities = componentChanges.Entities,
                ComponentTypes = componentChanges.ComponentTypes,
                AddComponents = componentChanges.AddComponents,
                RemoveComponents = componentChanges.RemoveComponents,
                SetComponents = componentChanges.SetComponents,
                EntityReferencePatches = componentChanges.EntityReferenceChanges,
                BlobAssetReferenceChanges = componentChanges.BlobAssetReferenceChanges,
                LinkedEntityGroupAdditions = componentChanges.LinkedEntityGroupAdditions,
                LinkedEntityGroupRemovals = componentChanges.LinkedEntityGroupRemovals,
                ComponentData = componentChanges.ComponentData,
                SharedComponentChanges = componentChanges.SharedComponentChanges,
                ManagedComponentChanges = componentChanges.ManagedComponentChanges,
                AfterBlobAssetRemap = afterBlobAssetRemap,
                BeforeBlobAssetRemap = beforeBlobAssetRemap
            }.Schedule(dependsOn);

            jobHandle = gatherComponentChanges;

            return componentChanges;
        }

        static EntityChangeSet CreateEntityChangeSet(
            EntityInChunkChanges entityInChunkChanges,
            ComponentChanges componentChanges,
            BlobAssetChanges blobAssetChanges,
            Allocator allocator)
        {
            if (!entityInChunkChanges.IsCreated || !componentChanges.IsCreated || !blobAssetChanges.IsCreated)
            {
                return default;
            }
            s_CreateEntityChangeSetProfilerMarker.Begin();

            // IMPORTANT. This can add to the packed collections. It must be done before adding destroyed entities.
            var sharedComponentDataChanges = GetChangedSharedComponents(
                componentChanges.Entities,
                componentChanges.ComponentTypes,
                componentChanges.SharedComponentChanges,
                entityInChunkChanges.BeforeEntityManager.GetCheckedEntityDataAccess()->ManagedComponentStore,
                entityInChunkChanges.AfterEntityManager.GetCheckedEntityDataAccess()->ManagedComponentStore);

            var managedComponentDataChanges = GetChangedManagedComponents(
                componentChanges.Entities,
                componentChanges.ComponentTypes,
                componentChanges.ManagedComponentChanges,
                componentChanges.EntityReferenceChanges,
                entityInChunkChanges.BeforeEntityManager.GetCheckedEntityDataAccess()->EntityComponentStore,
                entityInChunkChanges.AfterEntityManager.GetCheckedEntityDataAccess()->EntityComponentStore,
                entityInChunkChanges.BeforeEntityManager.GetCheckedEntityDataAccess()->ManagedComponentStore,
                entityInChunkChanges.AfterEntityManager.GetCheckedEntityDataAccess()->ManagedComponentStore);

            var entities = componentChanges.Entities.List;

            for (var i = 0; i < entityInChunkChanges.DestroyedEntities.Length; i++)
            {
                entities.Add(entityInChunkChanges.DestroyedEntities[i].EntityGuid);
            }

            var names = GetEntityNames(entities,
                entityInChunkChanges.CreatedEntities,
                entityInChunkChanges.ModifiedEntities,
                entityInChunkChanges.DestroyedEntities,
                entityInChunkChanges.AfterEntityManager,
                entityInChunkChanges.BeforeEntityManager,
                allocator);

            // Allocate and copy in to the results buffers.
            var result = new EntityChangeSet
                (
                entityInChunkChanges.CreatedEntities.Length,
                entityInChunkChanges.DestroyedEntities.Length,
                componentChanges.Entities.List.ToArray(allocator),
                componentChanges.ComponentTypes.List.ToArray(allocator),
                names,
                componentChanges.AddComponents.ToArray(allocator),
                componentChanges.RemoveComponents.ToArray(allocator),
                componentChanges.SetComponents.ToArray(allocator),
                componentChanges.ComponentData.ToArray(allocator),
                componentChanges.EntityReferenceChanges.ToArray(allocator),
                componentChanges.BlobAssetReferenceChanges.ToArray(allocator),
                managedComponentDataChanges,
                sharedComponentDataChanges,
                componentChanges.LinkedEntityGroupAdditions.ToArray(allocator),
                componentChanges.LinkedEntityGroupRemovals.ToArray(allocator),
                blobAssetChanges.CreatedBlobAssets.ToArray(allocator),
                blobAssetChanges.DestroyedBlobAssets.ToArray(allocator),
                blobAssetChanges.BlobAssetData.ToArray(allocator)
                );

            s_CreateEntityChangeSetProfilerMarker.End();
            return result;
        }

        static PackedSharedComponentDataChange[] GetChangedSharedComponents(
            PackedCollection<EntityGuid> packedEntityCollection,
            PackedCollection<ComponentTypeHash> packedStableTypeHashCollection,
            NativeList<DeferredSharedComponentChange> changes,
            ManagedComponentStore beforeManagedComponentStore,
            ManagedComponentStore afterManagedComponentStore)
        {
            if (changes.Length == 0)
            {
                return s_EmptySetSharedComponentDiff;
            }
            s_GetChangedSharedComponentsProfilerMarker.Begin();
            var result = new List<PackedSharedComponentDataChange>();

            for (var i = 0; i < changes.Length; i++)
            {
                var change = changes[i];

                object afterValue = null;

                if (change.AfterSharedComponentIndex == 0 && change.BeforeSharedComponentIndex == 0)
                    continue;

                if (change.AfterSharedComponentIndex != 0)
                {
                    afterValue = afterManagedComponentStore.GetSharedComponentDataBoxed(change.AfterSharedComponentIndex, change.TypeIndex);
                }

                if (change.BeforeSharedComponentIndex > -1 && change.AfterSharedComponentIndex != 0)
                {
                    var beforeValue = beforeManagedComponentStore.GetSharedComponentDataBoxed(change.BeforeSharedComponentIndex, change.TypeIndex);

                    if (TypeManager.Equals(beforeValue, afterValue, change.TypeIndex))
                    {
                        continue;
                    }
                }

                var packedEntityIndex = packedEntityCollection.GetOrAdd(change.EntityGuid);
                var packedTypeIndex = packedStableTypeHashCollection.GetOrAdd(new ComponentTypeHash
                {
                    StableTypeHash = TypeManager.GetTypeInfo(change.TypeIndex).StableTypeHash
                });

                var packedComponent = new PackedComponent
                {
                    PackedEntityIndex = packedEntityIndex,
                    PackedTypeIndex = packedTypeIndex
                };

                (afterValue as IRefCounted)?.Retain();

                result.Add(new PackedSharedComponentDataChange
                {
                    Component = packedComponent,
                    BoxedSharedValue = afterValue
                });
            }
            s_GetChangedSharedComponentsProfilerMarker.End();
            return result.ToArray();
        }

        static PackedManagedComponentDataChange[] GetChangedManagedComponents(
            PackedCollection<EntityGuid> packedEntityCollection,
            PackedCollection<ComponentTypeHash> packedStableTypeHashCollection,
            NativeList<DeferredManagedComponentChange> changes,
            NativeList<EntityReferenceChange> patches,
            EntityComponentStore* beforeEntityComponentStore,
            EntityComponentStore* afterEntityComponentStore,
            ManagedComponentStore beforeManagedComponentStore,
            ManagedComponentStore afterManagedComponentStore)
        {
            if (changes.Length == 0)
            {
                return s_EmptySetManagedComponentDiff;
            }
            s_GetChangedManagedComponentsProfilerMarker.Begin();

            var entityGuidTypeIndex = TypeManager.GetTypeIndex<EntityGuid>();
            var result = new List<PackedManagedComponentDataChange>();

            for (var i = 0; i < changes.Length; i++)
            {
                var change = changes[i];
                var typeInfo = TypeManager.GetTypeInfo(change.TypeIndex);

                if (typeInfo.Category == TypeManager.TypeCategory.Class)
                {
                    // Hybrid Components should be ignored in the diff, the Companion Link will clone the Companion GameObject
                    // and when we apply the diff we'll relink the Hybrid Component to the ones from the Companion GameObject
                    continue;
                }

                var afterValue = afterManagedComponentStore.GetManagedComponent(change.AfterManagedComponentIndex);

                if (change.BeforeManagedComponentIndex > -1)
                {
                    var beforeValue = beforeManagedComponentStore.GetManagedComponent(change.BeforeManagedComponentIndex);

                    if (TypeManager.Equals(beforeValue, afterValue, change.TypeIndex))
                    {
                        continue;
                    }
                }

                var packedEntityIndex = packedEntityCollection.GetOrAdd(change.EntityGuid);
                var packedTypeIndex = packedStableTypeHashCollection.GetOrAdd(new ComponentTypeHash
                {
                    StableTypeHash = typeInfo.StableTypeHash
                });

                var packedComponent = new PackedComponent
                {
                    PackedEntityIndex = packedEntityIndex,
                    PackedTypeIndex = packedTypeIndex
                };

                afterValue = ManagedComponentStore.CloneManagedComponent(afterValue);

                result.Add(new PackedManagedComponentDataChange
                {
                    Component = packedComponent,
                    BoxedValue = afterValue
                });

                if (typeInfo.EntityOffsetCount > 0)
                {
                    AddEntityPatchesForObject(afterValue, packedComponent, patches, afterEntityComponentStore, entityGuidTypeIndex);
                }
            }
            s_GetChangedManagedComponentsProfilerMarker.End();
            return result.ToArray();
        }

        /// <summary>
        /// This method returns all entity names for the given array of entityGuid components.
        /// </summary>
        /// <remarks>
        /// This method relies on the source buffers the entityGuids was built from. While this could technically be done
        /// while building the entityGuid set, it's a bit more isolated this way so we can remove it easily in the future.
        /// </remarks>
        static NativeArray<NativeString64> GetEntityNames(
            NativeList<EntityGuid> entityGuids,
            NativeList<CreatedEntity> createdEntities,
            NativeList<ModifiedEntity> modifiedEntities,
            NativeList<DestroyedEntity> destroyedEntities,
            EntityManager afterEntityManager,
            EntityManager beforeEntityManager,
            Allocator allocator)
        {
            s_GetEntityNamesProfilerMarker.Begin();
            var names = new NativeArray<NativeString64>(entityGuids.Length, allocator);
            var index = 0;

            // Created entities will ALWAYS show up in the entityGuid set so we can safely grab the names.
            // They will exist in the after world.
            for (var i = 0; i < createdEntities.Length; i++)
            {
                var name = new NativeString64();
#if UNITY_EDITOR
                name.CopyFrom(afterEntityManager.GetName(ChunkDataUtility.GetEntityFromEntityInChunk(createdEntities[i].AfterEntityInChunk)));
#endif
                names[index++] = name;
            }

            // Scan through the potentially modified entities and extract names for ones that were actually changed.
            // Use the after world name.
            var entityGuidIndex = createdEntities.Length;
            for (var i = 0; i < modifiedEntities.Length && entityGuidIndex < entityGuids.Length; i++)
            {
                if (!modifiedEntities[i].EntityGuid.Equals(entityGuids[entityGuidIndex]))
                {
                    continue;
                }

                var name = new NativeString64();
#if UNITY_EDITOR
                name.CopyFrom(afterEntityManager.GetName(ChunkDataUtility.GetEntityFromEntityInChunk(modifiedEntities[i].AfterEntityInChunk)));
#endif
                names[index++] = name;
                entityGuidIndex++;
            }

            // Destroyed entities will always show up int he entityGuid set so we can grab the rest of those names.
            // The will not exist in the after world so use the before world.
            for (var i = 0; i < destroyedEntities.Length; i++)
            {
                var name = new NativeString64();
#if UNITY_EDITOR
                name.CopyFrom(beforeEntityManager.GetName(ChunkDataUtility.GetEntityFromEntityInChunk(destroyedEntities[i].BeforeEntityInChunk)));
#endif
                names[index++] = name;
            }
            s_GetEntityNamesProfilerMarker.End();
            return names;
        }

        internal static void AddEntityPatchesForObject(object container, PackedComponent component, NativeList<EntityReferenceChange> patches,
            EntityComponentStore* afterEntityComponentStore, int entityGuidTypeIndex)
        {
#if !NET_DOTS
            PropertyContainer.Visit(container, new EntityDiffWriter(component, patches, afterEntityComponentStore, entityGuidTypeIndex));
#endif
        }

#if !NET_DOTS
        class EntityDiffWriter : PropertyVisitor
        {
            public EntityDiffWriter(PackedComponent component, NativeList<EntityReferenceChange> patches, EntityComponentStore* afterEntityComponentStore, int entityGuidTypeIndex)
            {
                AddAdapter(new EntityDiffAdapter(component, patches, afterEntityComponentStore, entityGuidTypeIndex));
            }

            class EntityDiffAdapter : Properties.Adapters.IVisit<Entity>
            {
                readonly PackedComponent _Component;
                readonly NativeList<EntityReferenceChange> _Patches;
                readonly EntityComponentStore* _AfterEntityComponentStore;
                readonly int _EntityGuidTypeIndex;

                int _EntityPatchId;

                public EntityDiffAdapter(PackedComponent component, NativeList<EntityReferenceChange> patches, EntityComponentStore* afterEntityComponentStore, int entityGuidTypeIndex)
                {
                    _Component = component;
                    _Patches = patches;
                    _AfterEntityComponentStore = afterEntityComponentStore;
                    _EntityGuidTypeIndex = entityGuidTypeIndex;
                    _EntityPatchId = 0;
                }

                public VisitStatus Visit<TContainer>(Property<TContainer, Entity> property, ref TContainer container, ref Entity value)
                {
                    TryGetEntityGuid(value, out var entityGuid);

                    value = new Entity { Index = _EntityPatchId, Version = -1 };

                    _Patches.Add(new EntityReferenceChange
                    {
                        Component = _Component,
                        Offset = _EntityPatchId++,
                        Value = entityGuid
                    });

                    return VisitStatus.Stop;
                }

                bool TryGetEntityGuid(Entity e, out EntityGuid guid)
                {
                    guid = default;

                    if (!_AfterEntityComponentStore->HasComponent(e, _EntityGuidTypeIndex))
                    {
                        return false;
                    }

                    guid = *(EntityGuid*)_AfterEntityComponentStore->GetComponentDataWithTypeRO(e, _EntityGuidTypeIndex);
                    return true;
                }
            }
        }
#endif
    }
}
