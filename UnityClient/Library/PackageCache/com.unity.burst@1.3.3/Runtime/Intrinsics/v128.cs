using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Unity.Burst.Intrinsics
{
    /// <summary>
    /// Represents a 128-bit SIMD value
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
	[DebuggerTypeProxy(typeof(V128DebugView))]
    public struct v128
    {
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(5)] public byte Byte5;
        [FieldOffset(6)] public byte Byte6;
        [FieldOffset(7)] public byte Byte7;
        [FieldOffset(8)] public byte Byte8;
        [FieldOffset(9)] public byte Byte9;
        [FieldOffset(10)] public byte Byte10;
        [FieldOffset(11)] public byte Byte11;
        [FieldOffset(12)] public byte Byte12;
        [FieldOffset(13)] public byte Byte13;
        [FieldOffset(14)] public byte Byte14;
        [FieldOffset(15)] public byte Byte15;

        [FieldOffset(0)] public sbyte SByte0;
        [FieldOffset(1)] public sbyte SByte1;
        [FieldOffset(2)] public sbyte SByte2;
        [FieldOffset(3)] public sbyte SByte3;
        [FieldOffset(4)] public sbyte SByte4;
        [FieldOffset(5)] public sbyte SByte5;
        [FieldOffset(6)] public sbyte SByte6;
        [FieldOffset(7)] public sbyte SByte7;
        [FieldOffset(8)] public sbyte SByte8;
        [FieldOffset(9)] public sbyte SByte9;
        [FieldOffset(10)] public sbyte SByte10;
        [FieldOffset(11)] public sbyte SByte11;
        [FieldOffset(12)] public sbyte SByte12;
        [FieldOffset(13)] public sbyte SByte13;
        [FieldOffset(14)] public sbyte SByte14;
        [FieldOffset(15)] public sbyte SByte15;

        [FieldOffset(0)] public ushort UShort0;
        [FieldOffset(2)] public ushort UShort1;
        [FieldOffset(4)] public ushort UShort2;
        [FieldOffset(6)] public ushort UShort3;
        [FieldOffset(8)] public ushort UShort4;
        [FieldOffset(10)] public ushort UShort5;
        [FieldOffset(12)] public ushort UShort6;
        [FieldOffset(14)] public ushort UShort7;

        [FieldOffset(0)] public short SShort0;
        [FieldOffset(2)] public short SShort1;
        [FieldOffset(4)] public short SShort2;
        [FieldOffset(6)] public short SShort3;
        [FieldOffset(8)] public short SShort4;
        [FieldOffset(10)] public short SShort5;
        [FieldOffset(12)] public short SShort6;
        [FieldOffset(14)] public short SShort7;

        [FieldOffset(0)] public uint UInt0;
        [FieldOffset(4)] public uint UInt1;
        [FieldOffset(8)] public uint UInt2;
        [FieldOffset(12)] public uint UInt3;

        [FieldOffset(0)] public int SInt0;
        [FieldOffset(4)] public int SInt1;
        [FieldOffset(8)] public int SInt2;
        [FieldOffset(12)] public int SInt3;

        [FieldOffset(0)] public ulong ULong0;
        [FieldOffset(8)] public ulong ULong1;

        [FieldOffset(0)] public long SLong0;
        [FieldOffset(8)] public long SLong1;

        [FieldOffset(0)] public float Float0;
        [FieldOffset(4)] public float Float1;
        [FieldOffset(8)] public float Float2;
        [FieldOffset(12)] public float Float3;

        [FieldOffset(0)] public double Double0;
        [FieldOffset(8)] public double Double1;

        /// <summary>
        /// Splat a single byte across the v128
        /// </summary>
        public v128(byte b)
        {
            this = default(v128);
            Byte0 = Byte1 = Byte2 = Byte3 = Byte4 = Byte5 = Byte6 = Byte7 = Byte8 = Byte9 = Byte10 = Byte11 = Byte12 = Byte13 = Byte14 = Byte15 = b;
        }

        /// <summary>
        /// Initialize the v128 with 16 bytes
        /// </summary>
        public v128(
            byte a, byte b, byte c, byte d,
            byte e, byte f, byte g, byte h,
            byte i, byte j, byte k, byte l,
            byte m, byte n, byte o, byte p)
        {
            this = default(v128);
            Byte0 = a;
            Byte1 = b;
            Byte2 = c;
            Byte3 = d;
            Byte4 = e;
            Byte5 = f;
            Byte6 = g;
            Byte7 = h;
            Byte8 = i;
            Byte9 = j;
            Byte10 = k;
            Byte11 = l;
            Byte12 = m;
            Byte13 = n;
            Byte14 = o;
            Byte15 = p;
        }

        /// <summary>
        /// Splat a single sbyte across the v128
        /// </summary>
        public v128(sbyte b)
        {
            this = default(v128);
            SByte0 = SByte1 = SByte2 = SByte3 = SByte4 = SByte5 = SByte6 = SByte7 = SByte8 = SByte9 = SByte10 = SByte11 = SByte12 = SByte13 = SByte14 = SByte15 = b;
        }

        /// <summary>
        /// Initialize the v128 with 16 sbytes
        /// </summary>
        public v128(
            sbyte a, sbyte b, sbyte c, sbyte d,
            sbyte e, sbyte f, sbyte g, sbyte h,
            sbyte i, sbyte j, sbyte k, sbyte l,
            sbyte m, sbyte n, sbyte o, sbyte p)
        {
            this = default(v128);
            SByte0 = a;
            SByte1 = b;
            SByte2 = c;
            SByte3 = d;
            SByte4 = e;
            SByte5 = f;
            SByte6 = g;
            SByte7 = h;
            SByte8 = i;
            SByte9 = j;
            SByte10 = k;
            SByte11 = l;
            SByte12 = m;
            SByte13 = n;
            SByte14 = o;
            SByte15 = p;
        }

        /// <summary>
        /// Splat a single short across the v128
        /// </summary>
        public v128(short v)
        {
            this = default(v128);
            SShort0 = SShort1 = SShort2 = SShort3 = SShort4 = SShort5 = SShort6 = SShort7 = v;
        }

        /// <summary>
        /// Initialize the v128 with 8 shorts
        /// </summary>
        public v128(short a, short b, short c, short d, short e, short f, short g, short h)
        {
            this = default(v128);
            SShort0 = a;
            SShort1 = b;
            SShort2 = c;
            SShort3 = d;
            SShort4 = e;
            SShort5 = f;
            SShort6 = g;
            SShort7 = h;
        }

        /// <summary>
        /// Splat a single ushort across the v128
        /// </summary>
        public v128(ushort v)
        {
            this = default(v128);
            UShort0 = UShort1 = UShort2 = UShort3 = UShort4 = UShort5 = UShort6 = UShort7 = v;
        }

        /// <summary>
        /// Initialize the v128 with 8 ushorts
        /// </summary>
        public v128(ushort a, ushort b, ushort c, ushort d, ushort e, ushort f, ushort g, ushort h)
        {
            this = default(v128);
            UShort0 = a;
            UShort1 = b;
            UShort2 = c;
            UShort3 = d;
            UShort4 = e;
            UShort5 = f;
            UShort6 = g;
            UShort7 = h;
        }

        /// <summary>
        /// Splat a single int across the v128
        /// </summary>
        public v128(int v)
        {
            this = default(v128);
            SInt0 = SInt1 = SInt2 = SInt3 = v;
        }

        /// <summary>
        /// Initialize the v128 with 4 ints
        /// </summary>
        public v128(int a, int b, int c, int d)
        {
            this = default(v128);
            SInt0 = a;
            SInt1 = b;
            SInt2 = c;
            SInt3 = d;
        }

        /// <summary>
        /// Splat a single uint across the v128
        /// </summary>
        public v128(uint v)
        {
            this = default(v128);
            UInt0 = UInt1 = UInt2 = UInt3 = v;
        }

        /// <summary>
        /// Initialize the v128 with 4 uints
        /// </summary>
        public v128(uint a, uint b, uint c, uint d)
        {
            this = default(v128);
            UInt0 = a;
            UInt1 = b;
            UInt2 = c;
            UInt3 = d;
        }

        /// <summary>
        /// Splat a single float across the v128
        /// </summary>
        public v128(float f)
        {
            this = default(v128);
            Float0 = Float1 = Float2 = Float3 = f;
        }

        /// <summary>
        /// Initialize the v128 with 4 floats
        /// </summary>
        public v128(float a, float b, float c, float d)
        {
            this = default(v128);
            Float0 = a;
            Float1 = b;
            Float2 = c;
            Float3 = d;
        }

        /// <summary>
        /// Splat a single double across the v128
        /// </summary>
        public v128(double f)
        {
            this = default(v128);
            Double0 = Double1 = f;
        }

        /// <summary>
        /// Initialize the v128 with 2 doubles
        /// </summary>
        public v128(double a, double b)
        {
            this = default(v128);
            Double0 = a;
            Double1 = b;
        }

        /// <summary>
        /// Splat a single long across the v128
        /// </summary>
        public v128(long f)
        {
            this = default(v128);
            SLong0 = SLong1 = f;
        }

        /// <summary>
        /// Initialize the v128 with 2 longs
        /// </summary>
        public v128(long a, long b)
        {
            this = default(v128);
            SLong0 = a;
            SLong1 = b;
        }

        /// <summary>
        /// Splat a single ulong across the v128
        /// </summary>
        public v128(ulong f)
        {
            this = default(v128);
            ULong0 = ULong1 = f;
        }

        /// <summary>
        /// Initialize the v128 with 2 ulongs
        /// </summary>
        public v128(ulong a, ulong b)
        {
            this = default(v128);
            ULong0 = a;
            ULong1 = b;
        }
    }
}