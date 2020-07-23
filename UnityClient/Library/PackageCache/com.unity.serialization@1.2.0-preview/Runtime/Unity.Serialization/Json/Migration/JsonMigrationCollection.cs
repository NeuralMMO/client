using System;
using System.Collections.Generic;
using Unity.Serialization.Json.Unsafe;
using UnityEngine;

namespace Unity.Serialization.Json.Adapters
{
    struct JsonMigrationCollection
    {
        public List<IJsonMigration> Global;
        public List<IJsonMigration> UserDefined;
        
        public bool TryGetSerializedVersion<TValue>(out int version)
        {
            var migration = GetMigrationForType<TValue>(out version);

            if (null == migration)
                return false;

            if (version > 0) 
                return true;
            
            Debug.LogError($"An error occured while serializing Type=[{typeof(TValue)}] using IJsonMigration=[{migration.GetType()}]. Serialized version must be greater than 0.");
            return false;
        }

        public bool TryMigrate<TValue>(UnsafeObjectView view, out TValue value, JsonPropertyReader reader, List<DeserializationEvent> events)
        {
            var migration = GetMigrationForType<TValue>(out var version);

            if (null == migration)
            {
                value = default;
                return false;
            }

            var serializedVersion = 0;
            
            if (view.TryGetValue(JsonPropertyVisitor.k_SerializedVersionKey, out var serializedVersionView))
            {
                if (serializedVersionView.Type != TokenType.Primitive)
                {
                    events.Add(new DeserializationEvent(EventType.Exception, new Exception($"An error occured while deserializing Type=[{typeof(TValue)}]. Property=[{JsonPropertyVisitor.k_SerializedVersionKey}] is expected to be an int value.")));
                    value = default;
                    return false;
                }

                serializedVersion = serializedVersionView.AsInt32();

                if (version == serializedVersion)
                {
                    value = default;
                    return false;
                }
            }

            var context = new JsonMigrationContext(serializedVersion, view.AsSafe(), typeof(TValue), reader);

            switch (migration)
            {
                case IJsonMigration<TValue> typed:
                    value = typed.Migrate(context);
                    break;
                case Contravariant.IJsonMigration<TValue> typedContravariant:
                    value = (TValue) typedContravariant.Migrate(context);
                    break;
                default:
                    throw new Exception("An internal error has occured.");
            }
            
            return true;
        }

        IJsonMigration GetMigrationForType<TValue>(out int version)
        {
            if (null != UserDefined && UserDefined.Count > 0)
            {
                foreach (var adapter in UserDefined)
                {
                    if (adapter is IJsonMigration<TValue> typed)
                    {
                        version = typed.Version;
                        return typed; 
                    }
                    
                    if (adapter is Contravariant.IJsonMigration<TValue> typedContravariant)
                    {
                        version = typedContravariant.Version;
                        return typedContravariant; 
                    }
                }
            }
            
            if (null != Global && Global.Count > 0)
            {
                foreach (var adapter in Global)
                {
                    if (adapter is IJsonMigration<TValue> typed)
                    {
                        version = typed.Version;
                        return typed; 
                    }
                    
                    if (adapter is Contravariant.IJsonMigration<TValue> typedContravariant)
                    {
                        version = typedContravariant.Version;
                        return typedContravariant; 
                    }
                }
            }

            version = 0;
            return null;
        }
    }
}