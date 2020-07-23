namespace Unity.Serialization.Json.Unsafe
{
    readonly struct UnsafeMemberView
    {
        readonly UnsafePackedBinaryStream m_Stream;
        readonly int m_TokenIndex;

        internal UnsafeMemberView(UnsafePackedBinaryStream stream, int tokenIndex)
        {
            m_Stream = stream;
            m_TokenIndex = tokenIndex;
        }
        
        /// <summary>
        /// Returns a <see cref="UnsafeValueView"/> over the key of this member.
        /// </summary>
        /// <returns>A view over the key.</returns>
        public UnsafeValueView Key() => new UnsafeValueView(m_Stream, m_TokenIndex);

        /// <summary>
        /// Returns a <see cref="UnsafeValueView"/> over the value of this member.
        /// </summary>
        /// <returns>A view over the value.</returns>
        public UnsafeValueView Value() => new UnsafeValueView(m_Stream, m_Stream.GetFirstChildIndex(m_TokenIndex));
    }
}