using System;

namespace Unity.Properties.UI.Internal
{
    struct TypePairKey : IEquatable<TypePairKey>
    {
        readonly Type m_First;
        readonly Type m_Second;

        public static TypePairKey Make<TFirst, TSecond>() => new TypePairKey(typeof(TFirst), typeof(TSecond));

        public TypePairKey(Type first, Type second)
        {
            m_First = first;
            m_Second = second;
        }

        public override bool Equals(object obj)
        {
            return obj != null && Equals((TypePairKey)obj);
        }

        public bool Equals(TypePairKey key)
        {
            return m_First == key.m_First && m_Second == key.m_Second;
        }

        public override int GetHashCode()
        {
            return m_First.GetHashCode() ^ 17 * m_Second.GetHashCode();
        }
    }
}