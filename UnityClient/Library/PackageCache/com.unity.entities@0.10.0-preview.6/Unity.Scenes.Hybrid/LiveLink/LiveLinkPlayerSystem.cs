using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Networking.PlayerConnection;

namespace Unity.Scenes
{
    struct ResourceGUID : IComponentData
    {
        public Hash128 Guid;
    }

    struct SubSceneGUID : IComponentData, IEquatable<SubSceneGUID>
    {
        public Hash128 Guid;
        public Hash128 BuildConfigurationGuid;

        public SubSceneGUID(Hash128 Guid, Hash128 buildConfigurationGuid)
        {
            this.Guid = Guid;
            this.BuildConfigurationGuid = buildConfigurationGuid;
        }

        public override bool Equals(object o)
        {
            return o is SubSceneGUID other && Equals(other);
        }

        public bool Equals(SubSceneGUID other)
        {
            return Guid.Equals(other.Guid) && BuildConfigurationGuid.Equals(other.BuildConfigurationGuid);
        }

        public override unsafe int GetHashCode()
        {
            var stackCopy = this;
            return (int)math.hash(&stackCopy, sizeof(SubSceneGUID));
        }

        public override string ToString()
        {
            return $"{Guid} | {BuildConfigurationGuid}";
        }

        public static bool operator==(SubSceneGUID lhs, SubSceneGUID rhs) => lhs.Equals(rhs);
        public static bool operator!=(SubSceneGUID lhs, SubSceneGUID rhs) => !lhs.Equals(rhs);
    }

