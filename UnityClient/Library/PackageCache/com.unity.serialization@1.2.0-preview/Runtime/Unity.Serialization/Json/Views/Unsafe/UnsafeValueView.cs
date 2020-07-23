using System;

namespace Unity.Serialization.Json.Unsafe
{
    readonly struct UnsafeValueView
    {
        readonly UnsafePackedBinaryStream m_Stream;
        readonly int m_TokenIndex;
        
        internal UnsafeValueView(UnsafePackedBinaryStream stream, int tokenIndex)
        {
            m_Stream = stream;
            m_TokenIndex = tokenIndex;
        }

        /// <summary>
        /// The <see cref="TokenType"/> for this view.
        /// </summary>
        public TokenType Type => m_Stream.GetToken(m_TokenIndex).Type;
        
        public bool IsMember()
        {
            var token = m_Stream.GetToken(m_TokenIndex);
            
            if (token.Parent != -1 && token.Type != TokenType.Object)
            {
                return false;
            }

            return token.Type == TokenType.String || token.Type == TokenType.Primitive;
        }
        
        /// <summary>
        /// Reinterprets the value as an string.
        /// </summary>
        /// <returns>The value as a <see cref="SerializedStringView"/>.</returns>
        /// <exception cref="InvalidOperationException">The value could not be reinterpreted.</exception>
        public UnsafeStringView AsStringView() => new UnsafeStringView(m_Stream, m_TokenIndex);

        /// <summary>
        /// Reinterprets the value as an array.
        /// </summary>
        /// <returns>The value as a <see cref="UnsafeArrayView"/>.</returns>
        public UnsafeArrayView AsArrayView() => new UnsafeArrayView(m_Stream, m_TokenIndex);
        
        /// <summary>
        /// Reinterprets the value as an object.
        /// </summary>
        /// <returns>The value as a <see cref="UnsafeObjectView"/>.</returns>
        public UnsafeObjectView AsObjectView() => new UnsafeObjectView(m_Stream, m_TokenIndex);

        /// <summary>
        /// Reinterprets the value as a primitive.
        /// </summary>
        /// <returns>The value as a <see cref="UnsafePrimitiveView"/>.</returns>
        public UnsafePrimitiveView AsPrimitiveView() => new UnsafePrimitiveView(m_Stream, m_TokenIndex);

        /// <summary>
        /// Reinterprets the value as a int.
        /// </summary>
        /// <returns>The value as a int.</returns>
        public int AsInt32() => (int) AsPrimitiveView().AsInt64();
        
        /// <summary>
        /// Reinterprets the value as a long.
        /// </summary>
        /// <returns>The value as a long.</returns>
        public long AsInt64() => AsPrimitiveView().AsInt64();
        
        internal SerializedValueView AsSafe() => new SerializedValueView(m_Stream.AsSafe(), m_Stream.GetHandle(m_TokenIndex));

        public override string ToString() => AsStringView().ToString();
    }
}