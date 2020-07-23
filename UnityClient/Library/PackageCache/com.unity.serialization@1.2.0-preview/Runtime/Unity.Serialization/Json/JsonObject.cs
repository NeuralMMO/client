using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// The default object output by <see cref="JsonSerialization"/> if an object type can not be resolved.
    /// </summary>
    public class JsonObject : Dictionary<string, object>
    {
        static JsonObject()
        {
            PropertyBagStore.AddPropertyBag(new DictionaryPropertyBag<JsonObject, string, object>());
        }
    }

    /// <summary>
    /// The default object output by <see cref="JsonSerialization"/> if an array type can not be resolved.
    /// </summary>
    public class JsonArray : List<object>
    {
        static JsonArray()
        {
            PropertyBagStore.AddPropertyBag(new ListPropertyBag<JsonArray, object>());
        }
    }
}