using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Build;
using Unity.Build.Common;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Tests;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Hash128 = Unity.Entities.Hash128;

namespace Unity.Scenes.Editor.Tests
{
    class LiveLinkEditorConnectionTests
    {
        static LiveLinkTestConnection s_Connection;
        static GUID[] s_TempAssetGuids;
        static Material s_TempMaterial;
        static GUID s_TempMaterialGuid;
        static Texture s_TempTexture;
        static GUID s_TempTextureGuid;

        static Scene s_TempScene;
        static SubScene s_SubSceneWithSections;

        static GUID s_LiveLinkBuildConfigGuid;

        static TestWithTempAssets s_Assets = default;

        [OneTimeSetUp]
        public static void SetUpOnce()
        {
            s_Assets.SetUp();

            var assetGuids = new List<GUID>();
            try
            {
                {
                    string path = s_Assets.GetNextPath(".asset");
                    AssetDatabase.CreateAsset(s_TempTexture = new Texture2D(64, 64), path);
                    s_TempTextureGuid = new GUID(AssetDatabase.AssetPathToGUID(path));
                    assetGuids.Add(s_TempTextureGuid);
                }
                {
                    string path = s_Assets.GetNextPath(".mat");
                    AssetDatabase.CreateAsset(s_TempMaterial = new Material(Shader.Find("Standard")), path);
                    s_TempMaterialGuid = new GUID(AssetDatabase.AssetPathToGUID(path));
                    assetGuids.Add(s_TempMaterialGuid);
                    s_TempMaterial.mainTexture = s_TempTexture;
                }

                var tempScenePath = s_Assets.GetNextPath(".unity");
                s_TempScene = SubSceneTestsHelper.CreateScene(tempScenePath);
                s_SubSceneWithSections = SubSceneTestsHelper.CreateSubSceneInSceneFromObjects("SubScene", false, s_TempScene, () =>
                {
                    var go1 = new GameObject();
                    go1.AddComponent<SceneSectionComponent>().SectionIndex = 0;
                    var go2 = new GameObject();
                    go2.AddComponent<SceneSectionComponent>().SectionIndex = 2;
                    go2.AddComponent<TestPrefabComponentAuthoring>().Material = s_TempMaterial;
                    return new List<GameObject> { go1, go2 };
                });

                {
                    var path = s_Assets.GetNextPath("LiveLinkBuildConfig.buildconfiguration");
                    BuildConfiguration.CreateAsset(path, config =>
                    {
                        config.SetComponent(new SceneList
                        {
                            SceneInfos = new List<SceneList.SceneInfo>
                            {
                                new SceneList.SceneInfo
                                {
                                    Scene = GlobalObjectId.GetGlobalObjectIdSlow(AssetDatabase.LoadAssetAtPath<SceneAsset>(tempScenePath))
                                }
                            }
                        });
                    });
                    s_LiveLinkBuildConfigGuid = new GUID(AssetDatabase.AssetPathToGUID(path));
                }
            }
            catch
            {
                s_Assets.TearDown();
                throw;
            }

            // This call ensures that the asset worker is already running and no test times out because we're still
            // waiting for the asset worker. Effectively this doesn't change the runtime that much since we will have
            // to wait for the import to finish in most of the tests anyway.
            GetLiveLinkArtifactHash(s_TempMaterialGuid, ImportMode.Synchronous);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            s_TempAssetGuids = assetGuids.ToArray();
        }

        [SetUp]
        public static void SetUp()
        {
            DefaultWorldInitialization.DefaultLazyEditModeInitialize();
            s_Connection = new LiveLinkTestConnection();
            EditorSceneLiveLinkToPlayerSendSystem.instance.SetConnection(s_Connection);
            LiveLinkAssetBundleBuildSystem.instance.SetConnection(s_Connection);
        }

        [TearDown]
        public static void TearDown()
        {
            var connections = s_Connection.OpenConnections.ToArray();
            foreach (var connectedPlayer in connections)
                s_Connection.PostDisconnect(connectedPlayer);
        }

