using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Conversion;
using Unity.Entities.Serialization;
using Unity.Entities.Streaming;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.VersionControl;
using static Unity.Entities.GameObjectConversionUtility;
using Hash128 = Unity.Entities.Hash128;
using UnityObject = UnityEngine.Object;
using AssetImportContext = UnityEditor.Experimental.AssetImporters.AssetImportContext;

namespace Unity.Scenes.Editor
{
    public static class EditorEntityScenes
    {
        static readonly ProfilerMarker k_ProfileEntitiesSceneSave = new ProfilerMarker("EntitiesScene.Save");
        static readonly ProfilerMarker k_ProfileEntitiesSceneSaveHeader = new ProfilerMarker("EntitiesScene.WriteHeader");
        static readonly ProfilerMarker k_ProfileEntitiesSceneSaveConversionLog = new ProfilerMarker("EntitiesScene.WriteConversionLog");
        static readonly ProfilerMarker k_ProfileEntitiesSceneWriteObjRefs = new ProfilerMarker("EntitiesScene.WriteObjectReferences");

        public static bool IsEntitySubScene(Scene scene)
        {
            return scene.isSubScene;
        }

        static AABB GetBoundsAndRemove(EntityManager entityManager, EntityQuery query)
        {
            var bounds = MinMaxAABB.Empty;
            using (var allBounds = query.ToComponentDataArray<SceneBoundingVolume>(Allocator.TempJob))
            {
                foreach (var b in allBounds)
                    bounds.Encapsulate(b.Value);
            }

            using (var entities = query.ToEntityArray(Allocator.TempJob))
            {
                foreach (var e in entities)
                {
                    // Query includes SceneBoundingVolume & SceneSection
                    // If thats the only data, just destroy the entity
                    if (entityManager.GetComponentCount(e) == 2)
                        entityManager.DestroyEntity(e);
                    else
                        entityManager.RemoveComponent<SceneBoundingVolume>(e);
                }
            }

            return bounds;
        }

        internal static string GetSceneWritePath(EntityScenesPaths.PathType type, string subsectionName, AssetImportContext ctx)
        {
            var prefix = string.IsNullOrEmpty(subsectionName) ? "" : subsectionName + ".";
            var path = ctx.GetResultPath(prefix + EntityScenesPaths.GetExtension(type));

            return path;
        }

        static void AddRetainBlobAssetsEntity(EntityManager manager, int framesToRetainBlobAssets)
        {
            var entity = manager.CreateEntity(typeof(RetainBlobAssets));
            manager.SetComponentData(entity, new RetainBlobAssets { FramesToRetainBlobAssets = framesToRetainBlobAssets });
        }

        static void RegisterDependencies(AssetImportContext importContext, ConversionDependencies dependencies)
        {
            using (var assets = dependencies.AssetDependentsByInstanceId.GetKeyArray(Allocator.Temp))
            {
                for (int i = 0; i < assets.Length; i++)
                {
                    var asset = EditorUtility.InstanceIDToObject(assets[i]);
                    importContext.DependsOnSourceAsset(AssetDatabase.GetAssetPath(asset));
                }
            }
        }

        public static SceneSectionData[] ConvertAndWriteEntityScene(Scene scene, GameObjectConversionSettings settings, List<ReferencedUnityObjects> sectionRefObjs = null)
        {
            var world = new World("ConversionWorld");
            settings.DestinationWorld = world;

            bool disposeBlobAssetCache = false;
            if (settings.BlobAssetStore == null)
            {
                settings.BlobAssetStore = new BlobAssetStore();
                disposeBlobAssetCache = true;
            }

            SceneSectionData[] sections = null;
            settings.ConversionWorldPreDispose += conversionWorld =>
            {
                var mappingSystem = conversionWorld.GetExistingSystem<GameObjectConversionMappingSystem>();
                if (settings.AssetImportContext != null)
                    RegisterDependencies(settings.AssetImportContext, mappingSystem.Dependencies);

                // Optimizing and writing the scene is done here to include potential log messages in the conversion log.
                EntitySceneOptimization.Optimize(world);
                int framesToRetainBlobAssets = RetainBlobAssetsSetting.GetFramesToRetainBlobAssets(settings.BuildConfiguration);
                sections = WriteEntityScene(world.EntityManager, settings.SceneGUID, scene.name, settings.AssetImportContext, framesToRetainBlobAssets, sectionRefObjs);

                var journalData = mappingSystem.JournalData.SelectLogEventsOrdered().ToList();
                // Save the log of issues that happened during conversion
                WriteConversionLog(settings.SceneGUID, journalData, scene.name, settings.AssetImportContext);
            };

            ConvertScene(scene, settings);

            if (disposeBlobAssetCache)
            {
                settings.BlobAssetStore.Dispose();
            }

            world.Dispose();

            return sections;
        }

