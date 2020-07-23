using System;
using System.Collections.Generic;
using Unity.Properties;
using Unity.Properties.Internal;
using Unity.Serialization.Json.Unsafe;

namespace Unity.Serialization.Json.Adapters
{
    struct JsonAdapterCollection
    {
        public JsonAdapter InternalAdapter;
        public List<IJsonAdapter> Global;
        public List<IJsonAdapter> UserDefined;

        public bool TrySerialize<TValue>(JsonStringBuffer writer, ref TValue value)
        {
            if (null != UserDefined && UserDefined.Count > 0)
            {
                foreach (var adapter in UserDefined)
                {
                    if (TrySerializeAdapter(adapter, writer, ref value))
                    {
                        return true;
                    }
                }
            }

            if (null != Global && Global.Count > 0)
            {
                foreach (var adapter in Global)
                {
                    if (TrySerializeAdapter(adapter, writer, ref value))
                    {
                        return true;
                    }
                }
            }

            if (TrySerializeAdapter(InternalAdapter, writer, ref value))
            {
                return true;
            }

#if UNITY_EDITOR
            if (TrySerializeLazyLoadReference(writer, ref value))
            {
                return true;
            }
#endif

            return false;
        }

        static bool TrySerializeAdapter<TValue>(IJsonAdapter adapter, JsonStringBuffer writer, ref TValue value)
        {
            if (adapter is IJsonAdapter<TValue> typed)
            {
                typed.Serialize(writer, value);
                return true;
            }

            if (adapter is Adapters.Contravariant.IJsonAdapter<TValue> typedContravariant)
            {
                typedContravariant.Serialize(writer, value);
                return true;
            }

            return false;
        }

        public bool TryDeserialize<TValue>(UnsafeValueView view, ref TValue value, List<DeserializationEvent> events)
        {
            if (null != UserDefined && UserDefined.Count > 0)
            {
                foreach (var adapter in UserDefined)
                {
                    if (TryDeserializeAdapter(adapter, view, ref value, events))
                    {
                        return true;
                    }
                }
            }

            if (null != Global && Global.Count > 0)
            {
                foreach (var adapter in Global)
                {
                    if (TryDeserializeAdapter(adapter, view, ref value, events))
                    {
                        return true;
                    }
                }
            }

            if (TryDeserializeAdapter(InternalAdapter, view, ref value, events))
            {
                return true;
            }

#if UNITY_EDITOR
            if (TryDeserializeLazyLoadReference(view.AsSafe(), ref value, events))
            {
                return true;
            }
#endif

            return false;
        }

        static bool TryDeserializeAdapter<TValue>(IJsonAdapter adapter, UnsafeValueView view, ref TValue value, List<DeserializationEvent> events)
        {
            if (adapter is IJsonAdapter<TValue> typed)
            {
                try
                {
                    value = typed.Deserialize(view.AsSafe());
                }
                catch (Exception e)
                {
                    events.Add(new DeserializationEvent(EventType.Exception, e));
                }
                return true;
            }

            if (adapter is Adapters.Contravariant.IJsonAdapter<TValue> typedContravariant)
            {
                try
                {
                    // @TODO Type checking on return value.
                    value = (TValue)typedContravariant.Deserialize(view.AsSafe());
                }
                catch (Exception e)
                {
                    events.Add(new DeserializationEvent(EventType.Exception, e));
                }
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        const string k_LazyLoadReference_InstanceID = "m_InstanceID";
        static readonly string s_EmptyGlobalObjectId = new UnityEditor.GlobalObjectId().ToString();

        static bool TrySerializeLazyLoadReference<TValue>(JsonStringBuffer writer, ref TValue value)
        {
            if (!RuntimeTypeInfoCache<TValue>.IsLazyLoadReference)
            {
                return false;
            }

            var instanceID = PropertyContainer.GetValue<TValue, int>(ref value, k_LazyLoadReference_InstanceID);
#if UNITY_2020_1_OR_NEWER
            writer.WriteEncodedJsonString(UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(instanceID).ToString());
#else
            var asset = UnityEditor.EditorUtility.InstanceIDToObject(instanceID);
            // Here if asset is null and false, and instanceID is NOT zero, it means we have an unloaded asset
            // that still has a valid pptr but we are going to serialize it as null and lose the reference.
            // Only way to fix this would be to backport GlobalObjectId.GetGlobalObjectIdSlow(instanceID).
            writer.WriteEncodedJsonString(UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(asset).ToString());
#endif
            return true;
        }

        static bool TryDeserializeLazyLoadReference<TValue>(SerializedValueView view, ref TValue value, List<DeserializationEvent> events)
        {
            if (!RuntimeTypeInfoCache<TValue>.IsLazyLoadReference)
            {
                return false;
            }

            if (view.Type != TokenType.String)
            {
                return false;
            }

            var json = view.AsStringView().ToString();
            if (json == s_EmptyGlobalObjectId) // Workaround issue where GlobalObjectId.TryParse returns false for empty GlobalObjectId
            {
                return true;
            }

            if (UnityEditor.GlobalObjectId.TryParse(json, out var id))
            {
#if UNITY_2020_1_OR_NEWER
                var instanceID = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToInstanceIDSlow(id);
                PropertyContainer.SetValue(ref value, k_LazyLoadReference_InstanceID, instanceID);
#else
                var asset = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                if ((asset == null || !asset) && !id.assetGUID.Empty())
                {
                    throw new InvalidOperationException($"An error occured while deserializing asset reference GUID=[{id.assetGUID.ToString()}]. Asset is not yet loaded and will result in a null reference.");
                }

                var instanceID = asset.GetInstanceID();
                PropertyContainer.SetValue(ref value, k_LazyLoadReference_InstanceID, instanceID);
#endif
                return true;
            }

            events.Add(new DeserializationEvent(EventType.Error, $"An error occured while deserializing asset reference Value=[{json}]."));
            return false;
        }
#endif
    }
}