        [OneTimeTearDown]
        public static void TearDownOnce()
        {
            s_Assets.TearDown();
            LiveLinkEditorEventSystem.InitializeLiveLinkSystems();
            SceneWithBuildConfigurationGUIDs.ClearBuildSettingsCache();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        static LiveLinkTestConnection.Message AssertNextMessage(Guid msgType, int forPlayer)
        {
            Assert.Greater(s_Connection.IncomingMessages.Count, 0, $"Expected another message of type {LiveLinkDebugHelper.GetMessageName(msgType) ?? msgType.ToString()} but there are no more messages.");
            var msg = s_Connection.IncomingMessages.Peek();
            Assert.IsTrue(forPlayer == msg.Player || msg.Player == 0, $"Expected a message for player {forPlayer} but received a message for player {msg.Player}, message type: {LiveLinkDebugHelper.GetMessageName(msg.Id)}");
            Assert.AreEqual(msgType, msg.Id, $"Expected a message of type {LiveLinkDebugHelper.GetMessageName(msgType)}, but found {LiveLinkDebugHelper.GetMessageName(msg.Id)}. Messages:\n{s_Connection.GetMessageString()}");
            s_Connection.IncomingMessages.Dequeue();
            return msg;
        }

        static void AssertNumMessages(int n)
        {
            int c = s_Connection.IncomingMessages.Count;
            Assert.AreEqual(n, c, $"Expected {n} more messages but found {c} messages:\n{s_Connection.GetMessageString()}");
        }

        static IEnumerable WaitForMessage(int numMessages = 1, int maxWaitFrames = 10000)
        {
            for (int frame = 0; frame < maxWaitFrames; frame++)
            {
                if (s_Connection.IncomingMessages.Count >= numMessages)
                    yield break;
                yield return null;
            }
            Assert.Fail("Waiting for messages timed out!");
        }

        NativeArray<T> ReceiveArray<T>(LiveLinkTestConnection.Message msg, string error = null) where T : unmanaged
        {
            NativeArray<T> dat = default;
            if (error != null)
                Assert.DoesNotThrow(() => dat = msg.EventArgs.ReceiveArray<T>(), error);
            else
                Assert.DoesNotThrow(() => dat = msg.EventArgs.ReceiveArray<T>(), $"Failed to deserialize NativeArray<{typeof(T).Name}> from {msg.Data.Length} byte long message {msg.FormatBytes()}");
            var read = MessageEventArgsExtensions.SerializeUnmanagedArray(dat).Length;
            Assert.AreEqual(msg.Data.Length, read, $"Failed to read all bytes when receiving array of type {typeof(T).Name}. Read {read} out of {msg.Data.Length} bytes. Is that really the right type for this message?");
            return dat;
        }

        [Test]
        public void NoDuplicateGUIDsExist()
        {
            var hashSet = new HashSet<Guid>();
            AddMessages(hashSet, LiveLinkDebugHelper.EditorMessages);
            AddMessages(hashSet, LiveLinkDebugHelper.PlayerMessages);
            AddMessages(hashSet, LiveLinkDebugHelper.UnknownMessages);

            void AddMessages(HashSet<Guid> set, IEnumerable<Guid> guids)
            {
                foreach (var msg in guids)
                    Assert.IsTrue(set.Add(msg), $"Duplicate LiveLink message GUID {msg}. Please check {nameof(LiveLinkMsg)} and ensure each message has a unique GUID.");
            }
        }

        [Test]
        public void NoUnknownMessagesExist()
        {
            var messages = LiveLinkDebugHelper.UnknownMessages;
            Assert.IsEmpty(messages, $"There are LiveLink messages that have neither the player prefix {LiveLinkMsg.PlayerPrefix} nor the editor prefix {LiveLinkMsg.EditorPrefix}: {string.Join(", ", messages.Select(LiveLinkDebugHelper.GetMessageName))}");
        }

        static bool IsSpecialGUID(Hash128 h)
        {
            // for these kinds of guids, the last component encodes the file identifier
            h.Value.w = 0;
            GUID guid = h;
            return guid == LiveLinkBuildPipeline.k_UnityBuiltinResources ||
                guid == LiveLinkBuildPipeline.k_UnityEditorResources ||
                guid == LiveLinkBuildPipeline.k_UnityBuiltinExtraResources;
        }

        [Test]
        public void EditorListensForMessages()
        {
            Assert.AreEqual(1, s_Connection.ConnectHandler.Count);
            Assert.AreEqual(1, s_Connection.DisconnectHandler.Count);

            foreach (var msg in LiveLinkDebugHelper.PlayerMessages)
                Assert.Greater(s_Connection.GetNumMessageHandlers(msg), 0, $"Editor does not listen for player message {LiveLinkDebugHelper.GetMessageName(msg)}.");
        }

        [Test]
        public void EditorResetsOnePlayer()
        {
            var sendSystem = EditorSceneLiveLinkToPlayerSendSystem.instance;
            s_Connection.PostConnect(1);
            s_Connection.PostConnect(2);

            s_Connection.IncomingMessages.Clear();

            sendSystem.ResetPlayer(1);

            AssertNextMessage(LiveLinkMsg.EditorResetGame, 1);
            AssertNumMessages(0);

            s_Connection.PostDisconnect(1);
            s_Connection.PostDisconnect(2);
        }

        [Test]
        public void EditorResetsAllPlayers()
        {
            var sendSystem = EditorSceneLiveLinkToPlayerSendSystem.instance;

            s_Connection.PostConnect(1);
            s_Connection.PostConnect(2);

            s_Connection.IncomingMessages.Clear();

            sendSystem.ResetAllPlayers();

            AssertNextMessage(LiveLinkMsg.EditorResetGame, 0);
            AssertNumMessages(0);

            s_Connection.PostDisconnect(1);
            s_Connection.PostDisconnect(2);
        }

        static void DoConnect()
        {
            s_Connection.PostConnect(1);
            s_Connection.IncomingMessages.Clear();

            s_Connection.PostMessage(1, LiveLinkMsg.PlayerRequestConnectLiveLink, s_LiveLinkBuildConfigGuid);
            AssertNextMessage(LiveLinkMsg.EditorResponseConnectLiveLink, 1);
        }

        [UnityTest]
        [Ignore("Doesn't currently work because LiveLink cannot distinguish invalid assets from assets that are still importing.")]
        public IEnumerator EditorComputesAssetHashForInvalidAssets()
        {
            DoConnect();
            s_Connection.PostMessageArray(1, LiveLinkMsg.PlayerRequestAssetTargetHash, new GUID[] { default });
            foreach (var v in WaitForMessage())
                yield return v;
            var msg = AssertNextMessage(LiveLinkMsg.EditorResponseAssetTargetHash, 1);

            using (var assets = ReceiveArray<ResolvedAssetID>(msg))
            {
                Assert.AreEqual(1, assets.Length);
                Assert.AreEqual(default(Hash128), assets[0].GUID);
                Assert.IsFalse(assets[0].TargetHash.IsValid);
            }
        }

        static Hash128 GetLiveLinkArtifactHash(string guid, ImportMode syncMode = ImportMode.NoImport) => AssetDatabaseCompatibility.GetArtifactHash(guid, typeof(LiveLinkBuildImporter), syncMode);
        static Hash128 GetLiveLinkArtifactHash(GUID guid, ImportMode syncMode = ImportMode.NoImport) => GetLiveLinkArtifactHash(guid.ToString(), syncMode);

        IEnumerable WaitForAssets(Dictionary<Hash128, Hash128> outHashesByGUID, int player = 1)
        {
            outHashesByGUID.Clear();
            // Receive hashes for assets until all of them are valid
            do
            {
                foreach (var v in WaitForMessage())
                    yield return v;
                if (s_Connection.IncomingMessages.Peek().Id != LiveLinkMsg.EditorResponseAssetTargetHash)
                {
                    Assert.AreEqual(0, outHashesByGUID.Count, "Received another message, but we're still waiting for more asset hashes.");
                    yield break;
                }

                var msg = AssertNextMessage(LiveLinkMsg.EditorResponseAssetTargetHash, player);
                using (var assets = ReceiveArray<ResolvedAssetID>(msg))
                {
                    for (int i = 0; i < assets.Length; i++)
                    {
                        Assert.IsTrue(assets[i].GUID.IsValid, "Received an invalid GUID!");
                        var path = AssetDatabase.GUIDToAssetPath(assets[i].GUID.ToString());
                        if (!assets[i].TargetHash.IsValid)
                        {
                            Assert.IsFalse(outHashesByGUID.TryGetValue(assets[i].GUID, out var hash), $"Received an invalid hash for GUID {assets[i].GUID} ({path}) but we have already received the hash {hash} for it before!");
                        }
                        else
                        {
                            Assert.IsTrue(!outHashesByGUID.TryGetValue(assets[i].GUID, out var hash) || !hash.IsValid, $"Received a valid hash for GUID {assets[i].GUID} ({path}), but we previously already received the hash {hash}.");
                            if (!IsSpecialGUID(assets[i].GUID))
                                Assert.AreEqual(GetLiveLinkArtifactHash(assets[i].GUID), assets[i].TargetHash, $"Hash mismatch for GUID {assets[i].GUID} ({path})");
                        }
                        outHashesByGUID[assets[i].GUID] = assets[i].TargetHash;
                    }
                }
            }
            while (outHashesByGUID.Any(kvp => !kvp.Value.IsValid));
        }

        [UnityTest]
        public IEnumerator EditorComputesAssetHashForValidAssets()
        {
            DoConnect();
            Assert.Greater(s_TempAssetGuids.Length, 0);
            s_Connection.PostMessageArray(1, LiveLinkMsg.PlayerRequestAssetTargetHash, s_TempAssetGuids);

            // in the worst case, every asset comes in its own message plus some overhead messages
            var hashesByGUID = new Dictionary<Hash128, Hash128>();
            foreach (var v in WaitForAssets(hashesByGUID))
                yield return v;
            foreach (var guid in s_TempAssetGuids)
                Assert.IsTrue(hashesByGUID.ContainsKey(guid), $"Failed to find asset for GUID {guid} ({AssetDatabase.GUIDToAssetPath(guid.ToString())}).");
            AssertNumMessages(0);
        }

        /// <summary>
        /// Checks that the editor sends an updated version of an asset hash when an asset that was previously requested
        /// is changed on disk.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator EditorTracksAssetChanges_ModifyAsset()
        {
            DoConnect();

            var materialGuid = (Hash128)s_TempMaterialGuid;
            s_Connection.PostMessageArray(1, LiveLinkMsg.PlayerRequestAssetTargetHash, new[] { materialGuid });

            var hashByGUID = new Dictionary<Hash128, Hash128>();
            foreach (var v in WaitForAssets(hashByGUID))
                yield return v;
            Assert.IsTrue(hashByGUID.TryGetValue(materialGuid, out var previousHash));

            yield return null;

            // now modify the material and commit the changes
            s_TempMaterial.color = Color.cyan;
            AssetDatabase.SaveAssets();


            foreach (var v in WaitForAssets(hashByGUID))
                yield return v;
            Assert.IsTrue(hashByGUID.TryGetValue(materialGuid, out var newHash));
            Assert.AreNotEqual(previousHash, newHash);

            AssertNumMessages(0);
        }

        /// <summary>
        /// Checks that if we request on asset with a dependency and we change the asset that there is a dependency on,
        /// we get an update for the dependent asset.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator EditorTracksAssetChanges_ChangeDependency()
        {
            DoConnect();

            s_Connection.PostMessageArray(1, LiveLinkMsg.PlayerRequestAssetTargetHash, new[] { (Hash128)s_TempMaterialGuid });

            var hashesByGUID = new Dictionary<Hash128, Hash128>();
            foreach (var v in WaitForAssets(hashesByGUID))
                yield return v;
            Assert.IsTrue(hashesByGUID.ContainsKey(s_TempMaterialGuid), "The editor did not send the hash of the material");
            Assert.IsTrue(hashesByGUID.ContainsKey(s_TempTextureGuid), "The editor did not send the hash of the dependency");

            yield return null;

            var wrapMode = s_TempTexture.wrapMode;
            s_TempTexture.wrapMode = wrapMode == TextureWrapMode.Clamp ? TextureWrapMode.Mirror : TextureWrapMode.Clamp;
            AssetDatabase.SaveAssets();

            foreach (var v in WaitForAssets(hashesByGUID))
                yield return v;
            // sometimes we might only get the updated material and then later the updated texture
            if (!hashesByGUID.ContainsKey(s_TempTextureGuid))
            {
                foreach (var v in WaitForAssets(hashesByGUID))
                    yield return v;
            }

            // check that the updated assets is the texture and that the hash changed
            Assert.IsTrue(hashesByGUID.ContainsKey(s_TempTextureGuid), "The editor did not send the modified texture.");

            AssertNumMessages(0);
        }

        /// <summary>
        /// Checks that the editor correctly sends back an invalid hash when a previously tracked asset is deleted.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        [Ignore("Doesn't currently work because LiveLink cannot distinguish invalid assets from assets that are still importing.")]
        public IEnumerator EditorTracksAssetChanges_DeleteAsset()
        {
            DoConnect();

            string path = s_Assets.GetNextPath(".mat");
            AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")), path);
            var materialGuid = (Hash128) new GUID(AssetDatabase.AssetPathToGUID(path));

            s_Connection.PostMessageArray(1, LiveLinkMsg.PlayerRequestAssetTargetHash, new[] { materialGuid });
            var hashByGUID = new Dictionary<Hash128, Hash128>();
            foreach (var v in WaitForAssets(hashByGUID))
                yield return v;
            Assert.IsTrue(hashByGUID.TryGetValue(materialGuid, out var previousHash), "The editor did not send the material back");

            yield return null;

            AssetDatabase.DeleteAsset(path);

            foreach (var v in WaitForAssets(hashByGUID))
                yield return v;

            Assert.IsTrue(hashByGUID.TryGetValue(materialGuid, out var newHash), "The editor did not update the material.");
            Assert.IsFalse(newHash.IsValid);

            AssertNumMessages(0);
        }

        static unsafe void Read<T>(byte[] data, out T result) where T : struct
        {
            fixed(byte* buffer = data)
            {
                var reader = new UnsafeAppendBuffer.Reader(buffer, data.Length);
                reader.ReadNext(out result);
            }
        }

        static unsafe int SizeOf<T>() where T : unmanaged => sizeof(T);

        [UnityTest]
        public IEnumerator EditorSendsAssetBundle()
        {
            DoConnect();

            s_Connection.PostMessage(1, LiveLinkMsg.PlayerRequestAssetForGUID, (Hash128)s_TempTextureGuid);

            var hashByGUID = new Dictionary<Hash128, Hash128>();
            foreach (var v in WaitForAssets(hashByGUID))
                yield return v;
            if (hashByGUID.Count > 0)
            {
                // if we received any hashes, that means the texture had to be imported and we now need to re-request it
                Assert.IsTrue(hashByGUID.ContainsKey(s_TempTextureGuid));
                s_Connection.PostMessage(1, LiveLinkMsg.PlayerRequestAssetForGUID, (Hash128)s_TempTextureGuid);
            }
            var msg = AssertNextMessage(LiveLinkMsg.EditorResponseAssetForGUID, 1);
            Assert.GreaterOrEqual(msg.Data.Length, SizeOf<ResolvedAssetID>(), $"Received an asset bundle message, but it does not start with a {nameof(ResolvedAssetID)}.");
            Read(msg.Data, out ResolvedAssetID assetId);
            Assert.AreEqual((Hash128)s_TempTextureGuid, assetId.GUID);
            Assert.IsTrue(assetId.TargetHash.IsValid, "The hash sent by the editor is not valid.");
            Assert.AreEqual(GetLiveLinkArtifactHash(s_TempTextureGuid), assetId.TargetHash, $"Hash mismatch for asset");

            Assert.Greater(msg.Data.Length - SizeOf<ResolvedAssetID>(), 0, $"Received an asset bundle message, but it only contains an {nameof(ResolvedAssetID)} header without contents.");
            AssertNumMessages(0);
        }

        [UnityTest]
        public IEnumerator EditorComputesSubSceneHash()
        {
            DoConnect();

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s_SubSceneWithSections.SceneAsset, out var subSceneGuidString, out long _);
            var subSceneGuid = new SubSceneGUID
            {
                Guid = new GUID(subSceneGuidString),
                BuildConfigurationGuid = s_LiveLinkBuildConfigGuid,
            };
            Assert.IsTrue(subSceneGuid.Guid.IsValid);

            s_Connection.PostMessageArray(1, LiveLinkMsg.PlayerRequestSubSceneTargetHash, new[] { subSceneGuid });

            foreach (var v in WaitForMessage())
                yield return v;

            var msg = AssertNextMessage(LiveLinkMsg.EditorResponseSubSceneTargetHash, 1);
            using (var assets = ReceiveArray<ResolvedSubSceneID>(msg))
            {
                Assert.AreEqual(1, assets.Length);
                Assert.AreEqual(subSceneGuid, assets[0].SubSceneGUID);
                var hash = EntityScenesPaths.GetSubSceneArtifactHash(subSceneGuid.Guid, subSceneGuid.BuildConfigurationGuid, ImportMode.NoImport);
                Assert.AreEqual(hash, assets[0].TargetHash);
            }

            AssertNumMessages(0);
        }

