using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEditor;
using Hash128 = Unity.Entities.Hash128;

namespace Unity.Scenes
{
    static unsafe class EntityChangeSetSerialization
    {
        public struct ResourcePacket : IDisposable
        {
            public NativeArray<RuntimeGlobalObjectId> GlobalObjectIds;
            public UnsafeAppendBuffer                 ChangeSet;

            public ResourcePacket(byte[] buffer)
            {
                fixed(byte* ptr = buffer)
                {
                    var bufferReader = new UnsafeAppendBuffer.Reader(ptr, buffer.Length);

                    bufferReader.ReadNext(out GlobalObjectIds, Allocator.Persistent);

                    var entityChangeSetSourcePtr = bufferReader.Ptr + bufferReader.Offset;
                    var entityChangeSetSourceSize = bufferReader.Size - bufferReader.Offset;
                    ChangeSet = new UnsafeAppendBuffer(entityChangeSetSourceSize, 16, Allocator.Persistent);
                    ChangeSet.Add(entityChangeSetSourcePtr, entityChangeSetSourceSize);
                }
            }

            public void Dispose()
            {
                GlobalObjectIds.Dispose();
                ChangeSet.Dispose();
            }

#if UNITY_EDITOR
            unsafe public static void SerializeResourcePacket(EntityChangeSet entityChangeSet, ref UnsafeAppendBuffer buffer)
            {
                var changeSetBuffer = new UnsafeAppendBuffer(1024, 16, Allocator.TempJob);
                Serialize(entityChangeSet, &changeSetBuffer, out var globalObjectIds);

                buffer.Add(globalObjectIds);
                buffer.Add(changeSetBuffer.Ptr, changeSetBuffer.Length);

                changeSetBuffer.Dispose();
                globalObjectIds.Dispose();
            }

#endif
        }

        public static EntityChangeSet Deserialize(UnsafeAppendBuffer.Reader* bufferReader, NativeArray<RuntimeGlobalObjectId> globalObjectIDs, GlobalAssetObjectResolver resolver)
        {
            bufferReader->ReadNext<ComponentTypeHash>(out var typeHashes, Allocator.Persistent);

            for (int i = 0; i != typeHashes.Length; i++)
            {
                var stableTypeHash = typeHashes[i].StableTypeHash;
                var typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(stableTypeHash);
                if (typeIndex == -1)
                {
                    typeHashes.Dispose();
                    throw new ArgumentException("The LiveLink Patch Type Layout doesn't match the Data Layout of the Components. Please Rebuild the Player.");
                }
            }

            var createdEntityCount = bufferReader->ReadNext<int>();
            var destroyedEntityCount = bufferReader->ReadNext<int>();
            bufferReader->ReadNext<EntityGuid>(out var entities, Allocator.Persistent);
            bufferReader->ReadNext<NativeString64>(out var names, Allocator.Persistent);
            bufferReader->ReadNext<PackedComponent>(out var addComponents, Allocator.Persistent);
            bufferReader->ReadNext<PackedComponent>(out var removeComponents, Allocator.Persistent);
            bufferReader->ReadNext<PackedComponentDataChange>(out var setComponents, Allocator.Persistent);
            bufferReader->ReadNext<byte>(out var componentData, Allocator.Persistent);
            bufferReader->ReadNext<EntityReferenceChange>(out var entityReferenceChanges, Allocator.Persistent);
            bufferReader->ReadNext<BlobAssetReferenceChange>(out var blobAssetReferenceChanges, Allocator.Persistent);
            bufferReader->ReadNext<LinkedEntityGroupChange>(out var linkedEntityGroupAdditions, Allocator.Persistent);
            bufferReader->ReadNext<LinkedEntityGroupChange>(out var linkedEntityGroupRemovals, Allocator.Persistent);
            bufferReader->ReadNext<BlobAssetChange>(out var createdBlobAssets, Allocator.Persistent);
            bufferReader->ReadNext<ulong>(out var destroyedBlobAssets, Allocator.Persistent);
            bufferReader->ReadNext<byte>(out var blobAssetData, Allocator.Persistent);

            var resolvedObjects = new UnityEngine.Object[globalObjectIDs.Length];
            resolver.ResolveObjects(globalObjectIDs, resolvedObjects);
            var reader = new ManagedObjectBinaryReader(bufferReader, resolvedObjects);

            var setSharedComponents = ReadSharedComponentDataChanges(bufferReader, reader, typeHashes);
            var setManagedComponents = ReadManagedComponentDataChanges(bufferReader, reader, typeHashes);

            //if (!bufferReader->EndOfBuffer)
            //    throw new Exception("Underflow in EntityChangeSet buffer");

            return new EntityChangeSet(
                createdEntityCount,
                destroyedEntityCount,
                entities,
                typeHashes,
                names,
                addComponents,
                removeComponents,
                setComponents,
                componentData,
                entityReferenceChanges,
                blobAssetReferenceChanges,
                setManagedComponents,
                setSharedComponents,
                linkedEntityGroupAdditions,
                linkedEntityGroupRemovals,
                createdBlobAssets,
                destroyedBlobAssets,
                blobAssetData);
        }

