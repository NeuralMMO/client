using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEditor;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;
using Hash128 = UnityEngine.Hash128;

namespace Unity.Scenes.Editor
{
    class LiveLinkAssetBundleBuildSystem : ScriptableSingleton<LiveLinkAssetBundleBuildSystem>
    {
        readonly Dictionary<GUID, Hash128> m_TrackedAssets = new Dictionary<GUID, Hash128>();
        readonly Dictionary<SubSceneGUID, Hash128> m_TrackedSubScenes = new Dictionary<SubSceneGUID, Hash128>();
        IEditorConnection m_Connection;

        internal void SetConnection(IEditorConnection connection)
        {
            var newConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            TearDownConnection();
            m_Connection = newConnection;
            SetupConnection();
        }

        public void ClearTrackedAssets()
        {
            m_TrackedAssets.Clear();
            m_TrackedSubScenes.Clear();
        }

        void RequestAssetByGUID(MessageEventArgs args)
        {
            var guid = args.Receive<GUID>();
            LiveLinkMsg.LogReceived($"AssetBundleBuild request: '{guid}' -> '{AssetDatabase.GUIDToAssetPath(guid.ToString())}'");

            SendAsset(guid, args.playerId);
        }

        public void RequestSubSceneByGUID(MessageEventArgs args)
        {
            var subSceneId = args.Receive<ResolvedSubSceneID>();
            LiveLinkMsg.LogInfo($"RequestSubSceneForGUID => {subSceneId.SubSceneGUID}");

            SendSubScene(subSceneId, args.playerId);
        }

        void RequestSubSceneTargetHash(MessageEventArgs args)
        {
            using (var subScenes = args.ReceiveArray<SubSceneGUID>())
            {
                var resolvedScenes = new HashSet<ResolvedSubSceneID>();
                foreach (var subScene in subScenes)
                {
                    LiveLinkMsg.LogInfo($"RequestSubSceneTargetHash => {subScene.Guid}, {subScene.BuildConfigurationGuid}");

                    var targetHash = EntityScenesPaths.GetSubSceneArtifactHash(subScene.Guid, subScene.BuildConfigurationGuid, ImportMode.Asynchronous);
                    m_TrackedSubScenes[subScene] = targetHash;
                    if (targetHash.IsValid)
                        resolvedScenes.Add(new ResolvedSubSceneID {SubSceneGUID = subScene, TargetHash = targetHash});
                }

                TimeBasedCallbackInvoker.SetCallback(DetectChangedAssets);

                if (resolvedScenes.Count == 0)
                    return;

                var resolved = new NativeArray<ResolvedSubSceneID>(resolvedScenes.Count, Allocator.Temp);
                int i = 0;
                foreach (var id in resolvedScenes)
                    resolved[i++] = id;

                SendSubSceneTargetHash(resolved, args.playerId);
            }
        }

        void RequestAssetTargetHash(MessageEventArgs args)
        {
            //@TODO: should be based on connection / BuildSetting
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;

            // Array of Asset GUIDs the player is requesting the asset hash of
            using (var assets = args.ReceiveArray<GUID>())
            {
                // Set of ready to send (all valid target hashes) assets
                var resolvedAssets = new HashSet<ResolvedAssetID>();
                foreach (var asset in assets)
                {
                    LiveLinkMsg.LogReceived($"AssetBundleTargetHash request => {asset}");

                    // For each Asset- queue calculating target hash and add to tracked assets
                    Unity.Entities.Hash128 targetHash = LiveLinkBuildPipeline.CalculateTargetHash(asset, buildTarget, ImportMode.Asynchronous);
                    m_TrackedAssets[asset] = targetHash;

                    resolvedAssets.Add(new ResolvedAssetID { GUID = asset, TargetHash = targetHash });

                    // If asset hash is valid (meaning import is ready) then also do the same for dependencies
                    if (targetHash.IsValid)
                    {
                        LiveLinkBuildPipeline.CalculateTargetDependencies(targetHash, buildTarget, out ResolvedAssetID[] dependencies, ImportMode.Asynchronous);
                        foreach (var dependency in dependencies)
                        {
                            m_TrackedAssets[dependency.GUID] = dependency.TargetHash;
                            resolvedAssets.Add(new ResolvedAssetID {GUID = dependency.GUID, TargetHash = dependency.TargetHash});
                        }
                    }
                }

                // Callback to re-send tracked assets when their targethash changes
                if (m_TrackedAssets.Count > 0)
                    TimeBasedCallbackInvoker.SetCallback(DetectChangedAssets);

                // No assets? Send nothing and set no callback
                if (resolvedAssets.Count == 0)
                    return;

                var resolved = new NativeArray<ResolvedAssetID>(resolvedAssets.Count, Allocator.Temp);
                int j = 0;
                foreach (var id in resolvedAssets)
                    resolved[j++] = id;

                SendAssetTargetHash(resolved, args.playerId);
            }
        }

