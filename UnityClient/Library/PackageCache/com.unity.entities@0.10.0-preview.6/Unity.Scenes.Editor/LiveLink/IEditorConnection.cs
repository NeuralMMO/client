using System;
using System.Collections.Generic;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine.Networking.PlayerConnection;

namespace Unity.Scenes
{
#if UNITY_EDITOR
    interface IEditorConnection : IEditorPlayerConnection
    {
        void Send(Guid guid, byte[] message, int playerId);
    }
#endif
}
