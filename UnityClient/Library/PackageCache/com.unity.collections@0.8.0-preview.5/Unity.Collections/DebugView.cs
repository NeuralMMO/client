using System;
using System.Collections;

namespace Unity.Collections
{
    internal struct Pair<Key, Value>
    {
        public Key key;
        public Value value;
        public Pair(Key k, Value v)
        {
            key = k;
            value = v;
        }

#if !NET_DOTS
        public override string ToString()
        {
            return $"{key} = {value}";
        }

#endif
    }

    // Tiny does not contains an IList definition (or even ICollection)
#if !NET_DOTS
    internal struct ListPair<Key, Value> where Value : IList
    {
        public Key key;
        public Value value;

        public ListPair(Key k, Value v)
        {
            key = k;
            value = v;
        }

        public override string ToString()
        {
            String result = $"{key} = [";
            for (var v = 0; v < value.Count; ++v)
            {
                result += value[v];
                if (v < value.Count - 1)
                    result += ", ";
            }

            result += "]";
            return result;
        }
    }
#endif
}
