using System.IO;
using Unity.Entities.Hybrid;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEngine;

namespace Unity.Scenes
{
    internal class LiveLinkUtility
    {
        #if UNITY_EDITOR
        public static unsafe void WriteBootstrap(string path, Entities.Hash128 buildConfigurationGUID)
        {
            long handshakeId = GetEditorLiveLinkId();
            using (var stream = new StreamBinaryWriter(path))
            {
                stream.WriteBytes(&buildConfigurationGUID, sizeof(GUID));
                stream.WriteBytes(&handshakeId, sizeof(long));
            }
        }

        public static long GetEditorLiveLinkId()
        {
            return Application.dataPath.GetHashCode();
        }

        #endif

        public const string LiveLinkBootstrapFileName = "livelink-bootstrap";
        // Used to verify the connection between a LiveLink Player and Editor is valid.
        // In this case we use the Application's data path (hashed) and store it in the bootstrap file.
        // If the LiveLink player connects to a different Editor instance or the Editor was launched with the wrong project, it will display an error on the Player.
        public static long LiveLinkId { get; private set; }
        public static bool LiveLinkEnabled { get; private set; }
        public static Entities.Hash128 BuildConfigurationGUID { get; private set; }

        static bool liveLinkBooted;
        static string bootstrapPath;

        public static void DisableLiveLink()
        {
            LiveLinkEnabled = false;
        }

        public static void LiveLinkBoot()
        {
            if (liveLinkBooted)
                return;

            liveLinkBooted = true;
            bootstrapPath = Path.Combine(Application.streamingAssetsPath, LiveLinkBootstrapFileName);

            LiveLinkEnabled = FileUtilityHybrid.FileExists(bootstrapPath);

            if (LiveLinkEnabled)
            {
                if (!UnityEngine.Networking.PlayerConnection.PlayerConnection.instance.isConnected)
                    Debug.LogError(
                        "Failed to connect to the Editor.\nAn Editor connection is required for LiveLink to work.");

                ReadBootstrap();
            }
        }

        private static unsafe void ReadBootstrap()
        {
            var path = bootstrapPath;
            using (var rdr = new StreamBinaryReader(path))
            {
                Entities.Hash128 guid = default;
                long id = 0;

                rdr.ReadBytes(&guid, sizeof(Entities.Hash128));
                rdr.ReadBytes(&id, sizeof(long));

                BuildConfigurationGUID = guid;
                LiveLinkId = id;
            }
        }
    }
}