        static unsafe void ReadSubSceneResponse(byte[] msg, out ResolvedSubSceneID resolvedSubSceneId, out NativeArray<RuntimeGlobalObjectId> dependencies)
        {
            fixed(byte* ptr = msg)
            {
                var reader = new UnsafeAppendBuffer.Reader(ptr, msg.Length);
                reader.ReadNext(out resolvedSubSceneId);
                reader.ReadNext(out dependencies, Allocator.Temp);
            }
        }

        static unsafe string ReadArtifactName(byte[] msg)
        {
            fixed(byte* ptr = msg)
            {
                var reader = new UnsafeAppendBuffer.Reader(ptr, msg.Length);
                reader.ReadNext(out string artifactName);
                return artifactName;
            }
        }

        [UnityTest]
        public IEnumerator EditorSendsSubSceneEndToEnd()
        {
            DoConnect();

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s_SubSceneWithSections.SceneAsset, out var subSceneGuidString, out long _);
            var subSceneGuid = new SubSceneGUID
            {
                Guid = new GUID(subSceneGuidString),
                BuildConfigurationGuid = s_LiveLinkBuildConfigGuid,
            };
            Assert.IsTrue(subSceneGuid.Guid.IsValid);
            var sentSubSceneId = new ResolvedSubSceneID
            {
                SubSceneGUID = subSceneGuid,
                TargetHash = EntityScenesPaths.GetSubSceneArtifactHash(subSceneGuid.Guid, subSceneGuid.BuildConfigurationGuid, ImportMode.Synchronous)
            };

