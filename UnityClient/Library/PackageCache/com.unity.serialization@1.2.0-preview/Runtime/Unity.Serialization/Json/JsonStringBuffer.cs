using System;
using System.Globalization;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// Represents a writable string of characters.
    /// </summary>
    public unsafe struct JsonStringBuffer : IDisposable
    {
        const int k_MinimumCapacity = 32;
        
        struct Data
        {
            public char* Buffer;
            public int Capacity;
            public int Length;
        }
        
        readonly Allocator m_Label;
        Data* m_Data;
        
        /// <summary>
        /// Initializes a new instance of <see cref="JsonStringBuffer"/>.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity to use for the internal buffer.</param>
        /// <param name="label">The allocator label to use.</param>
        public JsonStringBuffer(int initialCapacity, Allocator label)
        {
            m_Label = label;
            m_Data = (Data*) UnsafeUtility.Malloc(sizeof(Data), UnsafeUtility.AlignOf<Data>(), label);
            m_Data->Buffer = null;
            m_Data->Capacity = 0;
            m_Data->Length = 0;
            
            SetCapacity(initialCapacity);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (m_Label != Allocator.Invalid)
                UnsafeUtility.Free(m_Data->Buffer, m_Label);
            
            UnsafeUtility.Free(m_Data, m_Label);
            m_Data = null;
        }
        
        /// <summary>
        /// Writes the string representation of a specified 32-bit signed integer to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(int value)
        {
            Write(FixedString.Format("{0}", value));
        }
        
        /// <summary>
        /// Writes the string representation of a specified 32-bit unsigned integer to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(uint value)
        {
            Write(FixedString.Format("{0}", value));
        }
        
        /// <summary>
        /// Writes the string representation of a specified 64-bit signed integer to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(long value)
        {
            Write(FixedString.Format("{0}", value));
        }
        
        /// <summary>
        /// Writes the string representation of a specified 64-bit unsigned integer to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(ulong value)
        {
            Write(value.ToString(CultureInfo.InvariantCulture));
        }
        
        /// <summary>
        /// Writes the string representation of a specified 32-bit floating-point number to the buffer. This method will allocate.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(float value)
        {
            Write(FixedString.Format("{0}", value));
        }
        
        /// <summary>
        /// Writes the string representation of a specified 64-bit floating-point number to the buffer. This method will allocate.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(double value)
        {
            Write(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Writes the string representation of a specified unicode character to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(char value)
        {
            EnsureCapacity(1);
            m_Data->Buffer[m_Data->Length++] = value;
        }
        
        /// <summary>
        /// Writes a specified number of copies of the string representation of a unicode character to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="repeatCount">The number of times to write <paramref name="value"/>.</param>
        public void Write(char value, int repeatCount)
        {
            EnsureCapacity(repeatCount);
            for (var i=0; i<repeatCount; i++)
                m_Data->Buffer[m_Data->Length++] = value;
        }
        
        /// <summary>
        /// Writes a copy of the specified string to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void Write(string value)
        {
            if (value != null)
            {
                fixed (char* ptr = value)
                {
                    Write(ptr, value.Length);
                }
            }
            else
            {
                WriteNull();
            }
        }

        /// <summary>
        /// Writes a copy of the specified string to the buffer.
        /// </summary>
        /// <param name="buffer">A pointer to the string.</param>
        /// <param name="length">The number of characters to write.</param>
        public void Write(char* buffer, int length)
        {
            EnsureCapacity(length);
            UnsafeUtility.MemCpy(m_Data->Buffer + m_Data->Length, buffer, length * sizeof(char));
            m_Data->Length += length;
        }
        
        /// <summary>
        /// Writes a copy of the specified string to the buffer with surrounding quotes and escape characters.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteEncodedJsonString(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            Write('"');

            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\':
                        Write("\\\\");
                        break;
                    case '\"':
                        Write("\\\"");
                        break;
                    case '\t':
                        Write("\\t");
                        break;
                    case '\r':
                        Write("\\r");
                        break;
                    case '\n':
                        Write("\\n");
                        break;
                    case '\b':
                        Write("\\b");
                        break;
                    default:
                        Write(c);
                        break;
                }
            }

            Write('"');
        }
        
        /// <summary>
        /// Writes the specified unicode character to the buffer with surrounding quotes and escape characters.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteEncodedJsonString(char value)
        {
            Write('"');
            
            switch (value)
            {
                case '\\':
                    Write("\\\\");
                    break;
                case '\"':
                    Write("\\\"");
                    break;
                case '\t':
                    Write("\\t");
                    break;
                case '\r':
                    Write("\\r");
                    break;
                case '\n':
                    Write("\\n");
                    break;
                case '\b':
                    Write("\\b");
                    break;
                case '\0':
                    Write("\\0");
                    break;
                default:
                    Write(value);
                    break;
            }
            
            Write('"');
        }
        
        /// <summary>
        /// Writes a copy of the specified <see cref="FixedString128"/> to the buffer.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void Write(FixedString128 value)
        {
            var capacity = value.UTF8LengthInBytes;
            var utf16_buffer = stackalloc char[capacity];

            fixed (FixedListByte128* c = &value.AsFixedList)
            {
                Unicode.Utf8ToUtf16((byte*) c + sizeof(ushort), value.UTF8LengthInBytes, utf16_buffer, out var utf16_length, capacity);
                Write(utf16_buffer, utf16_length);
            }
        }

        void WriteNull()
        {
            var chars = stackalloc char[4] {'n', 'u', 'l', 'l'};
            Write(chars, 4);
        }
        
        void SetCapacity(int targetCapacity)
        {
            if (targetCapacity <= m_Data->Capacity)
            {
                return;
            }

            targetCapacity = targetCapacity < k_MinimumCapacity ? k_MinimumCapacity : math.ceilpow2(targetCapacity);
            
            var buffer = (char*) UnsafeUtility.Malloc(targetCapacity * sizeof(char), UnsafeUtility.AlignOf<char>(), m_Label);
            
            if (m_Data->Buffer != null)
            {
                UnsafeUtility.MemCpy(buffer, m_Data->Buffer, m_Data->Length * sizeof(char));
                UnsafeUtility.Free(m_Data->Buffer, m_Label);
            }

            m_Data->Buffer = buffer;
            m_Data->Capacity = targetCapacity;
        }

        void EnsureCapacity(int sizeOf)
        {
            SetCapacity(m_Data->Length + sizeOf);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return new string(m_Data->Buffer, 0, m_Data->Length);
        }
    }
}