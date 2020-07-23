using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Mathematics;

namespace Unity.Collections
{
    internal struct Bitwise
    {
        internal static int AlignDown(int value, int alignPow2)
        {
            return value & ~(alignPow2 - 1);
        }

        internal static int AlignUp(int value, int alignPow2)
        {
            return AlignDown(value + alignPow2 - 1, alignPow2);
        }

        internal static int FromBool(bool value)
        {
            return value ? 1 : 0;
        }

        // 32-bit uint

        internal static uint ExtractBits(uint input, int pos, uint mask)
        {
            var tmp0 = input >> pos;
            return tmp0 & mask;
        }

        internal static uint ReplaceBits(uint input, int pos, uint mask, uint value)
        {
            var tmp0 = (value & mask) << pos;
            var tmp1 = input & ~(mask << pos);
            return tmp0 | tmp1;
        }

        internal static uint SetBits(uint input, int pos, uint mask, bool value)
        {
            return ReplaceBits(input, pos, mask, (uint)-FromBool(value));
        }

        // 64-bit ulong

        internal static ulong ExtractBits(ulong input, int pos, ulong mask)
        {
            var tmp0 = input >> pos;
            return tmp0 & mask;
        }

        internal static ulong ReplaceBits(ulong input, int pos, ulong mask, ulong value)
        {
            var tmp0 = (value & mask) << pos;
            var tmp1 = input & ~(mask << pos);
            return tmp0 | tmp1;
        }

        internal static ulong SetBits(ulong input, int pos, ulong mask, bool value)
        {
            return ReplaceBits(input, pos, mask, (ulong)-(long)FromBool(value));
        }
    }

    /// <summary>
    /// Fixed size 32-bit array of bits.
    /// </summary>
    [DebuggerTypeProxy(typeof(BitField32DebugView))]
    public struct BitField32
    {
        /// <summary>
        /// </summary>
        public uint Value;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialValue">Initial value of bit field. Default is 0.</param>
        public BitField32(uint initialValue = 0u)
        {
            Value = initialValue;
        }

        /// <summary>
        /// Clear all bits to 0.
        /// </summary>
        public void Clear()
        {
            Value = 0u;
        }

        /// <summary>
        /// Set single bit to desired boolean value.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <param name="value">Value of bits to set.</param>
        public void SetBits(int pos, bool value)
        {
            CheckArgs(pos, 1);
            Value = Bitwise.SetBits(Value, pos, 1, value);
        }

        /// <summary>
        /// Set bits to desired boolean value.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <param name="value">Value of bits to set.</param>
        /// <param name="numBits">Number of bits to set (must be 1-32).</param>
        public void SetBits(int pos, bool value, int numBits)
        {
            CheckArgs(pos, numBits);
            var mask = 0xffffffffu >> (32 - numBits);
            Value = Bitwise.SetBits(Value, pos, mask, value);
        }

        /// <summary>
        /// Returns all bits in range as uint.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <param name="numBits">Number of bits to get (must be 1-32).</param>
        /// <returns>Returns requested range of bits.</returns>
        public uint GetBits(int pos, int numBits = 1)
        {
            CheckArgs(pos, numBits);
            var mask = 0xffffffffu >> (32 - numBits);
            return Bitwise.ExtractBits(Value, pos, mask);
        }

        /// <summary>
        /// Returns true is bit at position is set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <returns>Returns true if bit is set.</returns>
        public bool IsSet(int pos)
        {
            return 0 != GetBits(pos);
        }

        /// <summary>
        /// Returns true if none of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <param name="numBits">Number of bits to test (must be 1-32).</param>
        /// <returns>Returns true if none of bits are set.</returns>
        public bool TestNone(int pos, int numBits = 1)
        {
            return 0u == GetBits(pos, numBits);
        }

        /// <summary>
        /// Returns true if any of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <param name="numBits">Number of bits to test (must be 1-32).</param>
        /// <returns>Returns true if at least one bit is set.</returns>
        public bool TestAny(int pos, int numBits = 1)
        {
            return 0u != GetBits(pos, numBits);
        }

        /// <summary>
        /// Returns true if all of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <param name="numBits">Number of bits to test (must be 1-32).</param>
        /// <returns>Returns true if all bits are set.</returns>
        public bool TestAll(int pos, int numBits = 1)
        {
            CheckArgs(pos, numBits);
            var mask = 0xffffffffu >> (32 - numBits);
            return mask == Bitwise.ExtractBits(Value, pos, mask);
        }

        /// <summary>
        /// Calculate number of set bits.
        /// </summary>
        /// <returns>Number of set bits.</returns>
        public int CountBits()
        {
            return math.countbits(Value);
        }

        /// <summary>
        /// Calculate number of leading zeros.
        /// </summary>
        /// <returns>Number of leading zeros</returns>
        public int CountLeadingZeros()
        {
            return math.lzcnt(Value);
        }

