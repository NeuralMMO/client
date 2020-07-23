using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class VectorsMaths
    {
        [TestCompiler()]
        public static ulong HalfToFloatAndDouble()
        {
            return math.asuint(new half {value = 0x0000})
                   + math.asulong(new half {value = 0x1000});
        }

        // ---------------------------------------------------------
        // asfloat
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float AsFloatInt4(ref int4 a)
        {
            return Vectors.ConvertToFloat(math.asfloat(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float AsFloatInt3(ref int3 a)
        {
            return Vectors.ConvertToFloat(math.asfloat(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float AsFloatInt2(ref int2 a)
        {
            return Vectors.ConvertToFloat(math.asfloat(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static float AsFloatUInt4(ref uint4 a)
        {
            return Vectors.ConvertToFloat(math.asfloat(a));
        }

        // ---------------------------------------------------------
        // asint
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static int AsIntFloat4(ref float4 a)
        {
            return Vectors.ConvertToInt(math.asint(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int AsIntFloat3(ref float3 a)
        {
            return Vectors.ConvertToInt(math.asint(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int AsIntFloat2(ref float2 a)
        {
            return Vectors.ConvertToInt(math.asint(a));
        }

        // ---------------------------------------------------------
        // asuint
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static int AsUIntFloat4(ref float4 a)
        {
            return Vectors.ConvertToInt(math.asuint(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int AsUIntFloat3(ref float3 a)
        {
            return Vectors.ConvertToInt(math.asuint(a));
        }

        [TestCompiler(DataRange.Standard)]
        public static int AsUIntFloat2(ref float2 a)
        {
            return Vectors.ConvertToInt(math.asuint(a));
        }

        // ---------------------------------------------------------
        // compress
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static unsafe int CompressInt4(ref int4 value, ref bool4 mask)
        {
            var temp = default(TestCompressInt4);

            var ptr = &temp.Value0;
            var count = math.compress(ptr, 0, value, mask);

            int result = 0;
            for (int i = 0; i < count; i++)
            {
                result = result * 397 + ptr[i];
            }

            return result;
        }

        // ---------------------------------------------------------
        // count_bits
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static int CountBitsInt(int value)
        {
            return math.countbits(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsInt2(ref int2 value)
        {
            return Vectors.ConvertToInt(math.countbits(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsInt3(ref int3 value)
        {
            return Vectors.ConvertToInt(math.countbits(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsInt4(ref int4 value)
        {
            return Vectors.ConvertToInt(math.countbits(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsUInt(uint value)
        {
            return math.countbits(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsUInt2(ref uint2 value)
        {
            return Vectors.ConvertToInt(math.countbits(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsUInt3(ref uint3 value)
        {
            return Vectors.ConvertToInt(math.countbits(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsUInt4(ref uint4 value)
        {
            return Vectors.ConvertToInt(math.countbits(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsLong(long value)
        {
            return math.countbits(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int CountBitsULong(ulong value)
        {
            return math.countbits(value);
        }

        // ---------------------------------------------------------
        // lzcnt
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static int LzCntInt(int value)
        {
            return math.lzcnt(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntInt2(ref int2 value)
        {
            return Vectors.ConvertToInt(math.lzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntInt3(ref int3 value)
        {
            return Vectors.ConvertToInt(math.lzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntInt4(ref int4 value)
        {
            return Vectors.ConvertToInt(math.lzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntUInt(uint value)
        {
            return math.lzcnt(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntUInt2(ref uint2 value)
        {
            return Vectors.ConvertToInt(math.lzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntUInt3(ref uint3 value)
        {
            return Vectors.ConvertToInt(math.lzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntUInt4(ref uint4 value)
        {
            return Vectors.ConvertToInt(math.lzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntLong(long value)
        {
            return math.lzcnt(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int LzCntULong(ulong value)
        {
            return math.lzcnt(value);
        }

        // ---------------------------------------------------------
        // tzcnt
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static int TzCntInt(int value)
        {
            return math.tzcnt(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntInt2(ref int2 value)
        {
            return Vectors.ConvertToInt(math.tzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntInt3(ref int3 value)
        {
            return Vectors.ConvertToInt(math.tzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntInt4(ref int4 value)
        {
            return Vectors.ConvertToInt(math.tzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntUInt(uint value)
        {
            return math.tzcnt(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntUInt2(ref uint2 value)
        {
            return Vectors.ConvertToInt(math.tzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntUInt3(ref uint3 value)
        {
            return Vectors.ConvertToInt(math.tzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntUInt4(ref uint4 value)
        {
            return Vectors.ConvertToInt(math.tzcnt(value));
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntLong(long value)
        {
            return math.tzcnt(value);
        }

        [TestCompiler(DataRange.Standard)]
        public static int TzCntULong(ulong value)
        {
            return math.tzcnt(value);
        }

        // ---------------------------------------------------------
        // min
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Min4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.min(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Min3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.min(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Min2(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.min(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Min(float a, float b)
        {
            return math.min(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MinInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(math.min(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MinInt3(ref int3 a, ref int3 b)
        {
            return Vectors.ConvertToInt(math.min(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MinInt2(ref int2 a, ref int2 b)
        {
            return Vectors.ConvertToInt(math.min(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MinInt(int a, int b)
        {
            return math.min(a, b);
        }

        // ---------------------------------------------------------
        // max
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Max4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.max(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Max3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.max(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Max2(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.max(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Max(float a, float b)
        {
            return math.max(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MaxInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(math.max(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MaxInt3(ref int3 a, ref int3 b)
        {
            return Vectors.ConvertToInt(math.max(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MaxInt2(ref int2 a, ref int2 b)
        {
            return Vectors.ConvertToInt(math.max(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float MaxInt(int a, int b)
        {
            return math.max(a, b);
        }

        // ---------------------------------------------------------
        // lerp
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive|DataRange.Zero)]
        public static float Lerp4(ref float4 a, ref float4 b, float w)
        {
            return Vectors.ConvertToFloat(math.lerp(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Lerp3(ref float3 a, ref float3 b, float w)
        {
            return Vectors.ConvertToFloat(math.lerp(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Lerp2(ref float2 a, ref float2 b, float w)
        {
            return Vectors.ConvertToFloat(math.lerp(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Lerp(float a, float b, float w)
        {
            return math.lerp(a, b, w);
        }


        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Lerp4_4(ref float4 a, ref float4 b, ref float4 w)
        {
            return Vectors.ConvertToFloat(math.lerp(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Lerp3_3(ref float3 a, ref float3 b, ref float3 w)
        {
            return Vectors.ConvertToFloat(math.lerp(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Lerp2_2(ref float2 a, ref float2 b, ref float2 w)
        {
            return Vectors.ConvertToFloat(math.lerp(a, b, w));
        }

        // ---------------------------------------------------------
        // mad
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Mad4(ref float4 a, ref float4 b, ref float4 c)
        {
            return Vectors.ConvertToFloat(math.mad(a, b, c));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Mad3(ref float3 a, ref float3 b, ref float3 c)
        {
            return Vectors.ConvertToFloat(math.mad(a, b, c));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Mad2(ref float2 a, ref float2 b, ref float2 c)
        {
            return Vectors.ConvertToFloat(math.mad(a, b, c));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Mad(float a, float b, float c)
        {
            return math.mad(a, b, c);
        }

        // ---------------------------------------------------------
        // clamp
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Clamp4(ref float4 x, ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.clamp(x, a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Clamp3(ref float3 x, ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.clamp(x, a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Clamp2(ref float2 x, ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.clamp(x, a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Clamp(float x, float a, float b)
        {
            return math.clamp(x, a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static int ClampInt4(ref int4 x, ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(math.clamp(x, a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static int ClampInt3(ref int3 x, ref int3 a, ref int3 b)
        {
            return Vectors.ConvertToInt(math.clamp(x, a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static int ClampInt2(ref int2 x, ref int2 a, ref int2 b)
        {
            return Vectors.ConvertToInt(math.clamp(x, a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static int ClampInt(int x, int a, int b)
        {
            return math.clamp(x, a, b);
        }

        // ---------------------------------------------------------
        // saturate
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Saturate4(ref float4 x)
        {
            return Vectors.ConvertToFloat(math.saturate(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Saturate3(ref float3 x)
        {
            return Vectors.ConvertToFloat(math.saturate(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Saturate2(ref float2 x)
        {
            return Vectors.ConvertToFloat(math.saturate(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Saturate(float x)
        {
            return math.saturate(x);
        }

        // ---------------------------------------------------------
        // abs
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Abs4(ref float4 x)
        {
            return Vectors.ConvertToFloat(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Abs3(ref float3 x)
        {
            return Vectors.ConvertToFloat(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Abs2(ref float2 x)
        {
            return Vectors.ConvertToFloat(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Abs(float x)
        {
            return math.abs(x);
        }

        [TestCompiler(DataRange.Standard)]
        public static double AbsDouble4(ref double4 x)
        {
            return Vectors.ConvertToDouble(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static double AbsDouble3(ref double3 x)
        {
            return Vectors.ConvertToDouble(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static double AbsDouble2(ref double2 x)
        {
            return Vectors.ConvertToDouble(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static double AbsDouble(double x)
        {
            return math.abs(x);
        }

        [TestCompiler(DataRange.Standard)]
        public static int AbsInt4(ref int4 x)
        {
            return Vectors.ConvertToInt(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static int AbsInt3(ref int3 x)
        {
            return Vectors.ConvertToInt(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static int AbsInt2(ref int2 x)
        {
            return Vectors.ConvertToInt(math.abs(x));
        }

        [TestCompiler(DataRange.Standard)]
        public static int AbsInt(int x)
        {
            return math.abs(x);
        }

        // ---------------------------------------------------------
        // dot
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Dot4(ref float4 a, ref float4 b)
        {
            return math.dot(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Dot3(ref float3 a, ref float3 b)
        {
            return math.dot(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Dot2(ref float2 a, ref float2 b)
        {
            return math.dot(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Dot(float a, float b)
        {
            return math.dot(a, b);
        }

        // ---------------------------------------------------------
        // cmin
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard & ~DataRange.NaN)] // TODO: Does no handle NaN correctly
        public static float CMin4(ref float4 input)
        {
            return math.cmin(input);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.NaN)] // TODO: Does no handle NaN correctly
        public static float CMin3(ref float3 input)
        {
            return math.cmin(input);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.NaN)] // TODO: Does no handle NaN correctly
        public static float CMin2(ref float2 input)
        {
            return math.cmin(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CMinInt4(ref int4 input)
        {
            return math.cmin(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CMinInt3(ref int3 input)
        {
            return math.cmin(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CMinInt2(ref int2 input)
        {
            return math.cmin(input);
        }

        // ---------------------------------------------------------
        // cmax
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard & ~DataRange.NaN)] // TODO: Does no handle NaN correctly
        public static float CMax4(ref float4 input)
        {
            return math.cmax(input);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.NaN)] // TODO: Does no handle NaN correctly
        public static float CMax3(ref float3 input)
        {
            return math.cmax(input);
        }

        [TestCompiler(DataRange.Standard & ~DataRange.NaN)] // TODO: Does no handle NaN correctly
        public static float CMax2(ref float2 input)
        {
            return math.cmax(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CMaxInt4(ref int4 input)
        {
            return math.cmax(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CMaxInt3(ref int3 input)
        {
            return math.cmax(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CMaxInt2(ref int2 input)
        {
            return math.cmax(input);
        }

        // ---------------------------------------------------------
        // csum
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static double CSum4d(ref double4 input)
        {
            return math.csum(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CSum4(ref float4 input)
        {
            return math.csum(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CSum3(ref float3 input)
        {
            return math.csum(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CSum2(ref float2 input)
        {
            return math.csum(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CSumInt4(ref int4 input)
        {
            return math.csum(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CSumInt3(ref int3 input)
        {
            return math.csum(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float CSumInt2(ref int2 input)
        {
            return math.csum(input);
        }

        // ---------------------------------------------------------
        // acos
        // ---------------------------------------------------------
        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ACos4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.acos(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ACos3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.acos(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ACos2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.acos(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ACos(float input)
        {
            return math.acos(input);
        }

        // ---------------------------------------------------------
        // asin
        // ---------------------------------------------------------
        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ASin4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.asin(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ASin3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.asin(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ASin2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.asin(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ASin(float input)
        {
            return math.asin(input);
        }

        // ---------------------------------------------------------
        // atan
        // ---------------------------------------------------------
        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan_4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.atan(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan_3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.atan(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan_2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.atan(input));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan(float input)
        {
            return math.atan(input);
        }

        // ---------------------------------------------------------
        // atan2
        // ---------------------------------------------------------
        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive, DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan2_4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.atan2(a, b));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive, DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan2_3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.atan2(a, b));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive, DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan2_2(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.atan2(a, b));
        }

        [TestCompiler(DataRange.MinusOneInclusiveToOneInclusive, DataRange.MinusOneInclusiveToOneInclusive)]
        public static float ATan2(float a, float b)
        {
            return math.atan2(a, b);
        }

        // ---------------------------------------------------------
        // cos
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static double Cos4d(ref double4 input)
        {
            return Vectors.ConvertToDouble(math.cos(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cos4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.cos(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cos3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.cos(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cos2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.cos(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cos(float input)
        {
            return math.cos(input);
        }

        // ---------------------------------------------------------
        // cosh
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static double Cosh4d(ref double4 input)
        {
            return Vectors.ConvertToDouble(math.cosh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cosh4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.cosh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cosh3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.cosh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cosh2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.cosh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Cosh(float input)
        {
            return math.cosh(input);
        }

        // ---------------------------------------------------------
        // sin
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static double Sin4d(ref double4 input)
        {
            return Vectors.ConvertToDouble(math.sin(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sin4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.sin(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sin3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.sin(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sin2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.sin(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sin(float input)
        {
            return math.sin(input);
        }

        // ---------------------------------------------------------
        // sinh
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Sinh4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.sinh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sinh3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.sinh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sinh2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.sinh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sinh(float input)
        {
            return math.sinh(input);
        }

        // ---------------------------------------------------------
        // sincos
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float SinCos4(ref float4 input)
        {
            float4 sina, cosa;
            math.sincos(input, out sina, out cosa);
            return Vectors.ConvertToFloat(sina) + Vectors.ConvertToFloat(cosa) * 7.1f;
        }

        [TestCompiler(DataRange.Standard)]
        public static float SinCos3(ref float3 input)
        {
            float3 sina, cosa;
            math.sincos(input, out sina, out cosa);
            return Vectors.ConvertToFloat(sina) + Vectors.ConvertToFloat(cosa) * 7.1f;
        }

        [TestCompiler(DataRange.Standard)]
        public static float SinCos2(ref float2 input)
        {
            float2 sina, cosa;
            math.sincos(input, out sina, out cosa);
            return Vectors.ConvertToFloat(sina) + Vectors.ConvertToFloat(cosa) * 7.1f;
        }

        [TestCompiler(DataRange.Standard)]
        public static float SinCos(float input)
        {
            float sina, cosa;
            math.sincos(input, out sina, out cosa);
            return sina + cosa * 7.1f;
        }

        // ---------------------------------------------------------
        // tanh
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Tanh4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.tanh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Tanh3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.tanh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Tanh2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.tanh(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Tanh(float input)
        {
            return math.tanh(input);
        }

        // ---------------------------------------------------------
        // sqrt
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Sqrt4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.sqrt(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sqrt3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.sqrt(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sqrt2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.sqrt(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sqrt(float input)
        {
            return math.sqrt(input);
        }

        // ---------------------------------------------------------
        // rsqrt
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float RSqrt4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.rsqrt(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float RSqrt3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.rsqrt(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float RSqrt2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.rsqrt(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float RSqrt(float input)
        {
            return math.rsqrt(input);
        }

        // ---------------------------------------------------------
        // floor
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Floor4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.floor(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Floor3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.floor(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Floor2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.floor(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Floor(float input)
        {
            return math.floor(input);
        }

        // ---------------------------------------------------------
        // ceil
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Ceil4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.ceil(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Ceil3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.ceil(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Ceil2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.ceil(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Ceil(float input)
        {
            return math.ceil(input);
        }

        // ---------------------------------------------------------
        // round
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Round4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.round(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Round3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.round(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Round2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.round(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Round(float input)
        {
            return math.round(input);
        }

        // ---------------------------------------------------------
        // frac
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Frac4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.frac(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Frac3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.frac(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Frac2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.frac(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Frac(float input)
        {
            return math.frac(input);
        }

        // ---------------------------------------------------------
        // rcp
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Rcp4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.rcp(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Rcp3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.rcp(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Rcp2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.rcp(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Rcp(float input)
        {
            return math.rcp(input);
        }

        // ---------------------------------------------------------
        // sign
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Sign4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.sign(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sign3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.sign(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sign2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.sign(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Sign(float input)
        {
            return math.sign(input);
        }

        // ---------------------------------------------------------
        // pow
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard & ~(DataRange.NaN | DataRange.Zero), DataRange.Standard)]
        [TestCompiler(DataRange.Standard & ~(DataRange.NaN), DataRange.Standard & ~(DataRange.Zero))]
        public static float Pow4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.pow(a, b));
        }

        [TestCompiler(DataRange.Standard & ~(DataRange.NaN | DataRange.Zero), DataRange.Standard)]
        [TestCompiler(DataRange.Standard & ~(DataRange.NaN), DataRange.Standard & ~(DataRange.Zero))]
        public static float Pow3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.pow(a, b));
        }

        [TestCompiler(DataRange.Standard & ~(DataRange.NaN | DataRange.Zero), DataRange.Standard)]
        [TestCompiler(DataRange.Standard & ~(DataRange.NaN), DataRange.Standard & ~(DataRange.Zero))]
        public static float Pow2(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.pow(a, b));
        }

        [TestCompiler(DataRange.Standard & ~(DataRange.NaN | DataRange.Zero), DataRange.Standard)]
        [TestCompiler(DataRange.Standard & ~(DataRange.NaN), DataRange.Standard & ~(DataRange.Zero))]
        public static float Pow(float a, float b)
        {
            return math.pow(a, b);
        }

        // ---------------------------------------------------------
        // exp
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Exp4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.exp(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Exp3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.exp(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Exp2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.exp(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Exp(float input)
        {
            return math.exp(input);
        }

        // ---------------------------------------------------------
        // mod
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Mod4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.fmod(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Mod3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.fmod(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Mod2(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.fmod(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Mod(float a, float b)
        {
            return math.fmod(a, b);
        }

        // ---------------------------------------------------------
        // normalize
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Normalize4(ref float4 input)
        {
            return Vectors.ConvertToFloat(math.normalize(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Normalize3(ref float3 input)
        {
            return Vectors.ConvertToFloat(math.normalize(input));
        }

        [TestCompiler(DataRange.Standard)]
        public static float Normalize2(ref float2 input)
        {
            return Vectors.ConvertToFloat(math.normalize(input));
        }

        // ---------------------------------------------------------
        // length
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static float Length4(ref float4 input)
        {
            return math.length(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float Length3(ref float3 input)
        {
            return math.length(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float Length2(ref float2 input)
        {
            return math.length(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static float Length(float input)
        {
            return math.length(input);
        }

        // ---------------------------------------------------------
        // distance
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Distance4(ref float4 a, ref float4 b)
        {
            return math.distance(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Distance3(ref float3 a, ref float3 b)
        {
            return math.distance(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Distance2(ref float2 a, ref float2 b)
        {
            return math.distance(a, b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Distance(float a, float b)
        {
            return math.distance(a, b);
        }

        // ---------------------------------------------------------
        // cross
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Cross3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.cross(a, b));
        }

        // ---------------------------------------------------------
        // smoothstep
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Smoothstep4(ref float4 a, ref float4 b, float w)
        {
            return Vectors.ConvertToFloat(math.smoothstep(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Smoothstep3(ref float3 a, ref float3 b, float w)
        {
            return Vectors.ConvertToFloat(math.smoothstep(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Smoothstep2(ref float2 a, ref float2 b, float w)
        {
            return Vectors.ConvertToFloat(math.smoothstep(a, b, w));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.ZeroExclusiveToOneInclusive | DataRange.Zero)]
        public static float Smoothstep(float a, float b, float w)
        {
            return math.smoothstep(a, b, w);
        }

        // ---------------------------------------------------------
        // any
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static bool Any4(ref float4 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool Any32(ref float3 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool Any(ref float2 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AnyInt4(ref int4 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AnyInt3(ref int3 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AnyInt2(ref int2 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AnyBool4(ref bool4 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AnyBool3(ref bool3 input)
        {
            return math.any(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AnyBool2(ref bool2 input)
        {
            return math.any(input);
        }

        // ---------------------------------------------------------
        // all
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard)]
        public static bool All4(ref float4 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool All3(ref float3 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool All2(ref float2 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AllInt4(ref int4 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AllInt3(ref int3 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AllInt2(ref int2 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AllBool4(ref bool4 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AllBool3(ref bool3 input)
        {
            return math.all(input);
        }

        [TestCompiler(DataRange.Standard)]
        public static bool AllBool2(ref bool2 input)
        {
            return math.all(input);
        }

        // ---------------------------------------------------------
        // select
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Select4(ref bool4 c, ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.select(a, b, c));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Select3(ref bool3 c, ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.select(a, b, c));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard, DataRange.Standard)]
        public static float Select(ref bool2 c, ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.select(a, b, c));
        }

        // ---------------------------------------------------------
        // step
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Step4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.step(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Step3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.step(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Step2(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.step(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Step(float a, float b)
        {
            return math.step(a, b);
        }

        // ---------------------------------------------------------
        // reflect
        // ---------------------------------------------------------
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Reflect4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToFloat(math.reflect(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Reflect3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToFloat(math.reflect(a, b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float Reflect2(ref float2 a, ref float2 b)
        {
            return Vectors.ConvertToFloat(math.reflect(a, b));
        }

        struct TestCompressInt4
        {
#pragma warning disable 0649
            public int Value0;
            public int Value1;
            public int Value2;
            public int Value3;
#pragma warning restore 0649
        }
    }
}
