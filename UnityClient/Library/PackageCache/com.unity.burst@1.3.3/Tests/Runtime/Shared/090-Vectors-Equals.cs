using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class VectorsEquality
    {
        // Float4
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Float4Equals(ref float4 a, ref float4 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float4Equality(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float4Inequality(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float4EqualityWithFloat(ref float4 a, float b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float4InequalityWithFloat(ref float4 a, float b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Float3
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Float3Equals(ref float3 a, ref float3 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float3Equality(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float3Inequality(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float3EqualityWithFloat(ref float3 a, float b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float3InequalityWithFloat(ref float3 a, float b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Float2
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Float2Equals(ref float2 a, ref float2 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float2Equality(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float2Inequality(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float2EqualityWithFloat(ref float2 a, float b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Float2InequalityWithFloat(ref float2 a, float b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Int4
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Int4Equals(ref int4 a, ref int4 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int4Equality(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int4Inequality(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int4EqualityWithScalar(ref int4 a, int b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int4InequalityWithScalar(ref int4 a, int b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Int3
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Int3Equals(ref int3 a, ref int3 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int3Equality(ref int3 a, ref int3 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int3Inequality(ref int3 a, ref int3 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int3EqualityWithScalar(ref int3 a, int b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int3InequalityWithScalar(ref int3 a, int b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Int2
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Int2Equals(ref int2 a, ref int2 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int2Equality(ref int2 a, ref int2 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int2Inequality(ref int2 a, ref int2 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int2EqualityWithScalar(ref int2 a, int b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Int2InequalityWithScalar(ref int2 a, int b)
        {
            return Vectors.ConvertToInt(a != b);
        }



        // UInt4
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool UInt4Equals(ref uint4 a, ref uint4 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt4Equality(ref uint4 a, ref uint4 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt4Inequality(ref uint4 a, ref uint4 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt4EqualityWithScalar(ref uint4 a, uint b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt4InequalityWithScalar(ref uint4 a, uint b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // UInt3
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool UInt3Equals(ref uint3 a, ref uint3 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt3Equality(ref uint3 a, ref uint3 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt3Inequality(ref uint3 a, ref uint3 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt3EqualityWithScalar(ref uint3 a, uint b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt3InequalityWithScalar(ref uint3 a, uint b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Int2
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool UInt2Equals(ref uint2 a, ref uint2 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt2Equality(ref uint2 a, ref uint2 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt2Inequality(ref uint2 a, ref uint2 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt2EqualityWithScalar(ref uint2 a, uint b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int UInt2InequalityWithScalar(ref uint2 a, uint b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Bool4
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Bool4Equals(ref bool4 a, ref bool4 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool4Equality(ref bool4 a, ref bool4 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool4Inequality(ref bool4 a, ref bool4 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool4EqualityWithScalar(ref bool4 a, bool b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool4InequalityWithScalar(ref bool4 a, bool b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Bool3
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Bool3Equals(ref bool3 a, ref bool3 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool3Equality(ref bool3 a, ref bool3 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool3Inequality(ref bool3 a, ref bool3 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool3EqualityWithScalar(ref bool3 a, bool b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool3InequalityWithScalar(ref bool3 a, bool b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        // Int2
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool Bool2Equals(ref bool2 a, ref bool2 b)
        {
            return a.Equals(b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool2Equality(ref bool2 a, ref bool2 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool2Inequality(ref bool2 a, ref bool2 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool2EqualityWithScalar(ref bool2 a, bool b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Bool2InequalityWithScalar(ref bool2 a, bool b)
        {
            return Vectors.ConvertToInt(a != b);
        }
    }
}