        /// <summary>
        /// Calculate number of trailing zeros.
        /// </summary>
        /// <returns>Number of trailing zeros</returns>
        public int CountTrailingZeros()
        {
            return math.tzcnt(Value);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckArgs(int pos, int numBits)
        {
            if (pos > 31
                ||  numBits == 0
                ||  numBits > 32
                ||  pos + numBits > 32)
            {
                throw new ArgumentException($"BitField32 invalid arguments: pos {pos} (must be 0-31), numBits {numBits} (must be 1-32).");
            }
        }
    }

    sealed class BitField32DebugView
    {
        BitField32 BitField;

        public BitField32DebugView(BitField32 bitfield)
        {
            BitField = bitfield;
        }

        public bool[] Bits
        {
            get
            {
                var array = new bool[32];
                for (int i = 0; i < 32; ++i)
                {
                    array[i] = BitField.IsSet(i);
                }
                return array;
            }
        }
    }

    /// <summary>
    /// Fixed size 64-bit array of bits.
    /// </summary>
    [DebuggerTypeProxy(typeof(BitField64DebugView))]
    public struct BitField64
    {
        public ulong Value;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialValue">Initial value of bit field. Default is 0.</param>
        public BitField64(ulong initialValue = 0ul)
        {
            Value = initialValue;
        }

        /// <summary>
        /// Clear all bits to 0.
        /// </summary>
        public void Clear()
        {
            Value = 0ul;
        }

        /// <summary>
        /// Set bits to desired boolean value.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-63).</param>
        /// <param name="value">Value of bits to set.</param>
        /// <param name="numBits">Number of bits to set (must be 1-64).</param>
        public void SetBits(int pos, bool value, int numBits = 1)
        {
            CheckArgs(pos, numBits);
            var mask = 0xfffffffffffffffful >> (64 - numBits);
            Value = Bitwise.SetBits(Value, pos, mask, value);
        }

        /// <summary>
        /// Returns all bits in range as uint.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-63).</param>
        /// <param name="numBits">Number of bits to set (must be 1-64).</param>
        /// <returns>Returns requested range of bits.</returns>
        public ulong GetBits(int pos, int numBits = 1)
        {
            CheckArgs(pos, numBits);
            var mask = 0xfffffffffffffffful >> (64 - numBits);
            return Bitwise.ExtractBits(Value, pos, mask);
        }

        /// <summary>
        /// Returns true is bit at position is set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-31).</param>
        /// <returns>Returns true if bit is set.</returns>
        public bool IsSet(int pos)
        {
            return 0ul != GetBits(pos);
        }

        /// <summary>
        /// Returns true if none of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-63).</param>
        /// <param name="numBits">Number of bits to set (must be 1-64).</param>
        /// <returns>Returns true if none of bits are set.</returns>
        public bool TestNone(int pos, int numBits = 1)
        {
            return 0ul == GetBits(pos, numBits);
        }

        /// <summary>
        /// Returns true if any of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-63).</param>
        /// <param name="numBits">Number of bits to set (must be 1-64).</param>
        /// <returns>Returns true if at least one bit is set.</returns>
        public bool TestAny(int pos, int numBits = 1)
        {
            return 0ul != GetBits(pos, numBits);
        }

        /// <summary>
        /// Returns true if all of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bitfield (must be 0-63).</param>
        /// <param name="numBits">Number of bits to set (must be 1-64).</param>
        /// <returns>Returns true if all bits are set.</returns>
        public bool TestAll(int pos, int numBits = 1)
        {
            CheckArgs(pos, numBits);
            var mask = 0xfffffffffffffffful >> (64 - numBits);
            return mask == Bitwise.ExtractBits(Value, pos, mask);
        }

        /// <summary>
        /// Calculate number of set bits.
        /// </summary>
        /// <returns>Number of set bits.</returns>
        public int CountBits()
        {
            return math.countbits(Value);
        }

        /// <summary>
        /// Calculate number of leading zeros.
        /// </summary>
        /// <returns>Number of leading zeros</returns>
        public int CountLeadingZeros()
        {
            return math.lzcnt(Value);
        }

        /// <summary>
        /// Calculate number of trailing zeros.
        /// </summary>
        /// <returns>Number of trailing zeros</returns>
        public int CountTrailingZeros()
        {
            return math.tzcnt(Value);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckArgs(int pos, int numBits)
        {
            if (pos > 63
                ||  numBits == 0
                ||  numBits > 64
                ||  pos + numBits > 64)
            {
                throw new ArgumentException($"BitField32 invalid arguments: pos {pos} (must be 0-63), numBits {numBits} (must be 1-64).");
            }
        }
    }

    sealed class BitField64DebugView
    {
        BitField64 Data;

        public BitField64DebugView(BitField64 data)
        {
            Data = data;
        }

        public bool[] Bits
        {
            get
            {
                var array = new bool[64];
                for (int i = 0; i < 64; ++i)
                {
                    array[i] = Data.IsSet(i);
                }
                return array;
            }
        }
    }
}