        static PackedSharedComponentDataChange[] ReadSharedComponentDataChanges(UnsafeAppendBuffer.Reader* buffer, ManagedObjectBinaryReader reader, NativeArray<ComponentTypeHash> typeHashes)
        {
            buffer->ReadNext<PackedComponent>(out var packedComponents, Allocator.Temp);
            var setSharedComponentDataChanges = new PackedSharedComponentDataChange[packedComponents.Length];
            for (var i = 0; i < packedComponents.Length; i++)
            {
                var packedTypeIndex = packedComponents[i].PackedTypeIndex;
                var stableTypeHash = typeHashes[packedTypeIndex].StableTypeHash;
                var typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(stableTypeHash);
                var type = TypeManager.GetType(typeIndex);
                var componentValue = reader.ReadObject(type);
                setSharedComponentDataChanges[i].Component = packedComponents[i];
                setSharedComponentDataChanges[i].BoxedSharedValue = componentValue;
            }
            packedComponents.Dispose();
            return setSharedComponentDataChanges;
        }

        static PackedManagedComponentDataChange[] ReadManagedComponentDataChanges(UnsafeAppendBuffer.Reader* buffer, ManagedObjectBinaryReader reader, NativeArray<ComponentTypeHash> typeHashes)
        {
            buffer->ReadNext<PackedComponent>(out var packedComponents, Allocator.Temp);
            var setManagedComponentDataChanges = new PackedManagedComponentDataChange[packedComponents.Length];
            for (var i = 0; i < packedComponents.Length; i++)
            {
                var packedTypeIndex = packedComponents[i].PackedTypeIndex;
                var stableTypeHash = typeHashes[packedTypeIndex].StableTypeHash;
                var typeIndex = TypeManager.GetTypeIndexFromStableTypeHash(stableTypeHash);
                var type = TypeManager.GetType(typeIndex);
                var componentValue = reader.ReadObject(type);
                setManagedComponentDataChanges[i].Component = packedComponents[i];
                setManagedComponentDataChanges[i].BoxedValue = componentValue;
            }
            packedComponents.Dispose();
            return setManagedComponentDataChanges;
        }

#if UNITY_EDITOR

