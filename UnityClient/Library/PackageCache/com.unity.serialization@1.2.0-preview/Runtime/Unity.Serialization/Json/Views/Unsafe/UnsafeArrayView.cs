using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Serialization.Json.Unsafe
{
    readonly struct UnsafeArrayView : IEnumerable<UnsafeValueView>
    {
        struct Enumerator : IEnumerator<UnsafeValueView>
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
            
            public UnsafeValueView Current => new UnsafeValueView(m_Stream, m_Position);
            
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

        internal UnsafeArrayView(UnsafePackedBinaryStream stream, int tokenIndex)
        {
            m_Stream = stream;
            m_TokenIndex = tokenIndex;
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

        public IEnumerator<UnsafeValueView> GetEnumerator() => new Enumerator(m_Stream, m_TokenIndex);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}