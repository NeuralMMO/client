#if BURST_INTERNAL || UNITY_BURST_EXPERIMENTAL_NEON_INTRINSICS
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Unity.Burst.Intrinsics
{
    /// <summary>
    /// Represents a 64-bit SIMD value (Arm only)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
	[DebuggerTypeProxy(typeof(V64DebugView))]
    public struct v64
    {
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(5)] public byte Byte5;
        [FieldOffset(6)] public byte Byte6;
        [FieldOffset(7)] public byte Byte7;

        [FieldOffset(0)] public sbyte SByte0;
        [FieldOffset(1)] public sbyte SByte1;
        [FieldOffset(2)] public sbyte SByte2;
        [FieldOffset(3)] public sbyte SByte3;
        [FieldOffset(4)] public sbyte SByte4;
        [FieldOffset(5)] public sbyte SByte5;
        [FieldOffset(6)] public sbyte SByte6;
        [FieldOffset(7)] public sbyte SByte7;

        [FieldOffset(0)] public ushort UShort0;
        [FieldOffset(2)] public ushort UShort1;
        [FieldOffset(4)] public ushort UShort2;
        [FieldOffset(6)] public ushort UShort3;

        [FieldOffset(0)] public short SShort0;
        [FieldOffset(2)] public short SShort1;
        [FieldOffset(4)] public short SShort2;
        [FieldOffset(6)] public short SShort3;

        [FieldOffset(0)] public uint UInt0;
        [FieldOffset(4)] public uint UInt1;

        [FieldOffset(0)] public int SInt0;
        [FieldOffset(4)] public int SInt1;

        [FieldOffset(0)] public ulong ULong0;

        [FieldOffset(0)] public long SLong0;

        [FieldOffset(0)] public float Float0;
        [FieldOffset(4)] public float Float1;

        [FieldOffset(0)] public double Double0;

        /// <summary>
        /// Splat a single byte across the v64
        /// </summary>
        public v64(byte b)
        {
            this = default(v64);
            Byte0 = Byte1 = Byte2 = Byte3 = Byte4 = Byte5 = Byte6 = Byte7 = b;
        }

        /// <summary>
        /// Initialize the v64 with 8 bytes
        /// </summary>
        public v64(
            byte a, byte b, byte c, byte d,
            byte e, byte f, byte g, byte h)
        {
            this = default(v64);
            Byte0 = a;
            Byte1 = b;
            Byte2 = c;
            Byte3 = d;
            Byte4 = e;
            Byte5 = f;
            Byte6 = g;
            Byte7 = h;
        }

        /// <summary>
        /// Splat a single sbyte across the v64
        /// </summary>
        public v64(sbyte b)
        {
            this = default(v64);
            SByte0 = SByte1 = SByte2 = SByte3 = SByte4 = SByte5 = SByte6 = SByte7 = b;
        }

        /// <summary>
        /// Initialize the v64 with 8 sbytes
        /// </summary>
        public v64(
            sbyte a, sbyte b, sbyte c, sbyte d,
            sbyte e, sbyte f, sbyte g, sbyte h)
        {
            this = default(v64);
            SByte0 = a;
            SByte1 = b;
            SByte2 = c;
            SByte3 = d;
            SByte4 = e;
            SByte5 = f;
            SByte6 = g;
            SByte7 = h;
        }

        /// <summary>
        /// Splat a single short across the v64
        /// </summary>
        public v64(short v)
        {
            this = default(v64);
            SShort0 = SShort1 = SShort2 = SShort3 = v;
        }

        /// <summary>
        /// Initialize the v64 with 4 shorts
        /// </summary>
        public v64(short a, short b, short c, short d)
        {
            this = default(v64);
            SShort0 = a;
            SShort1 = b;
            SShort2 = c;
            SShort3 = d;
        }

        /// <summary>
        /// Splat a single ushort across the v64
        /// </summary>
        public v64(ushort v)
        {
            this = default(v64);
            UShort0 = UShort1 = UShort2 = UShort3 = v;
        }

        /// <summary>
        /// Initialize the v64 with 4 ushorts
        /// </summary>
        public v64(ushort a, ushort b, ushort c, ushort d)
        {
            this = default(v64);
            UShort0 = a;
            UShort1 = b;
            UShort2 = c;
            UShort3 = d;
        }

        /// <summary>
        /// Splat a single int across the v64
        /// </summary>
        public v64(int v)
        {
            this = default(v64);
            SInt0 = SInt1 = v;
        }

        /// <summary>
        /// Initialize the v64 with 2 ints
        /// </summary>
        public v64(int a, int b)
        {
            this = default(v64);
            SInt0 = a;
            SInt1 = b;
        }

        /// <summary>
        /// Splat a single uint across the v64
        /// </summary>
        public v64(uint v)
        {
            this = default(v64);
            UInt0 = UInt1 = v;
        }

        /// <summary>
        /// Initialize the v64 with 2 uints
        /// </summary>
        public v64(uint a, uint b)
        {
            this = default(v64);
            UInt0 = a;
            UInt1 = b;
        }

        /// <summary>
        /// Splat a single float across the v64
        /// </summary>
        public v64(float f)
        {
            this = default(v64);
            Float0 = Float1 = f;
        }

        /// <summary>
        /// Initialize the v64 with 2 floats
        /// </summary>
        public v64(float a, float b)
        {
            this = default(v64);
            Float0 = a;
            Float1 = b;
        }

        /// Initialize the v64 with a double
        /// </summary>
        public v64(double a)
        {
            this = default(v64);
            Double0 = a;
        }

        /// <summary>
        /// Initialize the v64 with a long
        /// </summary>
        public v64(long a)
        {
            this = default(v64);
            SLong0 = a;
        }

        /// <summary>
        /// Initialize the v64 with a ulong
        /// </summary>
        public v64(ulong a)
        {
            this = default(v64);
            ULong0 = a;
        }
    }
}
#endif // BURST_INTERNAL || UNITY_BURST_EXPERIMENTAL_NEON_INTRINSICS
