using Unity.BuildSystem.NativeProgramSupport;
using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters;
using UnityEditor;

namespace Unity.Build.Classic.Private
{
    sealed class PlatformJsonAdapter : IJsonAdapter<Platform>
    {
        [InitializeOnLoadMethod]
        static void Register() => JsonSerialization.AddGlobalAdapter(new PlatformJsonAdapter());

        public void Serialize(JsonStringBuffer writer, Platform value)
        {
            string json = null;
            if (value != null)
            {
                json = value.Name;
            }
            writer.WriteEncodedJsonString(json);
        }

        public Platform Deserialize(SerializedValueView view)
        {
            if (view.Type != TokenType.String)
            {
                return null;
            }

            var json = view.AsStringView().ToString();
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return TypeCacheHelper.ConstructTypeDerivedFrom<Platform>(platform => platform.Name == json, false);
        }
    }
}