        void SetupConnection()
        {
            if (m_Connection == null)
                return;
            m_Connection.Register(LiveLinkMsg.PlayerRequestAssetForGUID, RequestAssetByGUID);
            m_Connection.Register(LiveLinkMsg.PlayerRequestAssetTargetHash, RequestAssetTargetHash);
            m_Connection.Register(LiveLinkMsg.PlayerRequestSubSceneTargetHash, RequestSubSceneTargetHash);
            m_Connection.Register(LiveLinkMsg.PlayerRequestSubSceneForGUID, RequestSubSceneByGUID);
        }

        void OnEnable()
        {
            var conn = new EditorPlayerConnection(EditorConnection.instance);
            SetConnection(conn);
        }

        void TearDownConnection()
        {
            if (m_Connection == null)
                return;
            m_Connection.Unregister(LiveLinkMsg.PlayerRequestAssetForGUID, RequestAssetByGUID);
            m_Connection.Unregister(LiveLinkMsg.PlayerRequestAssetTargetHash, RequestAssetTargetHash);
            m_Connection.Unregister(LiveLinkMsg.PlayerRequestSubSceneTargetHash, RequestSubSceneTargetHash);
            m_Connection.Unregister(LiveLinkMsg.PlayerRequestSubSceneForGUID, RequestSubSceneByGUID);
        }

        void OnDisable()
        {
            TearDownConnection();
        }

        static string ResolveCachePath(Unity.Entities.Hash128 targethash)
        {
            var path = "Library/LiveLinkAssetBundleCache/" + targethash;
            return path;
        }

        void SendSubSceneTargetHash(NativeArray<ResolvedSubSceneID> resolvedSubScenes, int playerId)
        {
            foreach (var asset in resolvedSubScenes)
            {
                m_TrackedSubScenes[asset.SubSceneGUID] = asset.TargetHash;
                LiveLinkMsg.LogInfo($"SendSubSceneTargetHash => {asset.SubSceneGUID} to playerId: {playerId}");
            }

            m_Connection.SendArray(LiveLinkMsg.EditorResponseSubSceneTargetHash, resolvedSubScenes, playerId);
        }

        void SendAssetTargetHash(NativeArray<ResolvedAssetID> resolvedAssets, int playerId)
        {
            foreach (var asset in resolvedAssets)
                LiveLinkMsg.LogSend($"AssetBundleTargetHash response {asset.GUID} | {asset.TargetHash} to playerId: {playerId}");

            m_Connection.SendArray(LiveLinkMsg.EditorResponseAssetTargetHash, resolvedAssets, playerId);
        }

        void SendBuildArtifact(string artifactPath, int playerId)
        {
            string artifactFileName = Path.GetFileName(artifactPath);
            SendBuildArtifact(artifactPath, artifactFileName, playerId);
        }

