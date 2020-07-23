#if !UNITY_DOTSPLAYER
using System;
using Unity.Collections;
using UnityObject = UnityEngine.Object;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        Contravariant.IJsonAdapter<UnityObject>
    {
        void Contravariant.IJsonAdapter<UnityObject>.Serialize(JsonStringBuffer writer, UnityObject value)
        {
#if UNITY_EDITOR
            var id = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
            writer.WriteEncodedJsonString(id);
#else
            writer.Write("null");
#endif
        }

        object Contravariant.IJsonAdapter<UnityObject>.Deserialize(SerializedValueView view)
        {
#if UNITY_EDITOR
            if (view.Type == TokenType.String)
            {
                if (UnityEditor.GlobalObjectId.TryParse(view.ToString(), out var id))
                {
                    if (id.assetGUID.Empty())
                    {
                        return null;
                    }

                    var obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                    if (obj == null || !obj)
                    {
                        throw new InvalidOperationException($"An error occured while deserializing asset reference GUID=[{id.assetGUID.ToString()}]. Asset is not yet loaded and will result in a null reference.");
                    }

                    return obj;
                }
            }

            if (view.Type == TokenType.Object)
            {
                return FromObjectHandle(view.AsObjectView());
            }
#endif
            return null;
        }
    
#if UNITY_EDITOR
        static readonly string s_EmptyGuid = Guid.Empty.ToString();

        class Container
        {
#pragma warning disable 649
            public UnityObject o;
#pragma warning restore 649
        }
        
        public static UnityObject FromObjectHandle(SerializedObjectView objectView)
        {
            var container = new Container();

            objectView.TryGetValueAsString("Guid", out var guid);
            objectView.TryGetValueAsInt64("FileId", out var fileId);
            objectView.TryGetValueAsInt64("Type", out var type);

            if (guid == s_EmptyGuid || guid == string.Empty)
                return null;

            using (var writer = new JsonStringBuffer(256, Allocator.Temp))
            {
                writer.Write("{\"o\":{");
                writer.Write("\"fileID\":");
                writer.Write(fileId);
                writer.Write(",\"guid\":\"");
                writer.Write(guid);
                writer.Write("\",\"type\": ");
                writer.Write(type);
                writer.Write("}}");
                
                var json = writer.ToString();
                UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, container);
                return container.o;
            }
        }
#endif
    }
}
#endif
