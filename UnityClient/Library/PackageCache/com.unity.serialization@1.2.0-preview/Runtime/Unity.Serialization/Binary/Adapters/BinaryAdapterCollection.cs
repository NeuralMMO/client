using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Binary.Adapters
{
    unsafe struct BinaryAdapterCollection
    {
        public BinaryAdapter InternalAdapter;
        public List<IBinaryAdapter> GlobalAdapters;
        public List<IBinaryAdapter> UserDefinedAdapters;

        public bool TrySerialize<TValue>(UnsafeAppendBuffer* stream, ref TValue value)
        {
            if (null != UserDefinedAdapters && UserDefinedAdapters.Count > 0)
            {
                foreach (var adapter in UserDefinedAdapters)
                {
                    if (TrySerialize(adapter, stream, value))
                    {
                        return true;
                    }
                }
            }

            if (null != GlobalAdapters && GlobalAdapters.Count > 0)
            {
                foreach (var adapter in GlobalAdapters)
                {
                    if (TrySerialize(adapter, stream, value))
                    {
                        return true;
                    }
                }
            }

            return TrySerialize(InternalAdapter, stream, value);
        }

        static bool TrySerialize<TValue>(IBinaryAdapter adapter, UnsafeAppendBuffer* buffer, TValue value)
        {
            if (adapter is IBinaryAdapter<TValue> typed)
            {
                typed.Serialize(buffer, value);
                return true;
            }
            
            if (adapter is Contravariant.IBinaryAdapter<TValue> typedContravariant)
            {
                typedContravariant.Serialize(buffer, value);
                return true;
            }

            return false;
        }

        public bool TryDeserialize<TValue>(UnsafeAppendBuffer.Reader* stream, ref TValue value)
        {
            if (null != UserDefinedAdapters && UserDefinedAdapters.Count > 0)
            {
                foreach (var adapter in UserDefinedAdapters)
                {
                    if (TryDeserialize(adapter, stream, ref value))
                    {
                        return true;
                    }
                }
            }

            if (null != GlobalAdapters && GlobalAdapters.Count > 0)
            {
                foreach (var adapter in GlobalAdapters)
                {
                    if (TryDeserialize(adapter, stream, ref value))
                    {
                        return true;
                    }
                }
            }

            return TryDeserialize(InternalAdapter, stream, ref value);
        }

        static bool TryDeserialize<TValue>(IBinaryAdapter adapter, UnsafeAppendBuffer.Reader* buffer, ref TValue value)
        {
            if (adapter is IBinaryAdapter<TValue> typed)
            {
                value = typed.Deserialize(buffer);
                return true;
            }
            
            if (adapter is Contravariant.IBinaryAdapter<TValue> typedContravariant)
            {
                value = (TValue) typedContravariant.Deserialize(buffer);
                return true;
            }

            return false;
        }
    }
}