            s_Connection.PostMessage(1, LiveLinkMsg.PlayerRequestSubSceneForGUID, sentSubSceneId);
            foreach (var v in WaitForMessage())
                yield return v;

            // we first expect to get some build artifact messages
            List<string> receivedFiles = new List<string>();
            while (s_Connection.IncomingMessages.Count > 0 && s_Connection.IncomingMessages.Peek().Id == LiveLinkMsg.EditorSendBuildArtifact)
            {
                var artifactMsg = AssertNextMessage(LiveLinkMsg.EditorSendBuildArtifact, 1);

                // read data: int - length, string - filename, rest - payload
                receivedFiles.Add(ReadArtifactName(artifactMsg.Data));
            }

            var msg = AssertNextMessage(LiveLinkMsg.EditorResponseSubSceneForGUID, 1);
            ReadSubSceneResponse(msg.Data, out var receivedSubSceneId, out var runtimeGlobalObjectIds);
            Assert.AreEqual(sentSubSceneId, receivedSubSceneId);
            using (runtimeGlobalObjectIds)
            {
#if !UNITY_DISABLE_MANAGED_COMPONENTS
                Assert.Greater(runtimeGlobalObjectIds.Length, 0, "Runtime references are empty!");
                Assert.IsTrue(runtimeGlobalObjectIds.Any(x => x.AssetGUID == (Hash128)s_TempMaterialGuid), "Runtime references does not include material");
                Assert.IsTrue(runtimeGlobalObjectIds.Any(x => x.AssetGUID == (Hash128)s_TempTextureGuid), "Runtime references does not include texture");
#endif
            }

