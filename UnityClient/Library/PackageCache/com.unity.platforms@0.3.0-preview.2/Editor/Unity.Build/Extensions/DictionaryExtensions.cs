using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Build
{
    internal static class DictionaryExtensions
    {
        public static int RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, bool> predicate)
        {
            var count = 0;
            foreach (var key in dictionary.Keys.ToArray().Where(key => predicate(key, dictionary[key])))
            {
                dictionary.Remove(key);
                count++;
            }
            return count;
        }
    }
}
