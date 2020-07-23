// #define DEBUG_LIVE_LINK
// #define DEBUG_LIVE_LINK_SEND
// #define DEBUG_LIVE_LINK_FILE

using System;
using System.Diagnostics;
using Unity.Collections;
#if DEBUG_LIVE_LINK_FILE
using System.IO;
#endif

namespace Unity.Scenes
{
    internal static class LiveLinkMsg
    {
        // All messages must be prefixed by who is sending them.
        public const string EditorPrefix = "Editor";
        public const string PlayerPrefix = "Player";

        public static readonly Guid PlayerRequestHandshakeLiveLink = new Guid("ee3b9ac439304c98838dcd0245b8b9b1");
        public static readonly Guid EditorResponseHandshakeLiveLink = new Guid("212ce8fe16a043428cd4bfbfc081701a");

        public static readonly Guid EditorReceiveEntityChangeSet = new Guid("6eca862e2e2b442bafb7566181fd989d");
        public static readonly Guid EditorUnloadScenes = new Guid("c34a0cb23efa4fae81f9f78d755cee10");
        public static readonly Guid EditorLoadScenes = new Guid("0d0fd642461447a59c45321269cb392d");

        public static readonly Guid PlayerRequestConnectLiveLink = new Guid("d58c350900c24b1e99e150338fa407b5");
        public static readonly Guid EditorResponseConnectLiveLink = new Guid("0b070511c643476cb31669334ef3ae88");

        //@TODO: Generate guid properly
        public static readonly Guid PlayerSetLoadedScenes = new Guid("f58c350900c24b1e99e150338fa407b6");
        public static readonly Guid EditorResetGame = new Guid("16a2408ca08e48758af41c5f2919d3e4");

        public static readonly Guid PlayerRequestAssetTargetHash = new Guid("a56c8732319341c18daae030959993f4");
        public static readonly Guid EditorResponseAssetTargetHash = new Guid("4c8f736a115f435cb576b92a6f30bd1f");

        public static readonly Guid PlayerRequestAssetForGUID = new Guid("e078f4ebc7f24e328615ba69bcde0d48");
        public static readonly Guid EditorResponseAssetForGUID = new Guid("68163744fe0540468d671f081cbf25cc");

        public static readonly Guid PlayerRequestSubSceneTargetHash = new Guid("5220998d5fdd4c45ab945774d0ea5583");
        public static readonly Guid EditorResponseSubSceneTargetHash = new Guid("decb387da44e4d9e8d7b54ee13c72bf5");
        public static readonly Guid PlayerRequestSubSceneForGUID = new Guid("bc3b54dbbbb140c3aff95f6130326ebc");
        public static readonly Guid EditorResponseSubSceneForGUID = new Guid("6aeb95d61c3048feb87a590f416f027b");

        public static readonly Guid EditorSendBuildArtifact = new Guid("978a5e8e8bbb4d878b00658e37cac302");

        const string k_LogFilePath = "LiveLink.log";

        [Conditional("DEBUG_LIVE_LINK_SEND")]
        public static void LogSend(string msg)
        {
            LogInfo($"send {msg}");
        }

        [Conditional("DEBUG_LIVE_LINK")]
        public static void LogReceived(string msg)
        {
            LogInfo($"received {msg}");
        }

        [Conditional("DEBUG_LIVE_LINK")]
        public static void LogInfo(string msg)
        {
            Debug.Log(msg);
            #if DEBUG_LIVE_LINK_FILE
            File.AppendAllText(k_LogFilePath, $"{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff}: {msg}\n");
            #endif
        }

        public static string ToDebugString<T>(this NativeArray<T> array, Func<T, string> converter = null) where T : struct
        {
            return array.Length == 0
                ? "(-)"
                : $"('{string.Join("', '", array.ToStringArray(converter ?? (item => item.ToString())))}')";
        }

        static string[] ToStringArray<T>(this NativeArray<T> array, Func<T, string> converter) where T : struct
        {
            var stringArray = new string[array.Length];
            for (var i = 0; i < array.Length; i++)
                stringArray[i] = converter(array[i]);

            return stringArray;
        }
    }
}
