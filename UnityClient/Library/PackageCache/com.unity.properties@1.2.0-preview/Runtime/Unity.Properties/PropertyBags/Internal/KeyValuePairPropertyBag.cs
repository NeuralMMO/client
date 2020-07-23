using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    class KeyValuePairPropertyBag<TKey, TValue> : PropertyBag<KeyValuePair<TKey, TValue>>,
        IPropertyNameable<KeyValuePair<TKey, TValue>>
    {
        static readonly DelegateProperty<KeyValuePair<TKey, TValue>, TKey> KeyProperty =
            new DelegateProperty<KeyValuePair<TKey, TValue>, TKey>(
                nameof(KeyValuePair<TKey, TValue>.Key),
                (ref KeyValuePair<TKey, TValue> container) => container.Key,
                null);

        static readonly DelegateProperty<KeyValuePair<TKey, TValue>, TValue> ValueProperty =
            new DelegateProperty<KeyValuePair<TKey, TValue>, TValue>(
                nameof(KeyValuePair<TKey, TValue>.Value),
                (ref KeyValuePair<TKey, TValue> container) => container.Value,
                null);

        internal override IEnumerable<IProperty<KeyValuePair<TKey, TValue>>> GetProperties(
            ref KeyValuePair<TKey, TValue> container)
        {
            return GetProperties();
        }

        static IEnumerable<IProperty<KeyValuePair<TKey, TValue>>> GetProperties()
        {
            yield return KeyProperty;
            yield return ValueProperty;
        }

        public bool TryGetProperty(ref KeyValuePair<TKey, TValue> container, string name,
            out IProperty<KeyValuePair<TKey, TValue>> property)
        {
            if (name == nameof(KeyValuePair<TKey, TValue>.Key))
            {
                property = KeyProperty;
                return true;
            }

            if (name == nameof(KeyValuePair<TKey, TValue>.Value))
            {
                property = ValueProperty;
                return true;
            }

            property = default;
            return false;
        }
    }
}