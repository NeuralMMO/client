using System;
using Unity.Serialization.Json;
using Unity.Serialization.Json.Adapters;
using UnityEditor;

namespace Unity.Build
{
    sealed class BuildPipelineBaseJsonAdapter : IJsonAdapter<BuildPipelineBase>
    {
        [InitializeOnLoadMethod]
        static void Register() => JsonSerialization.AddGlobalAdapter(new BuildPipelineBaseJsonAdapter());

        public void Serialize(JsonStringBuffer writer, BuildPipelineBase value)
        {
            string json = null;
            if (value != null)
            {
                json = value.GetType().GetQualifedAssemblyTypeName();
            }
            writer.WriteEncodedJsonString(json);
        }

        public BuildPipelineBase Deserialize(SerializedValueView view)
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

            if (TypeConstructionHelper.TryConstructFromAssemblyQualifiedTypeName<BuildPipelineBase>(json, out var step))
            {
                return step;
            }

            throw new ArgumentException($"Failed to construct type. Could not resolve type from TypeName=[{json}].");
        }
    }
}