        unsafe void SendBuildArtifact(string artifactPath, string artifactFileName, int playerId)
        {
            LiveLinkMsg.LogInfo($"SendBuildArtifact => artifactPath={artifactPath}, playerId={playerId}");

            if (!File.Exists(artifactPath))
            {
                Debug.LogError($"Attempting to send file that doesn't exist on editor. {artifactPath}");
                return;
            }

            using (FileStream fs = new FileStream(artifactPath, FileMode.Open, FileAccess.Read))
            {
                // TODO: Any OS/language supports wide chars here? Should be tested
                var bufferSize = fs.Length + artifactFileName.Length * sizeof(char) + sizeof(int);
                if (fs.Length > int.MaxValue)
                {
                    Debug.LogError($"File cannot be sent to the player because it exceeds the 2GB size limit. {artifactPath}");
                    return;
                }

                var buffer = new byte[bufferSize];
                fixed(byte* data = buffer)
                {
                    var writer = new UnsafeAppendBuffer(data, (int)bufferSize);
                    writer.Add(artifactFileName);

                    int numBytesToRead = (int)fs.Length;
                    int numBytesRead = writer.Length;
                    while (numBytesToRead > 0)
                    {
                        int n = fs.Read(buffer, numBytesRead, numBytesToRead);

                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                }

                m_Connection.Send(LiveLinkMsg.EditorSendBuildArtifact, buffer, playerId);
            }
        }

        unsafe void SendResponseSubSceneForGuid(in ResolvedSubSceneID subSceneId, HashSet<Unity.Entities.Hash128> assetDependencies, int playerId)
        {
            var runtimeGlobalObjectIds = new NativeArray<RuntimeGlobalObjectId>(assetDependencies.Count, Allocator.Temp);
            int j = 0;
            foreach (var asset in assetDependencies)
                runtimeGlobalObjectIds[j++] = new RuntimeGlobalObjectId {AssetGUID = asset};

            long bufferSize = sizeof(ResolvedSubSceneID) + sizeof(int) + runtimeGlobalObjectIds.Length * sizeof(RuntimeGlobalObjectId);
            if (bufferSize > int.MaxValue)
            {
                Debug.LogError($"Buffer cannot be sent to the player because it exceeds the 2GB size limit.");
                return;
            }

            var buffer = new byte[bufferSize];
            fixed(byte* data = buffer)
            {
                var writer = new UnsafeAppendBuffer(data, (int)bufferSize);
                writer.Add(subSceneId);
                writer.Add(runtimeGlobalObjectIds);

                m_Connection.Send(LiveLinkMsg.EditorResponseSubSceneForGUID, buffer, playerId);
            }
        }

        void SendSubScene(ResolvedSubSceneID subSceneId, int playerId)
        {
            LiveLinkMsg.LogInfo($"Sending SubScene: 'GUID: {subSceneId.SubSceneGUID.Guid}' Hash: '{subSceneId.TargetHash}' with 'BuildConfiguration: {subSceneId.SubSceneGUID.BuildConfigurationGuid}' to playerId: {playerId}");
            AssetDatabaseCompatibility.GetArtifactPaths(subSceneId.TargetHash, out var paths);
            var sceneHeaderPath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesHeader);

            // Send Header build artifact
            SendBuildArtifact(sceneHeaderPath, playerId);

            // Process each scene section, gathering runtime global obj IDs and sending EBFs
            if (!BlobAssetReference<SceneMetaData>.TryRead(sceneHeaderPath, SceneMetaDataSerializeUtility.CurrentFileFormatVersion, out var sceneMetaDataRef))
            {
                Debug.LogError("Send Entity Scene failed because the entity header file was an old version: " + sceneHeaderPath);
                return;
            }

            var assetDependencies = new HashSet<Unity.Entities.Hash128>();
            ref var sceneMetaData = ref sceneMetaDataRef.Value;
            for (int i = 0; i != sceneMetaData.Sections.Length; i++)
            {
                var sectionIndex = sceneMetaData.Sections[i].SubSectionIndex;

                var binaryPath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesBinary, sectionIndex);
                SendBuildArtifact(binaryPath, playerId);

                if (sceneMetaData.Sections[i].ObjectReferenceCount > 0)
                {
                    var refGuidsPath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesUnityObjectRefGuids, sectionIndex);
                    SendBuildArtifact(refGuidsPath, playerId);

                    var scriptedObjPath = EntityScenesPaths.GetLoadPathFromArtifactPaths(paths, EntityScenesPaths.PathType.EntitiesUnityObjectReferences, sectionIndex);
                    var bundleName = $"{(UnityEngine.Hash128)subSceneId.TargetHash}.{sectionIndex}.{EntityScenesPaths.GetExtension(EntityScenesPaths.PathType.EntitiesUnitObjectReferencesBundle)}";
                    var tempPath = Path.GetTempFileName();

                    AssetBundleTypeCache.RegisterMonoScripts();
                    LiveLinkBuildPipeline.BuildSubSceneBundle(scriptedObjPath, bundleName, tempPath,
                        EditorUserBuildSettings.activeBuildTarget, assetDependencies);
                    SendBuildArtifact(tempPath, bundleName, playerId);
                    File.Delete(tempPath);
                }
            }

