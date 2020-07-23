using System;
using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEngine;

namespace Unity.Serialization.Editor
{
    /// <summary>
    /// Wrapper utility around <see cref="UnityEditor.EditorUserSettings"/> to allow saving arbitrary data as User settings.
    /// </summary>
    /// <typeparam name="T">The type of the setting</typeparam>
    public static class UserSettings<T>
        where T : class, new()
    {
        static readonly Dictionary<string, T> s_Cache = new Dictionary<string, T>();
 
        static UserSettings()
        {
            EditorApplication.quitting += Save;
            AssemblyReloadEvents.beforeAssemblyReload += Save;
        }

        /// <summary>
        /// Clears the data stored at the provided key.
        /// </summary>
        /// <param name="key">The key to the data.</param>
        public static void Clear(string key)
        {
            s_Cache.Remove(key);
            EditorUserSettings.SetConfigValue(key, null);
        }
        
        /// <summary>
        /// Returns an instance of <see cref="T"/> for the provided key.  
        /// </summary>
        /// <param name="key">The key to the data.</param>
        /// <returns>The <see cref="T"/> instance.</returns>
        public static T GetOrCreate(string key) 
        {
            if (s_Cache.TryGetValue(key, out var value))
                return value;
            
            var json = EditorUserSettings.GetConfigValue(key) ?? string.Empty;
            try
            {
                value = string.IsNullOrEmpty(json) ? new T() : JsonSerialization.FromJson<T>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UserSettings<{typeof(T).Name}>: Could not load at key `{key}`.\nException `{exception}`");
                value = new T();
            }

            s_Cache.Add(key, value);
            return value;
        }

        static void Save()
        {
            foreach (var kvp in s_Cache)
                EditorUserSettings.SetConfigValue(kvp.Key, JsonSerialization.ToJson(kvp.Value));
        }
    }
}