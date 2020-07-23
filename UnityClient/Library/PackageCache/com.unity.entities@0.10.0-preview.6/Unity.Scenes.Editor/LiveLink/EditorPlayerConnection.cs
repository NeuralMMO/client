using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;

namespace Unity.Scenes.Editor
{
    class EditorPlayerConnection : IEditorConnection
    {
        readonly EditorConnection m_Connection;
        public EditorPlayerConnection([NotNull] EditorConnection connection)
        {
            m_Connection = connection ? connection : throw new ArgumentNullException(nameof(connection));
        }

        public void Register(Guid messageId, UnityAction<MessageEventArgs> callback) => m_Connection.Register(messageId, callback);
        public void Unregister(Guid messageId, UnityAction<MessageEventArgs> callback) => m_Connection.Unregister(messageId, callback);
        public void DisconnectAll() => m_Connection.DisconnectAll();
        public void RegisterConnection(UnityAction<int> callback) => m_Connection.RegisterConnection(callback);
        public void RegisterDisconnection(UnityAction<int> callback) => m_Connection.RegisterDisconnection(callback);
        public void UnregisterConnection(UnityAction<int> callback) => m_Connection.UnregisterConnection(callback);
        public void UnregisterDisconnection(UnityAction<int> callback) => m_Connection.UnregisterConnection(callback);
        public void Send(Guid messageId, byte[] data) => m_Connection.Send(messageId, data);
        public bool TrySend(Guid messageId, byte[] data) => m_Connection.TrySend(messageId, data);
        public void Send(Guid guid, byte[] message, int playerId) => m_Connection.Send(guid, message, playerId);
    }
}