        public static SceneSectionData[] WriteEntityScene(EntityManager entityManager, Hash128 sceneGUID, string sceneName, AssetImportContext importContext, int framesToRetainBlobAssets = 0, List<ReferencedUnityObjects> sectionRefObjs = null)
        {
            if (importContext != null)
            {
                using (var allTypes = new NativeHashMap<ComponentType, int>(100, Allocator.Temp))
                using (var archetypes = new NativeList<EntityArchetype>(Allocator.Temp))
                {
                    entityManager.GetAllArchetypes(archetypes);
                    foreach (var archetype in archetypes)
                    {
                        using (var componentTypes = archetype.GetComponentTypes())
                            foreach (var componentType in componentTypes)
                                if (allTypes.TryAdd(componentType, 0))
                                    TypeDependencyCache.AddDependency(importContext, componentType);
                    }
                }

                TypeDependencyCache.AddAllSystemsDependency(importContext);
            }


            var sceneSections = new List<SceneSectionData>();

            var subSectionList = new List<SceneSection>();
            entityManager.GetAllUniqueSharedComponentData(subSectionList);
            //Order sections by section id
            subSectionList.Sort(Comparer<SceneSection>.Create((a, b) => a.Section.CompareTo(b.Section)));

            var extRefInfoEntities = new NativeArray<Entity>(subSectionList.Count, Allocator.Temp);

            NativeArray<Entity> entitiesInMainSection;

            var sectionQuery = entityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new[] {ComponentType.ReadWrite<SceneSection>()},
                    Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled
                }
            );