        public static void Serialize(EntityChangeSet entityChangeSet, UnsafeAppendBuffer* buffer, out NativeArray<RuntimeGlobalObjectId> outAssets)
        {
            // @FIXME Workaround to solve an issue with hybrid components, LiveLink player builds do NOT support hybrid components.
            //
            // An implementation detail of hybrid components is the `CompanionLink` component.
            // This component is used to link an entity to a GameObject which hosts all of the hybrid components.
            // This companion GameObject lives in the scene and is NOT being serialized across to the player yet.
            //
            // In order to avoid crashing the companion link system in the player build we strip this component during serialization.
            //
            var companionLinkPackedTypeIndex = GetCompanionLinkPackedTypeIndex(entityChangeSet.TypeHashes);
            var addComponentsWithoutCompanionLinks = GetPackedComponentsWithoutCompanionLinks(entityChangeSet.AddComponents, companionLinkPackedTypeIndex, Allocator.Temp);
            var removeComponentWithoutCompanionLinks = GetPackedComponentsWithoutCompanionLinks(entityChangeSet.RemoveComponents, companionLinkPackedTypeIndex, Allocator.Temp);
            var setManagedComponentWithoutCompanionLinks = GetPackedManagedComponentChangesWithoutCompanionLinks(entityChangeSet.SetManagedComponents, companionLinkPackedTypeIndex);

            // Write EntityChangeSet
            buffer->Add(entityChangeSet.TypeHashes);
            buffer->Add(entityChangeSet.CreatedEntityCount);
            buffer->Add(entityChangeSet.DestroyedEntityCount);
            buffer->Add(entityChangeSet.Entities);
            buffer->Add(entityChangeSet.Names);
            buffer->Add(addComponentsWithoutCompanionLinks.AsArray());
            buffer->Add(removeComponentWithoutCompanionLinks.AsArray());
            buffer->Add(entityChangeSet.SetComponents);
            buffer->Add(entityChangeSet.ComponentData);
            buffer->Add(entityChangeSet.EntityReferenceChanges);
            buffer->Add(entityChangeSet.BlobAssetReferenceChanges);
            buffer->Add(entityChangeSet.LinkedEntityGroupAdditions);
            buffer->Add(entityChangeSet.LinkedEntityGroupRemovals);
            buffer->Add(entityChangeSet.CreatedBlobAssets);
            buffer->Add(entityChangeSet.DestroyedBlobAssets);
            buffer->Add(entityChangeSet.BlobAssetData);

            var writer = new ManagedObjectBinaryWriter(buffer);

            WriteSharedComponentDataChanges(buffer, writer, entityChangeSet.SetSharedComponents);
            WriteManagedComponentDataChanges(buffer, writer, setManagedComponentWithoutCompanionLinks);

            var objectTable = writer.GetObjectTable();
            var globalObjectIds = new GlobalObjectId[objectTable.Length];
            GlobalObjectId.GetGlobalObjectIdsSlow(objectTable, globalObjectIds);

            outAssets = new NativeArray<RuntimeGlobalObjectId>(globalObjectIds.Length, Allocator.Persistent);
            for (int i = 0; i != globalObjectIds.Length; i++)
            {
                var globalObjectId = globalObjectIds[i];

                //@TODO: HACK (Object is a scene object)
                if (globalObjectId.identifierType == 2)
                {
                    Debug.LogWarning($"{objectTable[i]} is part of a scene, LiveLink can't transfer scene objects. (Note: LiveConvertSceneView currently triggers this)");
                    globalObjectId = new GlobalObjectId();
                }

                if (globalObjectId.assetGUID == new GUID())
                {
                    //@TODO: How do we handle this
                    Debug.LogWarning($"{objectTable[i]} has no valid GUID. LiveLink currently does not support built-in assets.");
                    globalObjectId = new GlobalObjectId();
                }

                outAssets[i] = Unsafe.AsRef<RuntimeGlobalObjectId>(&globalObjectId);
            }

            addComponentsWithoutCompanionLinks.Dispose();
            removeComponentWithoutCompanionLinks.Dispose();
        }

        static void WriteSharedComponentDataChanges(UnsafeAppendBuffer* buffer, ManagedObjectBinaryWriter writer, PackedSharedComponentDataChange[] changes)
        {
            buffer->Add(changes.Length);

            for (var i = 0; i < changes.Length; i++)
                buffer->Add(changes[i].Component);

            for (var i = 0; i < changes.Length; i++)
                writer.WriteObject(changes[i].BoxedSharedValue);
        }

        static void WriteManagedComponentDataChanges(UnsafeAppendBuffer* buffer, ManagedObjectBinaryWriter writer, PackedManagedComponentDataChange[] changes)
        {
            buffer->Add(changes.Length);

            for (var i = 0; i < changes.Length; i++)
                buffer->Add(changes[i].Component);

            for (var i = 0; i < changes.Length; i++)
                writer.WriteObject(changes[i].BoxedValue);
        }

        static NativeList<PackedComponent> GetPackedComponentsWithoutCompanionLinks(NativeArray<PackedComponent> components, int companionLinkPackedTypeIndex, Allocator allocator)
        {
            var list = new NativeList<PackedComponent>(components.Length, allocator);

            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].PackedTypeIndex == companionLinkPackedTypeIndex)
                    continue;

                list.AddNoResize(components[i]);
            }

            return list;
        }

        static PackedManagedComponentDataChange[] GetPackedManagedComponentChangesWithoutCompanionLinks(PackedManagedComponentDataChange[] changes, int companionLinkPackedTypeIndex)
        {
            return companionLinkPackedTypeIndex == -1
                ? changes
                : changes.Where(x => x.Component.PackedTypeIndex != companionLinkPackedTypeIndex).ToArray();
        }

        static int GetCompanionLinkPackedTypeIndex(NativeArray<ComponentTypeHash> typeHashes)
        {
#if !UNITY_DISABLE_MANAGED_COMPONENTS
            var companionLinkTypeInfo = TypeManager.GetTypeInfo<CompanionLink>();
            for (var i = 0; i < typeHashes.Length; i++)
            {
                if (typeHashes[i].StableTypeHash == companionLinkTypeInfo.StableTypeHash)
                    return i;
            }
#endif
            return -1;
        }

#endif
    }
}