            var targetHash = ((UnityEngine.Hash128)receivedSubSceneId.TargetHash).ToString();
            AssertFileReceived($"{targetHash}.0.entities");
            AssertFileReceived($"{targetHash}.2.entities");
#if !UNITY_DISABLE_MANAGED_COMPONENTS
            AssertFileReceived($"{targetHash}.2.refguids");
            AssertFileReceived($"{targetHash}.2.bundle");
#endif
            AssertFileReceived($"{targetHash}.entityheader");

            AssertNumMessages(0);

            void AssertFileReceived(string endsWith)
            {
                Assert.IsTrue(receivedFiles.Any(p => p.EndsWith(endsWith)), $"Did not receive a file that ends with \"{endsWith}\". " +
                    $"Files received are:\n{string.Join("\n", receivedFiles)}");
            }
        }

        [Test]
        public void EditorRespondsToConnection()
        {
            s_Connection.PostConnect(1);
            s_Connection.IncomingMessages.Clear();

            s_Connection.PostMessage(1, LiveLinkMsg.PlayerRequestConnectLiveLink, s_LiveLinkBuildConfigGuid);

            var msg = AssertNextMessage(LiveLinkMsg.EditorResponseConnectLiveLink, 1);
            using (NativeArray<Hash128> sceneGUIDs = ReceiveArray<Hash128>(msg))
            {
                Assert.AreEqual(1, sceneGUIDs.Length);
                Assert.AreEqual(AssetDatabase.AssetPathToGUID(s_TempScene.path), sceneGUIDs[0].ToString());
            }

            AssertNumMessages(0);

            s_Connection.PostDisconnect(1);
        }

        [Test]
        public void EditorRespondsToHandshake()
        {
            var editorLiveLinkId = LiveLinkUtility.GetEditorLiveLinkId();
            s_Connection.PostConnect(1);
            s_Connection.IncomingMessages.Clear();

            s_Connection.PostMessage(1, LiveLinkMsg.PlayerRequestHandshakeLiveLink, s_LiveLinkBuildConfigGuid);

            var msg = AssertNextMessage(LiveLinkMsg.EditorResponseHandshakeLiveLink, 1);
            var incomingId = msg.EventArgs.Receive<long>();
            Assert.AreEqual(editorLiveLinkId, incomingId);

            AssertNumMessages(0);

            s_Connection.PostDisconnect(1);
        }
    }
}
