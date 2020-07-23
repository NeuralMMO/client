using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;

namespace Unity.Scenes.Editor.Tests
{
    class LiveLinkTestConnection : IEditorConnection
    {
        public struct Message
        {
            public Guid Id;
            public byte[] Data => EventArgs.data;
            public int Player => EventArgs.playerId;
            public MessageEventArgs EventArgs;

            public string FormatBytes() => string.Concat(Data.Select(b => b.ToString("X2")));
            public override string ToString() => $"{LiveLinkDebugHelper.GetMessageName(Id) ?? Id.ToString()} for {Player}, {Data.Length} bytes";
        }

        public readonly Dictionary<Guid, List<UnityAction<MessageEventArgs>>> MessageHandlers = new Dictionary<Guid, List<UnityAction<MessageEventArgs>>>();
        public readonly List<UnityAction<int>> ConnectHandler = new List<UnityAction<int>>();
        public readonly List<UnityAction<int>> DisconnectHandler = new List<UnityAction<int>>();
        public readonly Queue<Message> IncomingMessages = new Queue<Message>();
        public readonly HashSet<int> OpenConnections = new HashSet<int>();

        public string GetMessageString() => GetMessageString(IncomingMessages);
        public string GetMessageString(IEnumerable<Message> msgs) => string.Join("\n", msgs.Select(msg => msg.ToString()));
        public int GetNumMessageHandlers(Guid guid) => MessageHandlers.TryGetValue(guid, out var handlers) ? handlers.Count : 0;

        public void PostConnect(int player)
        {
            try
            {
                foreach (var callback in ConnectHandler) callback(player);
            }
            finally
            {
                OpenConnections.Add(player);
            }
        }

        public void PostDisconnect(int player)
        {
            try
            {
                foreach (var callback in DisconnectHandler) callback(player);
            }
            finally
            {
                OpenConnections.Remove(player);
            }
        }

        public void PostMessage(int player, Guid messageId, byte[] data)
        {
            if (MessageHandlers.TryGetValue(messageId, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    var args = new MessageEventArgs
                    {
                        data = (byte[])data.Clone(),
                        playerId = player,
                    };
                    handler(args);
                }
            }
        }

        public void PostMessage<T>(int player, Guid messageId, T data) where T : unmanaged =>
            PostMessage(player, messageId, MessageEventArgsExtensions.SerializeUnmanaged(ref data));

        public void PostMessageArray<T>(int player, Guid messageId, NativeArray<T> data) where T : unmanaged =>
            PostMessage(player, messageId, MessageEventArgsExtensions.SerializeUnmanagedArray(data));

        public void PostMessageArray<T>(int player, Guid messageId, T[] data) where T : unmanaged =>
            PostMessage(player, messageId, MessageEventArgsExtensions.SerializeUnmanagedArray(data));

        void IEditorPlayerConnection.Register(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            if (!MessageHandlers.TryGetValue(messageId, out var list))
            {
                list = new List<UnityAction<MessageEventArgs>> { callback };
                MessageHandlers[messageId] = list;
            }
            else
                list.Add(callback);
        }

        void IEditorPlayerConnection.Unregister(Guid messageId, UnityAction<MessageEventArgs> callback)
        {
            if (MessageHandlers.TryGetValue(messageId, out var list))
                list.Remove(callback);
        }

        void IEditorPlayerConnection.DisconnectAll() {}
        void IEditorPlayerConnection.RegisterConnection(UnityAction<int> callback) => ConnectHandler.Add(callback);
        void IEditorPlayerConnection.RegisterDisconnection(UnityAction<int> callback) => DisconnectHandler.Add(callback);
        void IEditorPlayerConnection.UnregisterConnection(UnityAction<int> callback) => ConnectHandler.Remove(callback);
        void IEditorPlayerConnection.UnregisterDisconnection(UnityAction<int> callback) => DisconnectHandler.Remove(callback);

        void SendData(Guid messageId, byte[] data, int playerId)
        {
            IncomingMessages.Enqueue(new Message
            {
                Id = messageId,
                EventArgs = new MessageEventArgs
                {
                    data = (byte[])data.Clone(),
                    playerId = playerId
                }
            });
        }

        void IEditorPlayerConnection.Send(Guid messageId, byte[] data) => SendData(messageId, data, 0);

        bool IEditorPlayerConnection.TrySend(Guid messageId, byte[] data)
        {
            SendData(messageId, data, 0);
            return true;
        }

        void IEditorConnection.Send(Guid messageId, byte[] data, int playerId)
        {
            SendData(messageId, data, playerId);
        }
    }
}