    struct ResourceLoaded : IComponentData
    {
    }

#if UNITY_EDITOR
    [DisableAutoCreation]
#endif
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(LiveLinkRuntimeSystemGroup))]
    class LiveLinkPlayerSystem : ComponentSystem
    {
        Queue<EntityChangeSetSerialization.ResourcePacket> m_ResourcePacketQueue;
        LiveLinkPatcher                                    m_Patcher;
        EntityQuery                                        m_Resources;
        LiveLinkSceneChangeTracker                         m_LiveLinkSceneChange;
        bool                                               m_DidRequestConnection;
        IEditorPlayerConnection                            m_Connection;
        internal IEditorPlayerConnection Connection => m_Connection;

#if UNITY_EDITOR
        internal void SetConnection(IEditorPlayerConnection connection)
        {
            m_Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

#endif

        static double                                      kSessionHandshakeTimeout = 5;
        double                                             m_SessionHandshakeTimeoutTimstamp;
        bool                                               m_SessionHandshake;

        protected override void OnCreate()
        {
            m_Connection = PlayerConnection.instance;
        }

        protected override void OnStartRunning()
        {
            Debug.Log("Initializing live link");
            m_Connection.Register(LiveLinkMsg.EditorResponseHandshakeLiveLink, ResponseSessionHandshake);
            m_Connection.Register(LiveLinkMsg.EditorReceiveEntityChangeSet, ReceiveEntityChangeSet);
            m_Connection.Register(LiveLinkMsg.EditorUnloadScenes, ReceiveUnloadScenes);
            m_Connection.Register(LiveLinkMsg.EditorLoadScenes, ReceiveLoadScenes);
            m_Connection.Register(LiveLinkMsg.EditorResetGame, ReceiveResetGame);
            m_Connection.Register(LiveLinkMsg.EditorResponseConnectLiveLink, ReceiveInitialScenes);

            m_ResourcePacketQueue = new Queue<EntityChangeSetSerialization.ResourcePacket>();
            m_Resources = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<ResourceGUID>() }
            });

            m_Patcher = new LiveLinkPatcher(World);

            m_LiveLinkSceneChange = new LiveLinkSceneChangeTracker(EntityManager);

            // Send handshake and set timeout
            PlayerConnection.instance.Send(LiveLinkMsg.PlayerRequestHandshakeLiveLink, new byte[0]);
            m_SessionHandshakeTimeoutTimstamp = Time.ElapsedTime + kSessionHandshakeTimeout;
            m_SessionHandshake = false;
        }

        protected override void OnStopRunning()
        {
            m_LiveLinkSceneChange.Dispose();
            m_Connection.Unregister(LiveLinkMsg.EditorResponseHandshakeLiveLink, ResponseSessionHandshake);
            m_Connection.Unregister(LiveLinkMsg.EditorResponseConnectLiveLink, ReceiveInitialScenes);
            m_Connection.Unregister(LiveLinkMsg.EditorReceiveEntityChangeSet, ReceiveEntityChangeSet);
            m_Connection.Unregister(LiveLinkMsg.EditorUnloadScenes, ReceiveUnloadScenes);
            m_Connection.Unregister(LiveLinkMsg.EditorLoadScenes, ReceiveLoadScenes);
            m_Connection.Unregister(LiveLinkMsg.EditorResetGame, ReceiveResetGame);
        }

        void ResponseSessionHandshake(MessageEventArgs args)
        {
            var editorLiveLinkId = args.Receive<long>();
            if (editorLiveLinkId != LiveLinkUtility.LiveLinkId)
            {
                Debug.LogError("Editor returned invalid LiveLink Id. We are probably connected to the wrong Editor (or an AssetImportWorker). Close extra Editors and run the LiveLink player again.");
                LiveLinkUtility.DisableLiveLink();
                World.GetExistingSystem<LiveLinkRuntimeSystemGroup>().Enabled = false;
                return;
            }

            m_SessionHandshake = true;
        }

        void SendSetLoadedScenes()
        {
            if (m_LiveLinkSceneChange.GetSceneMessage(out var msg))
            {
                LiveLinkMsg.LogSend($"SetLoadedScenes: Loaded {msg.LoadedScenes.ToDebugString()}, Removed {msg.RemovedScenes.ToDebugString()}");
                m_Connection.Send(LiveLinkMsg.PlayerSetLoadedScenes, msg.ToMsg());
                msg.Dispose();
            }
        }

        void ReceiveEntityChangeSet(MessageEventArgs args)
        {
            var resourcePacket = new EntityChangeSetSerialization.ResourcePacket(args.data);

            LiveLinkMsg.LogReceived($"EntityChangeSet patch: '{args.data.Length}' bytes, " +
                $"object GUIDs: {resourcePacket.GlobalObjectIds.ToDebugString(id => id.AssetGUID.ToString())}");

            m_ResourcePacketQueue.Enqueue(resourcePacket);
        }

        void ReceiveUnloadScenes(MessageEventArgs args)
        {
            using (var scenes = args.ReceiveArray<Hash128>())
            {
                LiveLinkMsg.LogReceived($"UnloadScenes {scenes.ToDebugString()}");
                foreach (var scene in scenes)
                {
                    m_Patcher.UnloadScene(scene);
                }
            }
        }

        void ReceiveLoadScenes(MessageEventArgs args)
        {
            using (var scenes = args.ReceiveArray<Hash128>())
            {
                LiveLinkMsg.LogReceived($"LoadScenes {scenes.ToDebugString()}");
                foreach (var scene in scenes)
                {
                    m_Patcher.TriggerLoad(scene);
                }
            }
        }

        void ReceiveResetGame(MessageEventArgs args)
        {
            LiveLinkMsg.LogReceived("ResetGame");
            ResetGame();
        }

        void ResetGame()
        {
            var sceneSystem = World.GetExistingSystem<SceneSystem>();
            sceneSystem.UnloadAllScenes();

            while (m_ResourcePacketQueue.Count != 0)
                m_ResourcePacketQueue.Dequeue().Dispose();

            EntityManager.DestroyEntity(EntityManager.UniversalQuery);
            LiveLinkPlayerAssetRefreshSystem.Reset();

            LiveLinkMsg.LogSend("ConnectLiveLink");
            m_Connection.Send(LiveLinkMsg.PlayerRequestConnectLiveLink, sceneSystem.BuildConfigurationGUID);

            m_LiveLinkSceneChange.Reset();
            SendSetLoadedScenes();
        }

        void ReceiveInitialScenes(MessageEventArgs args)
        {
            using (var scenes = args.ReceiveArray<Hash128>())
            {
                if (scenes.Length > 0)
                {
                    LiveLinkMsg.LogReceived($"ReceiveInitialScenes {scenes.ToString()}");
                    var sceneSystem = World.GetOrCreateSystem<SceneSystem>();
                    for (int i = 0; i < scenes.Length; i++)
                        sceneSystem.LoadSceneAsync(scenes[i], new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.LoadAdditive | SceneLoadFlags.LoadAsGOScene });
                }
            }
        }