            SendResponseSubSceneForGuid(subSceneId, assetDependencies, playerId);
        }

        unsafe void SendAsset(GUID guid, int playerId)
        {
            Hash128 targetHash;
            string path = BuildAssetBundleIfNotCached(guid, out targetHash);
            if (path == null)
            {
                // Hash is out of date, so send to player to keep waiting
                if (!targetHash.isValid)
                {
                    var resolvedAssetIds = new NativeArray<ResolvedAssetID>(1, Allocator.Temp);
                    resolvedAssetIds[0] = new ResolvedAssetID { GUID = guid, TargetHash = targetHash };
                    bool wasEmpty = m_TrackedAssets.Count == 0;
                    m_TrackedAssets[guid] = targetHash;
                    if (wasEmpty && m_TrackedAssets.Count > 0)
                    {
                        // if there were no tracked assets previously, we have to manually set up the callback that
                        // checks for changes
                        TimeBasedCallbackInvoker.SetCallback(DetectChangedAssets);
                    }

                    SendAssetTargetHash(resolvedAssetIds, 0);
                    resolvedAssetIds.Dispose();
                }

                return;
            }

            var stream = File.OpenRead(path);
            var assetBundleFileLength = stream.Length;
            var bufferSize = stream.Length + sizeof(Hash128) + sizeof(Hash128);

            if (bufferSize > int.MaxValue)
            {
                Debug.LogError($"AssetBundle {guid} can't be sent to the player because it exceeds the 2GB size limit");
                return;
            }

            var bundleAndHeader = new byte[bufferSize];
            fixed(byte* data = bundleAndHeader)
            {
                var writer = new UnsafeAppendBuffer(data, bundleAndHeader.Length);
                writer.Add(guid);
                writer.Add(targetHash);
                stream.Read(bundleAndHeader, writer.Length, (int)assetBundleFileLength);
            }

            stream.Close();
            stream.Dispose();

            LiveLinkMsg.LogSend($"AssetBundle: '{AssetDatabase.GUIDToAssetPath(guid.ToString())}' ({guid}), size: {assetBundleFileLength}, hash: {targetHash} to playerId: {playerId}");
            m_Connection.Send(LiveLinkMsg.EditorResponseAssetForGUID, bundleAndHeader, playerId);
        }

        public string BuildAssetBundleIfNotCached(GUID guid, out Hash128 targetHash)
        {
            //@TODO Get build target from player requesting it...
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            targetHash = LiveLinkBuildPipeline.CalculateTargetHash(guid, buildTarget, ImportMode.NoImport);

            // New build kicked off since player last got a valid hash, this is fine, we just resent GUID with invalid hash
            if (!targetHash.isValid)
                return null;

            var bundlePath = LiveLinkBuildImporter.GetBundlePathInternal(targetHash, guid);
            if (string.IsNullOrEmpty(bundlePath) || !File.Exists(bundlePath))
            {
                Debug.LogError($"Failed to build asset bundle: '{guid}'");
                return null;
            }
            return bundlePath;
        }

        // TODO: Add support for multiple players here
        void DetectChangedAssets()
        {
            if (m_TrackedAssets.Count == 0 && m_TrackedSubScenes.Count == 0)
            {
                TimeBasedCallbackInvoker.ClearCallback(DetectChangedAssets);
                return;
            }

            using (var changedAssets = new NativeList<ResolvedAssetID>(Allocator.Temp))
            {
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                foreach (var asset in m_TrackedAssets.ToArray())
                {
                    //@TODO: Artifact hash API should give error message when used on V1 pipeline (currently does not).

                    var targetHash = LiveLinkBuildPipeline.CalculateTargetHash(asset.Key, buildTarget, ImportMode.Asynchronous);

                    if (targetHash.isValid && asset.Value != targetHash)
                    {
                        LiveLinkMsg.LogInfo($"Detected asset change: {AssetDatabase.GUIDToAssetPath(asset.Key.ToString())}");
                        changedAssets.Add(new ResolvedAssetID { GUID = asset.Key, TargetHash = targetHash });
                        m_TrackedAssets[asset.Key] = targetHash;

                        LiveLinkBuildPipeline.CalculateTargetDependencies(targetHash, buildTarget, out ResolvedAssetID[] dependencies, ImportMode.Asynchronous);
                        foreach (var dependency in dependencies)
                        {
                            // We are already tracking this asset
                            if (m_TrackedAssets.ContainsKey(dependency.GUID))
                                continue;

                            m_TrackedAssets[dependency.GUID] = dependency.TargetHash;

                            // Invalid hash and not tracking yet, so queue an import
                            if (!dependency.TargetHash.IsValid)
                            {
                                LiveLinkBuildPipeline.CalculateTargetHash(dependency.GUID, buildTarget, ImportMode.Asynchronous);
                            }

                            // Send asset so the player is tracking it as a dependency before loading all ABs
                            changedAssets.Add(new ResolvedAssetID { GUID = dependency.GUID, TargetHash = dependency.TargetHash });
                        }
                    }
                }

                // Send changed asset hashes to player
                if (changedAssets.Length != 0)
                    SendAssetTargetHash(changedAssets, 0);
            }

            using (var changedSubScenes = new NativeList<ResolvedSubSceneID>(Allocator.Temp))
            {
                foreach (var subScene in m_TrackedSubScenes)
                {
                    var targetHash = EntityScenesPaths.GetSubSceneArtifactHash(subScene.Key.Guid,
                        subScene.Key.BuildConfigurationGuid,
                        ImportMode.NoImport);
                    if (targetHash.IsValid && (subScene.Value != (Hash128)targetHash))
                    {
                        LiveLinkMsg.LogInfo("Detected subscene change: " + subScene.Key);
                        changedSubScenes.Add(new ResolvedSubSceneID
                            {SubSceneGUID = subScene.Key, TargetHash = targetHash});
                    }
                }

                if (changedSubScenes.Length > 0)
                    SendSubSceneTargetHash(changedSubScenes, 0);
            }
        }
    }
}