            var sectionBoundsQuery = entityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new[] {ComponentType.ReadWrite<SceneBoundingVolume>(), ComponentType.ReadWrite<SceneSection>()},
                    Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled
                }
            );

            {
                var section = new SceneSection {SceneGUID = sceneGUID, Section = 0};
                sectionQuery.SetSharedComponentFilter(new SceneSection { SceneGUID = sceneGUID, Section = 0 });
                sectionBoundsQuery.SetSharedComponentFilter(new SceneSection { SceneGUID = sceneGUID, Section = 0 });
                entitiesInMainSection = sectionQuery.ToEntityArray(Allocator.TempJob);


                var bounds = GetBoundsAndRemove(entityManager, sectionBoundsQuery);

                // Each section will be serialized in its own world, entities that don't have a section are part of the main scene.
                // An entity that holds the array of external references to the main scene is required for each section.
                // We need to create them all before we start moving entities to section scenes,
                // otherwise they would reuse entities that have been moved and mess up the remapping tables.
                for (int sectionIndex = 1; sectionIndex < subSectionList.Count; ++sectionIndex)
                {
                    if (subSectionList[sectionIndex].Section == 0)
                        // Main section, the only one that doesn't need an external ref array
                        continue;

                    var extRefInfoEntity = entityManager.CreateEntity();
                    entityManager.AddSharedComponentData(extRefInfoEntity, subSectionList[sectionIndex]);
                    extRefInfoEntities[sectionIndex] = extRefInfoEntity;
                }

                // Public references array, only on the main section.
                var refInfoEntity = entityManager.CreateEntity();
                entityManager.AddBuffer<PublicEntityRef>(refInfoEntity);
                entityManager.AddSharedComponentData(refInfoEntity, section);
                var publicRefs = entityManager.GetBuffer<PublicEntityRef>(refInfoEntity);

//                entityManager.Debug.CheckInternalConsistency();

                //@TODO do we need to keep this index? doesn't carry any additional info
                for (int i = 0; i < entitiesInMainSection.Length; ++i)
                {
                    PublicEntityRef.Add(ref publicRefs,
                        new PublicEntityRef {entityIndex = i, targetEntity = entitiesInMainSection[i]});
                }

                UnityEngine.Debug.Assert(publicRefs.Length == entitiesInMainSection.Length);

                // Save main section
                var sectionWorld = new World("SectionWorld");
                var sectionManager = sectionWorld.EntityManager;

                var entityRemapping = entityManager.CreateEntityRemapArray(Allocator.TempJob);
                sectionManager.MoveEntitiesFrom(entityManager, sectionQuery, entityRemapping);

                AddRetainBlobAssetsEntity(sectionManager, framesToRetainBlobAssets);

                // The section component is only there to break the conversion world into different sections
                // We don't want to store that on the disk
                //@TODO: Component should be removed but currently leads to corrupt data file. Figure out why.
                //sectionManager.RemoveComponent(sectionManager.UniversalQuery, typeof(SceneSection));

                var sectionFileSize = WriteEntitySceneSection(sectionManager, "0", importContext, out var objectRefCount, out var objRefs);
                sectionRefObjs?.Add(objRefs);
                sceneSections.Add(new SceneSectionData
                {
                    FileSize = sectionFileSize,
                    SceneGUID = sceneGUID,
                    ObjectReferenceCount = objectRefCount,
                    SubSectionIndex = 0,
                    BoundingVolume = bounds
                });

                entityRemapping.Dispose();
                sectionWorld.Dispose();
            }

            {
                // Index 0 is the default value of the shared component, not an actual section
                for (int subSectionIndex = 0; subSectionIndex < subSectionList.Count; ++subSectionIndex)
                {
                    var subSection = subSectionList[subSectionIndex];
                    if (subSection.Section == 0)
                        continue;

                    sectionQuery.SetSharedComponentFilter(subSection);
                    sectionBoundsQuery.SetSharedComponentFilter(subSection);

                    var bounds = GetBoundsAndRemove(entityManager, sectionBoundsQuery);

                    var entitiesInSection = sectionQuery.ToEntityArray(Allocator.TempJob);

                    if (entitiesInSection.Length > 0)
                    {
                        // Fetch back the external reference entity we created earlier to not disturb the mapping
                        var refInfoEntity = extRefInfoEntities[subSectionIndex];
                        entityManager.AddBuffer<ExternalEntityRef>(refInfoEntity);
                        var externRefs = entityManager.GetBuffer<ExternalEntityRef>(refInfoEntity);

                        // Store the mapping to everything in the main section
                        //@TODO maybe we don't need all that? is this worth worrying about?
                        for (int i = 0; i < entitiesInMainSection.Length; ++i)
                        {
                            ExternalEntityRef.Add(ref externRefs, new ExternalEntityRef {entityIndex = i});
                        }

                        var entityRemapping = entityManager.CreateEntityRemapArray(Allocator.TempJob);

                        // Entities will be remapped to a contiguous range in the section world, but they will
                        // also come with an unpredictable amount of meta entities. We have the guarantee that
                        // the entities in the main section won't be moved over, so there's a free range of that
                        // size at the end of the remapping table. So we use that range for external references.
                        var externEntityIndexStart = entityRemapping.Length - entitiesInMainSection.Length;

                        entityManager.AddComponentData(refInfoEntity,
                            new ExternalEntityRefInfo
                            {
                                SceneGUID = sceneGUID,
                                EntityIndexStart = externEntityIndexStart
                            });

                        var sectionWorld = new World("SectionWorld");
                        var sectionManager = sectionWorld.EntityManager;

                        // Insert mapping for external references, conversion world entity to virtual index in section
                        for (int i = 0; i < entitiesInMainSection.Length; ++i)
                        {
                            EntityRemapUtility.AddEntityRemapping(ref entityRemapping, entitiesInMainSection[i],
                                new Entity {Index = i + externEntityIndexStart, Version = 1});
                        }

                        sectionManager.MoveEntitiesFrom(entityManager, sectionQuery, entityRemapping);

                        AddRetainBlobAssetsEntity(sectionManager, framesToRetainBlobAssets);
                        // Now that all the required entities have been moved over, we can get rid of the gap between
                        // real entities and external references. This allows remapping during load to deal with a
                        // smaller remap table, containing only useful entries.

                        int highestEntityIndexInUse = 0;
                        for (int i = 0; i < externEntityIndexStart; ++i)
                        {
                            var targetIndex = entityRemapping[i].Target.Index;
                            if (targetIndex < externEntityIndexStart && targetIndex > highestEntityIndexInUse)
                                highestEntityIndexInUse = targetIndex;
                        }

                        var oldExternEntityIndexStart = externEntityIndexStart;
                        externEntityIndexStart = highestEntityIndexInUse + 1;

                        sectionManager.SetComponentData
                            (
                                EntityRemapUtility.RemapEntity(ref entityRemapping, refInfoEntity),
                                new ExternalEntityRefInfo
                                {
                                    SceneGUID = sceneGUID,
                                    EntityIndexStart = externEntityIndexStart
                                }
                            );

                        // When writing the scene, references to missing entities are set to Entity.Null by default
                        // (but only if they have been used, otherwise they remain untouched)
                        // We obviously don't want that to happen to our external references, so we add explicit mapping
                        // And at the same time, we put them back at the end of the effective range of real entities.
                        for (int i = 0; i < entitiesInMainSection.Length; ++i)
                        {
                            var src = new Entity {Index = i + oldExternEntityIndexStart, Version = 1};
                            var dst = new Entity {Index = i + externEntityIndexStart, Version = 1};
                            EntityRemapUtility.AddEntityRemapping(ref entityRemapping, src, dst);
                        }

                        // The section component is only there to break the conversion world into different sections
                        // We don't want to store that on the disk
                        //@TODO: Component should be removed but currently leads to corrupt data file. Figure out why.
                        //sectionManager.RemoveComponent(sectionManager.UniversalQuery, typeof(SceneSection));
                        var fileSize = WriteEntitySceneSection(sectionManager, subSection.Section.ToString(), importContext, out var objectRefCount, out var objRefs, entityRemapping);
                        sectionRefObjs?.Add(objRefs);
                        sceneSections.Add(new SceneSectionData
                        {
                            FileSize = fileSize,
                            SceneGUID = sceneGUID,
                            ObjectReferenceCount = objectRefCount,
                            SubSectionIndex = subSection.Section,
                            BoundingVolume = bounds
                        });

                        entityRemapping.Dispose();
                        sectionWorld.Dispose();
                    }

                    entitiesInSection.Dispose();
                }
            }

            {
                var noSectionQuery = entityManager.CreateEntityQuery(
                    new EntityQueryDesc
                    {
                        None = new[] {ComponentType.ReadWrite<SceneSection>()},
                        Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled
                    }
                );
                var sectionEntityQuery = entityManager.CreateEntityQuery(
                    new EntityQueryDesc
                    {
                        None = new[] {ComponentType.ReadWrite<SectionMetadataSetup>()},
                        Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled
                    }
                );
                var notSerializedCount = noSectionQuery.CalculateEntityCount() - sectionEntityQuery.CalculateEntityCount();
                if (notSerializedCount != 0)
                    Debug.LogWarning($"{notSerializedCount} entities in the scene '{sceneName}' had no SceneSection and as a result were not serialized at all.");
            }

            // Save the new header
            var sceneSectionsArray = sceneSections.ToArray();
            WriteSceneHeader(sceneGUID, sceneSectionsArray, sceneName, importContext, entityManager);

            sectionQuery.Dispose();
            sectionBoundsQuery.Dispose();
            entitiesInMainSection.Dispose();

            return sceneSectionsArray;
        }

        public static void Write(EntityManager scene, string binaryPath, string objectReferencesPath)
        {
            // Write binary entity file
            int entitySceneFileSize = WriteEntityBinary(scene, out var objRefs, default, binaryPath);
            WriteObjectReferences(objRefs, objectReferencesPath);
        }

        public static void Read(EntityManager scene, string binaryPath, string objectReferencesPath)
        {
            ReferencedUnityObjects referencedUnityObjects = null;
            if (File.Exists(objectReferencesPath))
            {
                var resourceRequests = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(objectReferencesPath);
                referencedUnityObjects = (ReferencedUnityObjects)resourceRequests[0];
            }

            using (var reader = new StreamBinaryReader(binaryPath))
                SerializeUtilityHybrid.Deserialize(scene, reader, referencedUnityObjects);

            Object.DestroyImmediate(referencedUnityObjects);
        }

        static int WriteEntitySceneSection(EntityManager scene, string subsection, AssetImportContext importContext, out int objectReferenceCount, out ReferencedUnityObjects objRefs, NativeArray<EntityRemapUtility.EntityRemapInfo> entityRemapInfos = default)
        {
            k_ProfileEntitiesSceneSave.Begin();
            var entitiesBinaryPath = GetSceneWritePath(EntityScenesPaths.PathType.EntitiesBinary, subsection, importContext);
            var objRefsPath = GetSceneWritePath(EntityScenesPaths.PathType.EntitiesUnityObjectReferences, subsection, importContext);
            objectReferenceCount = 0;

            // Write binary entity file
            int entitySceneFileSize = WriteEntityBinary(scene, out objRefs, entityRemapInfos, entitiesBinaryPath);
            objectReferenceCount = WriteObjectReferences(objRefs, objRefsPath);

            k_ProfileEntitiesSceneSave.End();
            return entitySceneFileSize;
        }

        static int WriteObjectReferences(ReferencedUnityObjects objRefs, string objRefsPath)
        {
            if (objRefs == null || objRefs.Array.Length == 0)
                return 0;

            // Write object references
            using (k_ProfileEntitiesSceneWriteObjRefs.Auto())
            {
                var serializedObjectList = new List<UnityObject>();
                serializedObjectList.Add(objRefs);

                for (int i = 0; i != objRefs.Array.Length; i++)
                {
                    var obj = objRefs.Array[i];
                    if (obj != null && !EditorUtility.IsPersistent(obj))
                    {
                        if (obj is GameObject gameObject)
                        {
                            // Reset hide flags, otherwise they would prevent putting hybrid components in builds.
                            gameObject.hideFlags = HideFlags.None;
                            foreach (var component in gameObject.GetComponents<UnityEngine.Component>())
                                component.hideFlags = HideFlags.None;

                            serializedObjectList.Add(gameObject);
                            serializedObjectList.AddRange(gameObject.GetComponents<UnityEngine.Component>());
                            continue;
                        }

                        if (obj is UnityEngine.Component)
                            continue;

                        if ((obj.hideFlags & HideFlags.DontSaveInBuild) == 0)
                            serializedObjectList.Add(obj);
                        else
                            objRefs.Array[i] = null;
                    }
                }

                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(serializedObjectList.ToArray(), objRefsPath, false);

                return objRefs.Array.Length;
            }
        }

        private static int WriteEntityBinary(EntityManager scene, out ReferencedUnityObjects objRefs, NativeArray<EntityRemapUtility.EntityRemapInfo> entityRemapInfos, string entitiesBinaryPath)
        {
            int entitySceneFileSize;
            using (var writer = new StreamBinaryWriter(entitiesBinaryPath))
            {
                if (entityRemapInfos.IsCreated)
                    SerializeUtilityHybrid.Serialize(scene, writer, out objRefs, entityRemapInfos);
                else
                    SerializeUtilityHybrid.Serialize(scene, writer, out objRefs);
                entitySceneFileSize = (int)writer.Length;
            }

            return entitySceneFileSize;
        }

        static void WriteSceneHeader(Entities.Hash128 sceneGUID, SceneSectionData[] sections, string sceneName, AssetImportContext ctx, EntityManager entityManager)
        {
            k_ProfileEntitiesSceneSaveHeader.Begin();

            string headerPath = GetSceneWritePath(EntityScenesPaths.PathType.EntitiesHeader, "", ctx);

            var builder = new BlobBuilder(Allocator.TempJob);

            ref var metaData = ref builder.ConstructRoot<SceneMetaData>();
            builder.Construct(ref metaData.Sections, sections);
            builder.AllocateString(ref metaData.SceneName, sceneName);

            SerializeSceneSectionCustomMetadata(sections, ref metaData, builder, sceneName, entityManager);

            BlobAssetReference<SceneMetaData>.Write(builder, headerPath, SceneMetaDataSerializeUtility.CurrentFileFormatVersion);
            builder.Dispose();

            k_ProfileEntitiesSceneSaveHeader.End();
        }

        private static void SerializeSceneSectionCustomMetadata(SceneSectionData[] sections, ref SceneMetaData metaData,
            BlobBuilder builder, string sceneName, EntityManager entityManager)
        {
            var metaDataArray = builder.Allocate(ref metaData.SceneSectionCustomMetadata, sections.Length);
            EntityQuery sectionEntityQuery = default;
            for (int i = 0; i < sections.Length; ++i)
            {
                var sectionEntity = SerializeUtility.GetSceneSectionEntity(sections[i].SubSectionIndex, entityManager, ref sectionEntityQuery, false);
                if (sectionEntity != Entity.Null)
                    SerializeSceneSectionCustomMetadata(sectionEntity, ref metaDataArray[i], builder, sections[i], sceneName, entityManager);
            }
        }

        private static unsafe void SerializeSceneSectionCustomMetadata(Entity sectionEntity, ref BlobArray<SceneSectionCustomMetadata> metaDataSectionArray,
            BlobBuilder builder, SceneSectionData sectionData, string sceneName, EntityManager entityManager)
        {
            var types = entityManager.GetComponentTypes(sectionEntity);
            int componentCount = 0;
            for (int i = 0; i < types.Length; ++i)
            {
                var type = types[i];
                if (type == ComponentType.ReadWrite<SectionMetadataSetup>())
                    continue;
                var typeInfo = TypeManager.GetTypeInfo(type.TypeIndex);
                bool simpleComponentData = !type.IsManagedComponent && !type.IsSystemStateComponent && typeInfo.Category == TypeManager.TypeCategory.ComponentData;
                if (!simpleComponentData || typeInfo.EntityOffsetCount > 0 || typeInfo.BlobAssetRefOffsetCount > 0)
                {
                    UnityEngine.Debug.LogError(
                        $"Can't serialize Custom Metadata {typeInfo.Type.Name} of SceneSection {sectionData.SubSectionIndex} of SubScene {sceneName}. The component type must contains only blittable/basic data types");
                    continue;
                }
                types[componentCount++] = type;
            }
            var metadataArray = builder.Allocate(ref metaDataSectionArray, componentCount);
            for (int i = 0; i < componentCount; ++i)
            {
                var typeIndex = types[i].TypeIndex;
                var typeInfo = TypeManager.GetTypeInfo(typeIndex);
                metadataArray[i].StableTypeHash = typeInfo.StableTypeHash;
                if (types[i].IsZeroSized)
                    continue;

                var componentData = entityManager.GetComponentDataRawRO(sectionEntity, typeIndex);
                var data = builder.Allocate(ref metadataArray[i].Data, typeInfo.TypeSize);
                UnsafeUtility.MemCpy(data.GetUnsafePtr(), componentData, typeInfo.TypeSize);
            }
            types.Dispose();
        }

        static void WriteConversionLog(Hash128 sceneGUID, List<(int objectInstanceId, LogEventData eventData)> journalData, string sceneName, AssetImportContext ctx)
        {
            if (journalData.Count == 0)
                return;

            using (k_ProfileEntitiesSceneSaveConversionLog.Auto())
            {
                var conversionLogPath = GetSceneWritePath(EntityScenesPaths.PathType.EntitiesConversionLog, "", ctx);

                using (var writer = File.CreateText(conversionLogPath))
                {
                    foreach (var(objectInstanceId, eventData) in journalData)
                    {
                        if (eventData.Type != LogType.Exception)
                            writer.Write($"{eventData.Type}: {eventData.Message}");
                        else
                            writer.Write($"{eventData.Message}");

                        if (objectInstanceId != 0)
                        {
                            var unityObject = EditorUtility.InstanceIDToObject(objectInstanceId);
                            if (unityObject != null)
                                writer.WriteLine($" from {unityObject.name}");
                            else
                                writer.WriteLine($" from unknown object with instance Id {objectInstanceId}");
                        }
                        else
                            writer.WriteLine();
                    }
                }
            }
        }
    }
}
