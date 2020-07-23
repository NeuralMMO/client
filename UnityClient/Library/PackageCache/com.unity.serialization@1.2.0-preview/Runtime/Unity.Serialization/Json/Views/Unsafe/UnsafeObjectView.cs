using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Serialization.Json.Unsafe
{
    readonly struct UnsafeObjectView : ISerializedView, IEnumerable<UnsafeMemberView>
    {
        /// <summary>
        /// Enumerates the elements of <see cref="SerializedObjectView"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<UnsafeMemberView>
        {
            readonly UnsafePackedBinaryStream m_Stream;
            readonly int m_Start;
            readonly int m_End;
            int m_Position;
            
            internal Enumerator(UnsafePackedBinaryStream stream, int index)
            {
                m_Stream = stream;
                m_Start = index;
                m_End = index + m_Stream.GetToken(index).Length;
                m_Position = index;
            }

            public bool MoveNext()
            {
                if (m_Position >= m_End)
                {
                    throw new InvalidOperationException();
                }
                
                var length = m_Stream.GetToken(m_Position).Length;

                if (m_Position == m_Start)
                {
                    if (length == 1)
                    {
                        return false;
                    }
                    
                    m_Position = m_Stream.GetFirstChildIndex(m_Position);
                    return true;
                }
                
                if (m_Position + length < m_End)
                {
                    m_Position += length;
                    return true;
                }
                
                return false;
            }

            object IEnumerator.Current => Current;
            
            public UnsafeMemberView Current => new UnsafeMemberView(m_Stream, m_Position);
            
            public void Reset()
            {
                m_Position = m_Start;
            }

            public void Dispose()
            {
                
            }
        }

        readonly UnsafePackedBinaryStream m_Stream;
        readonly int m_TokenIndex;

        internal UnsafeObjectView(UnsafePackedBinaryStream stream, int tokenIndex)
        {
            m_Stream = stream;
            m_TokenIndex = tokenIndex;
        }

        public UnsafeValueView this[string name]
        {
            get
            {
                if (TryGetValue(name, out var value))
                {
                    return value;
                }
                
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Gets the member associated with the specified key.
        /// </summary>
        /// <param name="name">The key of the member to get.</param>
        /// <param name="member">When this method returns, contains the member associated with the specified key, if the key is found; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetMember(string name, out UnsafeMemberView member)
        {
            foreach (var m in this)
            {
                if (!m.Key().AsStringView().Equals(name))
                    continue;

                member = m;
                return true;
            }

            member = default;
            return false;
        }
        
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(string name, out UnsafeValueView value)
        {
            unsafe
            {
                var view = stackalloc UnsafeView[1];

                view->Stream = m_Stream;
            
                for (int index = m_Stream.GetFirstChildIndex(m_TokenIndex), end = m_TokenIndex + m_Stream.GetToken(m_TokenIndex).Length; index < end;)
                {
                    view->TokenIndex = index;
                    
                    if (((UnsafeStringView*) view)->Equals(name))
                    {
                        view->TokenIndex = m_Stream.GetFirstChildIndex(index);
                        value = *(UnsafeValueView*) view;
                        return true;
                    }
            
                    index += m_Stream.GetToken(index).Length;
                }

            }
            
            value = default;
            return false;
        }
        
        public int Count()
        {
            var count = 0;
            
            for (int index = m_Stream.GetFirstChildIndex(m_TokenIndex), end = m_TokenIndex + m_Stream.GetToken(m_TokenIndex).Length; index < end;)
            {
                index += m_Stream.GetToken(index).Length;
                count++;
            }

            return count;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SerializedObjectView"/>.
        /// </summary>
        /// <returns>A <see cref="SerializedObjectView.Enumerator"/> for the <see cref="SerializedObjectView"/>.</returns>
        public Enumerator GetEnumerator() => new Enumerator(m_Stream, m_TokenIndex);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SerializedObjectView"/>.
        /// </summary>
        /// <returns>A <see cref="SerializedObjectView.Enumerator"/> for the <see cref="SerializedObjectView"/>.</returns>
        IEnumerator<UnsafeMemberView> IEnumerable<UnsafeMemberView>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SerializedObjectView"/>.
        /// </summary>
        /// <returns>A <see cref="SerializedObjectView.Enumerator"/> for the <see cref="SerializedObjectView"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        internal SerializedObjectView AsSafe() => new SerializedObjectView(m_Stream.AsSafe(), m_Stream.GetHandle(m_TokenIndex));

        public UnsafeValueView AsValue() => new UnsafeValueView(m_Stream, m_TokenIndex);
    }
}