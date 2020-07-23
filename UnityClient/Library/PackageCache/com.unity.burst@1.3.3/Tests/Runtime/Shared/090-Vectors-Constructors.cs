using Burst.Compiler.IL.Tests.Helpers;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal partial class VectorsConstructors
    {
        // ---------------------------------------------------
        // float4
        // ---------------------------------------------------

        [TestCompiler(DataRange.Standard)]
        public static float Float4Int(int a)
        {
            return Vectors.ConvertToFloat(new float4(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float4Float3Float(float x)
        {
            return Vectors.ConvertToFloat(new float4(new float3(x), 5.0f));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float4Float2Float2(float x)
        {
            return Vectors.ConvertToFloat(new float4(new float2(x), new float2(5.0f)));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float44Floats(float a)
        {
            return Vectors.ConvertToFloat(new float4(1.0f, 2.0f, 3.0f + a, 4.0f));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float4Float(float a)
        {
            return Vectors.ConvertToFloat(new float4(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float4Int4(ref int4 a)
        {
            return Vectors.ConvertToFloat((float4) new float4(a).x);
        }

        // ---------------------------------------------------
        // float3
        // ---------------------------------------------------

        [TestCompiler(DataRange.Standard)]
        public static float Float3Int(int a)
        {
            return Vectors.ConvertToFloat(new float3(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float33Floats(float a)
        {
            return Vectors.ConvertToFloat(new float3(1.0f, 2.0f, 3.0f + a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float3Float(float a)
        {
            return Vectors.ConvertToFloat(new float3(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float3Float2Float(float a)
        {
            return Vectors.ConvertToFloat(new float3(new float2(a), 5.0f));
        }

        // ---------------------------------------------------
        // float2
        // ---------------------------------------------------

        [TestCompiler(DataRange.Standard)]
        public static float Float2Int(int a)
        {
            return Vectors.ConvertToFloat(new float2(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float22Floats(float a)
        {
            return Vectors.ConvertToFloat(new float2(1.0f, 3.0f + a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float2Float(float a)
        {
            return Vectors.ConvertToFloat(new float2(a));
        }

        // ---------------------------------------------------
        // int4
        // ---------------------------------------------------

        [TestCompiler(DataRange.Standard)]
        public static int Int4Int(int a)
        {
            return Vectors.ConvertToInt(new int4(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int Int4Int3Int(int x)
        {
            return Vectors.ConvertToInt(new int4(new int3(x), 5));
        }

        [TestCompiler(DataRange.Standard)]
        public static int Int44Ints(int a)
        {
            return Vectors.ConvertToInt(new int4(1, 2, 3 + a, 4));
        }

        // ---------------------------------------------------
        // int3
        // ---------------------------------------------------

        [TestCompiler(DataRange.Standard)]
        public static int Int3Int(int a)
        {
            return Vectors.ConvertToInt(new int3(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int Int33Ints(int a)
        {
            return Vectors.ConvertToInt(new int3(1, 2, 3 + a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int Int3Int2Int(int a)
        {
            return Vectors.ConvertToInt(new int3(new int2(a), 5));
        }

        // ---------------------------------------------------
        // int2
        // ---------------------------------------------------

        [TestCompiler(DataRange.Standard)]
        public static int Int2Int(int a)
        {
            return Vectors.ConvertToInt(new int2(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int Int22Ints(int a)
        {
            return Vectors.ConvertToInt(new int2(1, 3 + a));
        }


        // ---------------------------------------------------
        // bool4
        // ---------------------------------------------------

        [TestCompiler(true)]
        [TestCompiler(false)]
        public static int Bool4(bool a)
        {
            return Vectors.ConvertToInt(new bool4(a));
        }

        [TestCompiler(true)]
        [TestCompiler(false)]
        public static int Bool4Bool3(bool x)
        {
            return Vectors.ConvertToInt(new bool4(new bool3(x), true));
        }

        [TestCompiler(false, false, false, false)]
        [TestCompiler(true, false, false, false)]
        [TestCompiler(false, true, false, false)]
        [TestCompiler(false, false, true, false)]
        [TestCompiler(false, false, false, true)]
        public static int Bool44Bools(bool a, bool b, bool c, bool d)
        {
            return Vectors.ConvertToInt(new bool4(a, b, c, d));
        }

        // ---------------------------------------------------
        // bool3
        // ---------------------------------------------------

        [TestCompiler(true)]
        [TestCompiler(false)]
        public static int Bool3(bool a)
        {
            return Vectors.ConvertToInt(new bool3(a));
        }

        [TestCompiler(true)]
        [TestCompiler(false)]
        public static int Bool3Bool2(bool a)
        {
            return Vectors.ConvertToInt(new bool3(new bool2(a), true));
        }

        [TestCompiler(false, false, false)]
        [TestCompiler(true, false, false)]
        [TestCompiler(false, true, false)]
        [TestCompiler(false, false, true)]
        public static int Bool33Bools(bool a, bool b, bool c)
        {
            return Vectors.ConvertToInt(new bool3(a, b, c));
        }

        // ---------------------------------------------------
        // bool2
        // ---------------------------------------------------

        [TestCompiler(true)]
        [TestCompiler(false)]
        public static int Bool2(bool a)
        {
            return Vectors.ConvertToInt(new bool2(a));
        }

        [TestCompiler(true, false)]
        [TestCompiler(false, false)]
        [TestCompiler(false, true)]
        public static int Bool22Ints(bool a, bool b)
        {
            return Vectors.ConvertToInt(new bool2(a, b));
        }
    }
}