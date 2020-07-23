using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEditor;

namespace Unity.Entities.Editor
{
    static class SessionState<T>
        where T : class, new()
    {
        static readonly Dictionary<string, T> s_Cache = new Dictionary<string, T>();

        static SessionState()
        {
            AssemblyReloadEvents.beforeAssemblyReload += () =>
            {
                foreach (var kvp in s_Cache)
                    SessionState.SetString(kvp.Key, JsonSerialization.ToJson(kvp.Value));
            };
        }

        public static T GetOrCreateState(string key)
        {
            if (s_Cache.TryGetValue(key, out var value)) return value;
            var json = SessionState.GetString(key, string.Empty);
            value = string.IsNullOrEmpty(json) ? new T() : JsonSerialization.FromJson<T>(json);
            s_Cache.Add(key, value);
            return value;
        }
    }
}