#pragma warning disable 618
        [BurstCompile]
        struct BuildResourceMapJob : IJobForEachWithEntity<ResourceGUID>
        {
            [ReadOnly]
            public ComponentDataFromEntity<ResourceLoaded> ResourceLoaded;

            public NativeHashMap<Hash128, bool> GuidResourceReady;

            public void Execute(Entity entity, int index, ref ResourceGUID resourceGuid)
            {
                var guid = resourceGuid.Guid;
                GuidResourceReady[guid] = ResourceLoaded.HasComponent(entity);
            }
        }
#pragma warning restore 618

        public bool IsResourceReady(NativeArray<RuntimeGlobalObjectId> resourceGuids)
        {
            var guidResourceReady = new NativeHashMap<Hash128, bool>(m_Resources.CalculateEntityCount(), Allocator.Persistent);

            new BuildResourceMapJob
            {
                ResourceLoaded = GetComponentDataFromEntity<ResourceLoaded>(),
                GuidResourceReady = guidResourceReady
            }.Run(m_Resources);

            var isResourceReady = true;
            var archetype = EntityManager.CreateArchetype(typeof(ResourceGUID));

            for (int i = 0; i < resourceGuids.Length; i++)
            {
                var guid = resourceGuids[i].AssetGUID;
                var found = guidResourceReady.ContainsKey(guid);
                if (!found)
                {
                    guidResourceReady.TryAdd(guid, false);

                    var entity = EntityManager.CreateEntity(archetype);
                    EntityManager.SetComponentData(entity, new ResourceGUID
                    {
                        Guid = guid
                    });
                }

                var ready = guidResourceReady[guid];
                if (!ready)
                {
                    isResourceReady = false;
                }
            }

            guidResourceReady.Dispose();
            return isResourceReady;
        }

        protected override void OnUpdate()
        {
            // BuildConfigurationGUID isn't known in OnCreate since it could be configured from OnCreate of other systems,
            // So we delay connecting live link until first OnUpdate
            if (!m_DidRequestConnection)
            {
                if (!m_SessionHandshake)
                {
                    if (m_SessionHandshakeTimeoutTimstamp < Time.ElapsedTime)
                    {
                        Debug.LogError("LiveLink handshake timed out. This may be because your player connection is connected to an AssetWorker process or incorrect Editor.");
                        World.GetExistingSystem<LiveLinkRuntimeSystemGroup>().Enabled = false;
                    }

                    return;
                }

                m_DidRequestConnection = true;
                LiveLinkMsg.LogSend("ConnectLiveLink");
                m_Connection.Send(LiveLinkMsg.PlayerRequestConnectLiveLink, World.GetExistingSystem<SceneSystem>().BuildConfigurationGUID);
            }

            SendSetLoadedScenes();

            while (m_ResourcePacketQueue.Count != 0 && IsResourceReady(m_ResourcePacketQueue.Peek().GlobalObjectIds))
            {
                LiveLinkMsg.LogInfo($"Applying changeset ({m_ResourcePacketQueue.Count-1} left in queue)");
                using (var resourcePacket = m_ResourcePacketQueue.Dequeue())
                {
                    ApplyChangeSet(resourcePacket);
                }
            }
        }

        void ApplyChangeSet(EntityChangeSetSerialization.ResourcePacket resourcePacket)
        {
            var changeSet = LiveLinkChangeSet.Deserialize(resourcePacket, LiveLinkPlayerAssetRefreshSystem.GlobalAssetObjectResolver);
            m_Patcher.ApplyPatch(changeSet);
            changeSet.Dispose();
        }
    }
}
