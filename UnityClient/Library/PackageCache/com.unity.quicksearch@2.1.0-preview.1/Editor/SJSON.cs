using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEditor;

using JSONObject = System.Collections.IDictionary;

namespace Unity.QuickSearch
{
    static class SJSON
    {
        private static readonly MethodInfo s_EncodeMethod;
        private static readonly MethodInfo s_EncodeObjectMethod;
        private static readonly MethodInfo s_DecodeMethod;
        private static readonly MethodInfo s_DecodeObjectMethod;

        static SJSON()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.SJSON");
            s_EncodeMethod = type.GetMethod("Encode");
            s_EncodeObjectMethod = type.GetMethod("EncodeObject");
            s_DecodeMethod = type.GetMethod("Decode");
            s_DecodeObjectMethod = type.GetMethod("DecodeObject");
        }

        public static string Encode(JSONObject t)
        {
            return (string)s_EncodeMethod.Invoke(null, new object[] { t });
        }

        public static string EncodeObject(object o)
        {
            return (string)s_EncodeObjectMethod.Invoke(null, new object[] { o });
        }

        public static JSONObject Decode(byte[] sjson)
        {
            return (JSONObject)s_DecodeMethod.Invoke(null, new object[] { sjson });
        }

        public static object DecodeObject(byte[] sjson)
        {
            return s_DecodeObjectMethod.Invoke(null, new object[] { sjson });
        }

        public static JSONObject Load(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            var readResult = File.ReadAllBytes(path);
            try
            {
                return Decode(readResult);
            }
            catch (UnityException ex)
            {
                throw new UnityException(ex.Message.Replace("(memory)", $"({path})"));
            }
        }

        public static JSONObject LoadString(string json)
        {
            if (String.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            try
            {
                return Decode(Encoding.UTF8.GetBytes(json));
            }
            catch (UnityException ex)
            {
                throw new UnityException(ex.Message.Replace("(memory)", "(string)"), ex);
            }
        }

        public static byte[] GetBytes(JSONObject data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return Encoding.UTF8.GetBytes(Encode(data));
        }

        public static bool Save(JSONObject h, string path)
        {
            var s = Encode(h);
            if (File.Exists(path))
            {
                var oldS = File.ReadAllText(path, Encoding.GetEncoding(0));
                if (s.Equals(oldS))
                    return false;
            }

            var bytes = Encoding.UTF8.GetBytes(s);
            File.WriteAllBytes(path, bytes);
            return true;
        }

        public static bool TryGetValue(JSONObject data, string key, out object value)
        {
            value = null;
            if (data == null || !data.Contains(key))
                return false;

            value = data[key];
            return true;
        }
    }
}