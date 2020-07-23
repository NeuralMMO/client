using System;
using System.Diagnostics;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections
{
    /// <summary>
    /// </summary>
    public enum FormatError
    {
        /// <summary>
        /// </summary>
        None,

        /// <summary>
        /// </summary>
        Overflow,
    }

    /// <summary>
    /// </summary>
    public enum ParseError
    {
        /// <summary>
        /// </summary>
        None,

        /// <summary>
        /// </summary>
        Syntax,

        /// <summary>
        /// </summary>
        Overflow,

        /// <summary>
        /// </summary>
        Underflow,
    }

    /// <summary>
    /// </summary>
    public enum CopyError
    {
        /// <summary>
        /// </summary>
        None,

        /// <summary>
        /// </summary>
        Truncation,
    }

    /// <summary>
    /// </summary>
    public enum ConversionError
    {
        /// <summary>
        /// </summary>
        None,

        /// <summary>
        /// </summary>
        Overflow,

        /// <summary>
        /// </summary>
        Encoding,

        /// <summary>
        /// </summary>
        CodePoint,
    }

    /// <summary>
    /// </summary>
    public unsafe struct Unicode
    {
        /// <summary>
        /// </summary>
        public struct Rune
        {
            public int value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ucs"></param>
        /// <returns></returns>
        public static bool IsValidCodePoint(int ucs)
        {
            if (ucs > 0x10FFFF) // maximum valid code point
                return false;
            if (ucs >= 0xD800 && ucs <= 0xDFFF) // surrogate pair
                return false;
            if (ucs < 0) // negative?
                return false;
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool NotTrailer(byte b)
        {
            return (b & 0xC0) != 0x80;
        }

        /// <summary>
        ///
        /// </summary>
        static public Rune ReplacementCharacter => new Rune {value = 0xFFFD};

        /// <summary>
        ///
        /// </summary>
        static public Rune BadRune => new Rune {value = 0};

        /// <summary>
        ///
        /// </summary>
        /// <param name="rune"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static ConversionError Utf8ToUcs(out Rune rune, byte* buffer, ref int offset, int capacity)
        {
            int code = 0;
            rune = ReplacementCharacter;
            if (offset + 1 > capacity)
            {
                return ConversionError.Overflow;
            }

            if ((buffer[offset] & 0b10000000) == 0b00000000) // if high bit is 0, 1 byte
            {
                rune.value = buffer[offset + 0];
                offset += 1;
                return ConversionError.None;
            }

            if ((buffer[offset] & 0b11100000) == 0b11000000) // if high 3 bits are 110, 2 bytes
            {
                if (offset + 2 > capacity)
                {
                    offset += 1;
                    return ConversionError.Overflow;
                }
                code =              (buffer[offset + 0] & 0b00011111);
                code = (code << 6) |  (buffer[offset + 1] & 0b00111111);
                if (code < (1 << 7) || NotTrailer(buffer[offset + 1]))
                {
                    offset += 1;
                    return ConversionError.Encoding;
                }
                rune.value = code;
                offset += 2;
                return ConversionError.None;
            }

            if ((buffer[offset] & 0b11110000) == 0b11100000) // if high 4 bits are 1110, 3 bytes
            {
                if (offset + 3 > capacity)
                {
                    offset += 1;
                    return ConversionError.Overflow;
                }
                code =              (buffer[offset + 0] & 0b00001111);
                code = (code << 6) |  (buffer[offset + 1] & 0b00111111);
                code = (code << 6) |  (buffer[offset + 2] & 0b00111111);
                if (code < (1 << 11) || !IsValidCodePoint(code) || NotTrailer(buffer[offset + 1]) || NotTrailer(buffer[offset + 2]))
                {
                    offset += 1;
                    return ConversionError.Encoding;
                }
                rune.value = code;
                offset += 3;
                return ConversionError.None;
            }

            if ((buffer[offset] & 0b11111000) == 0b11110000) // if high 5 bits are 11110, 4 bytes
            {
                if (offset + 4 > capacity)
                {
                    offset += 1;
                    return ConversionError.Overflow;
                }
                code =              (buffer[offset + 0] & 0b00000111);
                code = (code << 6) |  (buffer[offset + 1] & 0b00111111);
                code = (code << 6) |  (buffer[offset + 2] & 0b00111111);
                code = (code << 6) |  (buffer[offset + 3] & 0b00111111);
                if (code < (1 << 16) || !IsValidCodePoint(code) || NotTrailer(buffer[offset + 1]) || NotTrailer(buffer[offset + 2]) || NotTrailer(buffer[offset + 3]))
                {
                    offset += 1;
                    return ConversionError.Encoding;
                }
                rune.value = code;
                offset += 4;
                return ConversionError.None;
            }

            offset += 1;
            return ConversionError.Encoding;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rune"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public static ConversionError Utf16ToUcs(out Rune rune, char* buffer, ref int offset, int capacity)
        {
            int code = 0;
            rune = ReplacementCharacter;
            if (offset + 1 > capacity)
                return ConversionError.Overflow;
            if (buffer[offset] >= 0xD800 && buffer[offset] <= 0xDBFF)
            {
                if (offset + 2 > capacity)
                {
                    offset += 1;
                    return ConversionError.Overflow;
                }
                code =               (buffer[offset + 0] & 0x03FF);
                char next = buffer[offset + 1];
                if (next < 0xDC00 || next > 0xDFFF)
                {
                    offset += 1;
                    return ConversionError.Encoding;
                }
                code = (code << 10) | (buffer[offset + 1] & 0x03FF);
                code += 0x10000;
                rune.value = code;
                offset += 2;
                return ConversionError.None;
            }
            rune.value = buffer[offset + 0];
            offset += 1;
            return ConversionError.None;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="capacity"></param>
        /// <param name="rune"></param>
        /// <returns></returns>
        public static ConversionError UcsToUtf8(byte* buffer, ref int offset, int capacity, Rune rune)
        {
            if (!IsValidCodePoint(rune.value))
            {
                return ConversionError.CodePoint;
            }

            if (offset + 1 > capacity)
            {
                return ConversionError.Overflow;
            }

            if (rune.value <= 0x7F)
            {
                buffer[offset++] = (byte)rune.value;
                return ConversionError.None;
            }

            if (rune.value <= 0x7FF)
            {
                if (offset + 2 > capacity)
                {
                    return ConversionError.Overflow;
                }

                buffer[offset++] = (byte)(0xC0 | (rune.value >> 6));
                buffer[offset++] = (byte)(0x80 | ((rune.value >> 0) & 0x3F));
                return ConversionError.None;
            }

            if (rune.value <= 0xFFFF)
            {
                if (offset + 3 > capacity)
                {
                    return ConversionError.Overflow;
                }

                buffer[offset++] = (byte)(0xE0 | (rune.value >> 12));
                buffer[offset++] = (byte)(0x80 | ((rune.value >> 6) & 0x3F));
                buffer[offset++] = (byte)(0x80 | ((rune.value >> 0) & 0x3F));
                return ConversionError.None;
            }

            if (rune.value <= 0x1FFFFF)
            {
                if (offset + 4 > capacity)
                {
                    return ConversionError.Overflow;
                }

                buffer[offset++] = (byte)(0xF0 | (rune.value >> 18));
                buffer[offset++] = (byte)(0x80 | ((rune.value >> 12) & 0x3F));
                buffer[offset++] = (byte)(0x80 | ((rune.value >> 6) & 0x3F));
                buffer[offset++] = (byte)(0x80 | ((rune.value >> 0) & 0x3F));
                return ConversionError.None;
            }

            return ConversionError.Encoding;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="capacity"></param>
        /// <param name="rune"></param>
        /// <returns></returns>
        public static ConversionError UcsToUtf16(char* buffer, ref int offset, int capacity, Rune rune)
        {
            if (!IsValidCodePoint(rune.value))
            {
                return ConversionError.CodePoint;
            }

            if (offset + 1 > capacity)
            {
                return ConversionError.Overflow;
            }

            if (rune.value >= 0x10000)
            {
                if (offset + 2 > capacity)
                {
                    return ConversionError.Overflow;
                }

                int code = rune.value - 0x10000;
                if (code >= (1 << 20))
                {
                    return ConversionError.Encoding;
                }

                buffer[offset++] = (char)(0xD800 | (code >> 10));
                buffer[offset++] = (char)(0xDC00 | (code & 0x3FF));
                return ConversionError.None;
            }

            buffer[offset++] = (char)rune.value;
            return ConversionError.None;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="utf16_buffer"></param>
        /// <param name="utf16_length"></param>
        /// <param name="utf8_buffer"></param>
        /// <param name="utf8_length"></param>
        /// <param name="utf8_capacity"></param>
        /// <returns></returns>
        public static ConversionError Utf16ToUtf8(char* utf16_buffer, int utf16_length, byte* utf8_buffer, out int utf8_length, int utf8_capacity)
        {
            utf8_length = 0;
            for (var utf16_offset = 0; utf16_offset < utf16_length;)
            {
                Utf16ToUcs(out var ucs, utf16_buffer, ref utf16_offset, utf16_length);
                if (UcsToUtf8(utf8_buffer, ref utf8_length, utf8_capacity, ucs) == ConversionError.Overflow)
                    return ConversionError.Overflow;
            }
            return ConversionError.None;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="src_buffer"></param>
        /// <param name="src_length"></param>
        /// <param name="dest_buffer"></param>
        /// <param name="dest_length"></param>
        /// <param name="dest_capacity"></param>
        /// <returns></returns>
        public static ConversionError Utf8ToUtf8(byte* src_buffer, int src_length, byte* dest_buffer, out int dest_length, int dest_capacity)
        {
            if (dest_capacity >= src_length)
            {
                UnsafeUtility.MemCpy(dest_buffer, src_buffer, src_length);
                dest_length = src_length;
                return ConversionError.None;
            }
            // TODO even in this case, it's possible to MemCpy all but the last 3 bytes that fit, and then by looking at only
            // TODO the high bits of the last 3 bytes that fit, decide how many of the 3 to append. but that requires a
            // TODO little UNICODE presence of mind that nobody has today.
            dest_length = 0;
            for (var src_offset = 0; src_offset < src_length;)
            {
                Utf8ToUcs(out var ucs, src_buffer, ref src_offset, src_length);
                if (UcsToUtf8(dest_buffer, ref dest_length, dest_capacity, ucs) == ConversionError.Overflow)
                    return ConversionError.Overflow;
            }
            return ConversionError.None;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="utf8_buffer"></param>
        /// <param name="utf8_length"></param>
        /// <param name="utf16_buffer"></param>
        /// <param name="utf16_length"></param>
        /// <param name="utf16_capacity"></param>
        /// <returns></returns>
        public static ConversionError Utf8ToUtf16(byte* utf8_buffer, int utf8_length, char* utf16_buffer, out int utf16_length, int utf16_capacity)
        {
            utf16_length = 0;
            for (var utf8_offset = 0; utf8_offset < utf8_length;)
            {
                Utf8ToUcs(out var ucs, utf8_buffer, ref utf8_offset, utf8_length);
                if (UcsToUtf16(utf16_buffer, ref utf16_length, utf16_capacity, ucs) == ConversionError.Overflow)
                    return ConversionError.Overflow;
            }
            return ConversionError.None;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// A "NativeStringView" does not manage its own memory - it expects some other object to manage its memory
    /// on its behalf.
    /// </remarks>
    public struct NativeStringView
    {
        unsafe char* pointer;
        int length;

        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        /// <param name="l"></param>
        public unsafe NativeStringView(char* p, int l)
        {
            pointer = p;
            length = l;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public unsafe char this[int index]
        {
            get => UnsafeUtility.ReadArrayElement<char>(pointer, index);
            set => UnsafeUtility.WriteArrayElement<char>(pointer, index, value);
        }

        /// <summary>
        ///
        /// </summary>
        public int Length => length;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            unsafe
            {
#if !UNITY_DOTSPLAYER
                return new string(pointer, 0, length);
#else
                var c = new char[Length];
                for (var i = 0; i < Length; ++i)
                    c[i] = pointer[i];
                return new String(c, 0, Length);
#endif
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unsafe
            {
                return (int)CollectionHelper.Hash(pointer, Length * sizeof(char));
            }
        }
    }

    sealed class WordStorageDebugView
    {
        WordStorage m_wordStorage;

        public WordStorageDebugView(WordStorage wordStorage)
        {
            m_wordStorage = wordStorage;
        }

        public NativeStringView[] Table
        {
            get
            {
                var table = new NativeStringView[m_wordStorage.Entries];
                for (var i = 0; i < m_wordStorage.Entries; ++i)
                    table[i] = m_wordStorage.GetNativeStringView(i);
                return table;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    [DebuggerTypeProxy(typeof(WordStorageDebugView))]
    public class WordStorage : IDisposable
    {
        NativeArray<ushort> buffer; // all the UTF-16 encoded bytes in one place
        NativeArray<int> offset; // one offset for each text in "buffer"
        NativeArray<ushort> length; // one length for each text in "buffer"
        NativeMultiHashMap<int, int> hash; // from string hash to table entry
        int chars; // bytes in buffer allocated so far
        int entries; // number of strings allocated so far
        static WordStorage _Instance;

        /// <summary>
        ///
        /// </summary>
        public static WordStorage Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WordStorage();
                return _Instance;
            }
            set { _Instance = value; }
        }

        const int kMaxEntries = 16 << 10;
        const int kMaxChars = kMaxEntries * 128;

        /// <summary>
        ///
        /// </summary>
        public const int kMaxCharsPerEntry = 4096;

        public int Entries => entries;

        void Initialize()
        {
            buffer = new NativeArray<ushort>(kMaxChars, Allocator.Persistent);
            offset = new NativeArray<int>(kMaxEntries, Allocator.Persistent);
            length = new NativeArray<ushort>(kMaxEntries, Allocator.Persistent);
            hash = new NativeMultiHashMap<int, int>(kMaxEntries, Allocator.Persistent);
            chars = 0;
            entries = 0;
            GetOrCreateIndex(new NativeStringView()); // make sure that Index=0 means empty string

#if UNITY_EDITOR
            // Free storage on domain unload, which happens when iterating on the Entities module a lot.
            AppDomain.CurrentDomain.DomainUnload += (_, __) => { this.Dispose(); };
#endif
        }

        WordStorage()
        {
            Initialize();
        }

        /// <summary>
        ///
        /// </summary>
        public static void Setup()
        {
            if (Instance.buffer.Length > 0)
                Instance.Dispose();
            Instance.Initialize();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public unsafe NativeStringView GetNativeStringView(int index)
        {
            Assert.IsTrue(index < entries);
            var o = offset[index];
            var l = length[index];
            Assert.IsTrue(l <= kMaxCharsPerEntry);
            return new NativeStringView((char*)buffer.GetUnsafePtr() + o, l);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="h"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        public int GetIndex(int h, NativeStringView temp)
        {
            Assert.IsTrue(temp.Length <= kMaxCharsPerEntry); // about one printed page of text
            int itemIndex;
            NativeMultiHashMapIterator<int> iter;
            if (hash.TryGetFirstValue(h, out itemIndex, out iter))
            {
                var l = length[itemIndex];
                Assert.IsTrue(l <= kMaxCharsPerEntry);
                if (l == temp.Length)
                {
                    var o = offset[itemIndex];
                    int matches;
                    for (matches = 0; matches < l; ++matches)
                        if (temp[matches] != buffer[o + matches])
                            break;
                    if (matches == temp.Length)
                        return itemIndex;
                }
            }
            while (hash.TryGetNextValue(out itemIndex, ref iter)) ;
            return -1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(NativeStringView value)
        {
            int h = value.GetHashCode();
            return GetIndex(h, value) != -1;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public unsafe bool Contains(string value)
        {
            fixed(char *c = value)
            return Contains(new NativeStringView(c, value.Length));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int GetOrCreateIndex(NativeStringView value)
        {
            int h = value.GetHashCode();
            var itemIndex = GetIndex(h, value);
            if (itemIndex != -1)
                return itemIndex;
            Assert.IsTrue(entries < kMaxEntries);
            Assert.IsTrue(chars + value.Length <= kMaxChars);
            var o = chars;
            var l = (ushort)value.Length;
            for (var i = 0; i < l; ++i)
                buffer[chars++] = value[i];
            offset[entries] = o;
            length[entries] = l;
            hash.Add(h, entries);
            return entries++;
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (buffer.IsCreated)
            {
                buffer.Dispose();
                offset.Dispose();
                length.Dispose();
                hash.Dispose();
            }
        }
    }


    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// A "Words" is an integer that refers to 4,096 or fewer chars of UTF-16 text in a global storage blob.
    /// Each should refer to *at most* about one printed page of text.
    ///
    /// If you need more text, consider using one Words struct for each printed page's worth.
    ///
    /// Each Words instance that you create is stored in a single, internally-managed WordStorage object,
    /// which can hold up to 16,384 Words entries. Once added, the entries in WordStorage cannot be modified
    /// or removed.
    /// </remarks>
    public struct Words
    {
        int Index;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public NativeStringView ToNativeStringView()
        {
            return WordStorage.Instance.GetNativeStringView(Index);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return WordStorage.Instance.GetNativeStringView(Index).ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public unsafe void SetString(string value)
        {
            fixed(char *c = value)
            Index = WordStorage.Instance.GetOrCreateIndex(new NativeStringView(c, value.Length));
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// A "NumberedWords" is a "Words", plus possibly a string of leading zeroes, followed by
    /// possibly a positive integer.
    /// The zeroes and integer aren't stored centrally as a string, they're stored as an int.
    /// Therefore, 1,000,000 items with names from FooBarBazBifBoo000000 to FooBarBazBifBoo999999
    /// Will cost 8MB + a single copy of "FooBarBazBifBoo", instead of ~48MB.
    /// They say that this is a thing, too.
    /// </remarks>
    public struct NumberedWords
    {
        int Index;
        int Suffix;

        const int kPositiveNumericSuffixShift = 0;
        const int kPositiveNumericSuffixBits = 29;
        const int kMaxPositiveNumericSuffix = (1 << kPositiveNumericSuffixBits) - 1;
        const int kPositiveNumericSuffixMask = (1 << kPositiveNumericSuffixBits) - 1;

        const int kLeadingZeroesShift = 29;
        const int kLeadingZeroesBits = 3;
        const int kMaxLeadingZeroes = (1 << kLeadingZeroesBits) - 1;
        const int kLeadingZeroesMask = (1 << kLeadingZeroesBits) - 1;

        int LeadingZeroes
        {
            get => (Suffix >> kLeadingZeroesShift) & kLeadingZeroesMask;
            set
            {
                Suffix &= ~(kLeadingZeroesMask << kLeadingZeroesShift);
                Suffix |= (value & kLeadingZeroesMask) << kLeadingZeroesShift;
            }
        }

        int PositiveNumericSuffix
        {
            get => (Suffix >> kPositiveNumericSuffixShift) & kPositiveNumericSuffixMask;
            set
            {
                Suffix &= ~(kPositiveNumericSuffixMask << kPositiveNumericSuffixShift);
                Suffix |= (value & kPositiveNumericSuffixMask) << kPositiveNumericSuffixShift;
            }
        }

        bool HasPositiveNumericSuffix => PositiveNumericSuffix != 0;

        string NewString(char c, int count)
        {
            char[] temp = new char[count];
            for (var i = 0; i < count; ++i)
                temp[i] = c;
            return new string(temp, 0, count);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string temp = WordStorage.Instance.GetNativeStringView(Index).ToString();
            var leadingZeroes = LeadingZeroes;
            if (leadingZeroes > 0)
                temp += NewString('0', leadingZeroes);
            if (HasPositiveNumericSuffix)
                temp += PositiveNumericSuffix;
            return temp;
        }

        bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        string Substring(string s, int offset, int count)
        {
            char[] c = new char[count];
            for (var i = 0; i < count; ++i)
                c[i] = s[offset + i];
            return new string(c, 0, count);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public unsafe void SetString(string value)
        {
            int beginningOfDigits = value.Length;

            // as long as there are digits at the end,
            // look back for more digits.

            while (beginningOfDigits > 0 && IsDigit(value[beginningOfDigits - 1]))
                --beginningOfDigits;

            // as long as the first digit is a zero, it's not the beginning of the positive integer - it's a leading zero.

            var beginningOfPositiveNumericSuffix = beginningOfDigits;
            while (beginningOfPositiveNumericSuffix < value.Length && value[beginningOfPositiveNumericSuffix] == '0')
                ++beginningOfPositiveNumericSuffix;

            // now we know where the leading zeroes begin, and then where the positive integer begins after them.
            // but if there are too many leading zeroes to encode, the excess ones become part of the string.

            var leadingZeroes = beginningOfPositiveNumericSuffix - beginningOfDigits;
            if (leadingZeroes > kMaxLeadingZeroes)
            {
                var excessLeadingZeroes = leadingZeroes - kMaxLeadingZeroes;
                beginningOfDigits += excessLeadingZeroes;
                leadingZeroes -= excessLeadingZeroes;
            }

            // if there is a positive integer after the zeroes, here's where we compute it and store it for later.

            PositiveNumericSuffix = 0;
            {
                int number = 0;
                for (var i = beginningOfPositiveNumericSuffix; i < value.Length; ++i)
                {
                    number *= 10;
                    number += value[i] - '0';
                }

                // an intrepid user may attempt to encode a positive integer with 20 digits or something.
                // they are rewarded with a string that is encoded wholesale without any optimizations.

                if (number <= kMaxPositiveNumericSuffix)
                    PositiveNumericSuffix = number;
                else
                {
                    beginningOfDigits = value.Length;
                    leadingZeroes = 0; // and your dog Toto, too.
                }
            }

            // set the leading zero count in the Suffix member.

            LeadingZeroes = leadingZeroes;

            // truncate the string, if there were digits at the end that we encoded.

            if (beginningOfDigits != value.Length)
                value = Substring(value, 0, beginningOfDigits);

            // finally, set the string to its index in the global string blob thing.

            fixed(char *c = value)
            Index = WordStorage.Instance.GetOrCreateIndex(new NativeStringView(c, value.Length));
        }
    }
}
