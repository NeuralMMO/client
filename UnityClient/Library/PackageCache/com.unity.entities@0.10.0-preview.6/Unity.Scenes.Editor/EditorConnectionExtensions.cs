using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Networking.PlayerConnection;

namespace Unity.Scenes
{
    internal static class EditorConnectionExtensions
    {
        public static void Send<T>(this IEditorConnection connection, Guid msgGuid, T data, int playerId = 0) where T : unmanaged
        {
            connection.Send(msgGuid, MessageEventArgsExtensions.SerializeUnmanaged(ref data), playerId);
        }

        public static void SendArray<T>(this IEditorConnection connection, Guid msgGuid, NativeArray<T> data, int playerId = 0) where T : unmanaged
        {
            connection.Send(msgGuid, MessageEventArgsExtensions.SerializeUnmanagedArray(data), playerId);
        }
    }
}
