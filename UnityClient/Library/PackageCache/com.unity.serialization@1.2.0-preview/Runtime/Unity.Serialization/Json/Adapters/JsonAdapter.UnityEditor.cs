#if UNITY_EDITOR
namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter : IJsonAdapter
        , IJsonAdapter<UnityEditor.GUID>
        , IJsonAdapter<UnityEditor.GlobalObjectId>
    {
        void IJsonAdapter<UnityEditor.GUID>.Serialize(JsonStringBuffer writer, UnityEditor.GUID value)
            => writer.WriteEncodedJsonString(value.ToString());

        UnityEditor.GUID IJsonAdapter<UnityEditor.GUID>.Deserialize(SerializedValueView view)
            => UnityEditor.GUID.TryParse(view.ToString(), out var value) ? value : default;
        
        void IJsonAdapter<UnityEditor.GlobalObjectId>.Serialize(JsonStringBuffer writer, UnityEditor.GlobalObjectId value)
            => writer.WriteEncodedJsonString(value.ToString());

        UnityEditor.GlobalObjectId IJsonAdapter<UnityEditor.GlobalObjectId>.Deserialize(SerializedValueView view)
            => UnityEditor.GlobalObjectId.TryParse(view.ToString(), out var value) ? value : default;
    }
}
#endif
