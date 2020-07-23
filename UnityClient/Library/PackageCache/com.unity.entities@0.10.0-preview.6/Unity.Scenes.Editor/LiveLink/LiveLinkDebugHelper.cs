using System;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.Scenes.Editor
{
    static class LiveLinkDebugHelper
    {
        static Dictionary<Guid, string> s_MessageNames;
        static List<Guid> s_EditorMessages;
        static List<Guid> s_PlayerMessages;
        static List<Guid> s_UnknownMessages;

        static void InitMessageData()
        {
            if (s_MessageNames != null)
                return;
            s_MessageNames = new Dictionary<Guid, string>();
            s_EditorMessages = new List<Guid>();
            s_PlayerMessages = new List<Guid>();
            s_UnknownMessages = new List<Guid>();
            var fields = typeof(LiveLinkMsg).GetFields(BindingFlags.Public | BindingFlags.Static);
            var guidType = typeof(Guid);
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType != guidType)
                    continue;
                var guid = (Guid)fields[i].GetValue(null);
                var name = fields[i].Name;
                s_MessageNames[guid] = name;
                if (name.StartsWith(LiveLinkMsg.EditorPrefix))
                    s_EditorMessages.Add(guid);
                else if (name.StartsWith(LiveLinkMsg.PlayerPrefix))
                    s_PlayerMessages.Add(guid);
                else
                    s_UnknownMessages.Add(guid);
            }
        }

        public static IReadOnlyList<Guid> EditorMessages
        {
            get
            {
                InitMessageData();
                return s_EditorMessages;
            }
        }

        public static IReadOnlyList<Guid> PlayerMessages
        {
            get
            {
                InitMessageData();
                return s_PlayerMessages;
            }
        }

        public static IReadOnlyList<Guid> UnknownMessages
        {
            get
            {
                InitMessageData();
                return s_UnknownMessages;
            }
        }

        public static string GetMessageName(Guid guid)
        {
            InitMessageData();
            return s_MessageNames.TryGetValue(guid, out var name) ? name : null;
        }
    }
}
