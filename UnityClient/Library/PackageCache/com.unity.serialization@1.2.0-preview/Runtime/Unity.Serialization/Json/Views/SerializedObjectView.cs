using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Serialization.Json.Unsafe;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// A view on top of the <see cref="PackedBinaryStream"/> that represents a set of key-values.
    /// </summary>
    public readonly struct SerializedObjectView : ISerializedView, IEnumerable<SerializedMemberView>
    {
        /// <summary>
        /// Enumerates the elements of <see cref="SerializedObjectView"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<SerializedMemberView>
        {
            readonly PackedBinaryStream m_Stream;
            readonly Handle m_Start;
            Handle m_Current;

            internal Enumerator(PackedBinaryStream stream, Handle start)
            {
                m_Stream = stream;
                m_Start = start;
                m_Current = new Handle {Index = -1, Version = -1};
            }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="SerializedObjectView"/>.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                var startIndex = m_Stream.GetTokenIndex(m_Start);
                var startToken = m_Stream.GetToken(startIndex);

                if (startToken.Length == 1)
                {
                    return false;
                }

                if (m_Current.Index == -1)
                {
                    m_Current = m_Stream.GetFirstChild(m_Start);
                    return true;
                }

                if (!m_Stream.IsValid(m_Current))
                {
                    return false;
                }

                var currentIndex = m_Stream.GetTokenIndex(m_Current);
                var currentToken = m_Stream.GetToken(currentIndex);

                if (currentIndex + currentToken.Length >= startIndex + startToken.Length)
                {
                    return false;
                }

                m_Current = m_Stream.GetHandle(currentIndex + currentToken.Length);
                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                m_Current = new Handle {Index = -1, Version = -1};
            }

            /// <summary>
            /// The element in the <see cref="SerializedObjectView"/> at the current position of the enumerator.
            /// </summary>
            /// <exception cref="InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception>
            public SerializedMemberView Current
            {
                get
                {
                    if (m_Current.Index < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return new SerializedMemberView(m_Stream, m_Current);
                }
            }

            object IEnumerator.Current => Current;

            /// <summary>
            /// Releases all resources used by the <see cref="SerializedObjectView.Enumerator" />.
            /// </summary>
            public void Dispose()
            {
            }
        }

        readonly PackedBinaryStream m_Stream;
        readonly Handle m_Handle;

        internal SerializedObjectView(PackedBinaryStream stream, Handle handle)
        {
            m_Stream = stream;
            m_Handle = handle;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <exception cref="KeyNotFoundException">The key does not exist in the collection.</exception>
        public SerializedValueView this[string name]
        {
            get
            {
                if (TryGetValue(name, out var value))
                {
                    return value;
                }

                throw new KeyNotFoundException($"The Key=[\"{name}\"] could not be found in the SerializedObjectView.");
            }
        }

        /// <summary>
        /// Gets the member associated with the specified key.
        /// </summary>
        /// <param name="name">The key of the member to get.</param>
        /// <param name="member">When this method returns, contains the member associated with the specified key, if the key is found; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetMember(string name, out SerializedMemberView member)
        {
            foreach (var m in this)
            {
                if (!m.Name().Equals(name))
                {
                    continue;
                }

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
        public bool TryGetValue(string name, out SerializedValueView value)
        {
            foreach (var m in this)
            {
                if (!m.Name().Equals(name))
                {
                    continue;
                }

                value = m.Value();
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key as a <see cref="string"/>.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key and type, if the key is found and the type matches; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key and type; otherwise, false.</returns>
        public bool TryGetValueAsString(string name, out string value)
        {
            value = default;
            
            if (!TryGetValue(name, out var view))
                return false;

            if (view.Type != TokenType.String)
                return false;

            value = view.ToString();
            return true;
        }

        /// <summary>
        /// Gets the value associated with the specified key as a <see cref="long"/>.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key and type, if the key is found and the type matches; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key and type; otherwise, false.</returns>
        public bool TryGetValueAsInt64(string name, out long value)
        {
            value = default;
            
            if (!TryGetValue(name, out var view))
                return false;

            if (view.Type != TokenType.Primitive)
                return false;

            var primitive = view.AsPrimitiveView();
            value = primitive.AsInt64();
            return true;
        }
        
        /// <summary>
        /// Gets the value associated with the specified key as a <see cref="ulong"/>.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key and type, if the key is found and the type matches; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key and type; otherwise, false.</returns>
        public bool TryGetValueAsUInt64(string name, out ulong value)
        {
            value = default;
            
            if (!TryGetValue(name, out var view))
                return false;

            if (view.Type != TokenType.Primitive)
                return false;

            var primitive = view.AsPrimitiveView();
            value = primitive.AsUInt64();
            return true;
        }
        
        /// <summary>
        /// Gets the value associated with the specified key as a <see cref="float"/>.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key and type, if the key is found and the type matches; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key and type; otherwise, false.</returns>
        public bool TryGetValueAsFloat(string name, out float value)
        {
            value = default;
            
            if (!TryGetValue(name, out var view))
                return false;

            if (view.Type != TokenType.Primitive)
                return false;

            var primitive = view.AsPrimitiveView();
            value = primitive.AsFloat();
            return true;
        }
        
        /// <summary>
        /// Gets the value associated with the specified key as a <see cref="double"/>.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key and type, if the key is found and the type matches; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key and type; otherwise, false.</returns>
        public bool TryGetValueAsDouble(string name, out double value)
        {
            value = default;
            
            if (!TryGetValue(name, out var view))
                return false;

            if (view.Type != TokenType.Primitive)
                return false;

            var primitive = view.AsPrimitiveView();
            value = primitive.AsDouble();
            return true;
        }
        
        /// <summary>
        /// Gets the value associated with the specified key as a <see cref="bool"/>.
        /// </summary>
        /// <param name="name">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key and type, if the key is found and the type matches; otherwise, the default value.</param>
        /// <returns>true if the <see cref="SerializedObjectView"/> contains an element with the specified key and type; otherwise, false.</returns>
        public bool TryGetValueAsBoolean(string name, out bool value)
        {
            value = default;
            
            if (!TryGetValue(name, out var view))
                return false;

            if (view.Type != TokenType.Primitive)
                return false;

            var primitive = view.AsPrimitiveView();
            value = primitive.AsBoolean();
            return true;
        }
        
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SerializedObjectView"/>.
        /// </summary>
        /// <returns>A <see cref="SerializedObjectView.Enumerator"/> for the <see cref="SerializedObjectView"/>.</returns>
        public Enumerator GetEnumerator() => new Enumerator(m_Stream, m_Handle);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SerializedObjectView"/>.
        /// </summary>
        /// <returns>A <see cref="SerializedObjectView.Enumerator"/> for the <see cref="SerializedObjectView"/>.</returns>
        IEnumerator<SerializedMemberView> IEnumerable<SerializedMemberView>.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SerializedObjectView"/>.
        /// </summary>
        /// <returns>A <see cref="SerializedObjectView.Enumerator"/> for the <see cref="SerializedObjectView"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        internal UnsafeObjectView AsUnsafe() => new UnsafeObjectView(m_Stream.AsUnsafe(), m_Stream.GetTokenIndex(m_Handle));
        
        public static implicit operator SerializedValueView(SerializedObjectView view) => new SerializedValueView(view.m_Stream, view.m_Handle);
    }
}