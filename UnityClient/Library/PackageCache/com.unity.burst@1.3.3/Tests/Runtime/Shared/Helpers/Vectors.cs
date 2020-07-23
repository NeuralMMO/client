using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal static partial class Vectors
    {
        public static int ConvertToInt(bool4 result)
        {
            return ConvertToInt(result.x) + ConvertToInt(result.y) * 10 + ConvertToInt(result.z) * 100 + ConvertToInt(result.w) * 1000;
        }

        public static int ConvertToInt(bool3 result)
        {
            return ConvertToInt(result.x) + ConvertToInt(result.y) * 10 + ConvertToInt(result.z) * 100;
        }

        public static int ConvertToInt(bool2 result)
        {
            return ConvertToInt(result.x) + ConvertToInt(result.y) * 10;
        }

        public static float ConvertToFloat(float4 result)
        {
            return result.x + result.y * 10.0f + result.z * 100.0f + result.w * 1000.0f;
        }

        public static double ConvertToDouble(double4 result)
        {
            return result.x + result.y * 10.0 + result.z * 100.0 + result.w * 1000.0;
        }

        public static float ConvertToFloat(float3 result)
        {
            return result.x + result.y * 10.0f + result.z * 100.0f;
        }

        public static double ConvertToDouble(double3 result)
        {
            return result.x + result.y * 10.0 + result.z * 100.0;
        }

        public static float ConvertToFloat(float2 result)
        {
            return result.x + result.y * 10.0f;
        }

        public static double ConvertToDouble(double2 result)
        {
            return result.x + result.y * 10.0;
        }

        public static int ConvertToInt(int4 result)
        {
            return result.x + result.y * 10 + result.z * 100 + result.w * 1000;
        }

        public static int ConvertToInt(int3 result)
        {
            return result.x + result.y * 10 + result.z * 100;
        }

        public static int ConvertToInt(int2 result)
        {
            return result.x + result.y * 10;
        }

        public static int ConvertToInt(uint4 result)
        {
            return (int)(result.x + result.y * 10 + result.z * 100 + result.w * 1000);
        }

        public static int ConvertToInt(uint3 result)
        {
            return (int)(result.x + result.y * 10 + result.z * 100);
        }

        public static int ConvertToInt(uint2 result)
        {
            return (int)(result.x + result.y * 10);
        }

        public static int ConvertToInt(bool value)
        {
            return value ? 1 : 0;
        }
    }
}