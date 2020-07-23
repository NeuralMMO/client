using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal partial class VectorsBinOpFloat3
    {
        [TestCompiler]
        public static float Add()
        {
            var left = new float3(1.0f);
            var right = new float3(1.0f, 2.0f, 3.0f);
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float AddFloatRight()
        {
            var left = new float3(1.0f);
            var right = 2.0f;
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float AddFloatLeft()
        {
            var left = 2.0f;
            var right = new float3(1.0f);
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float AddByArgs(ref float3 left, ref float3 right)
        {
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Sub()
        {
            var left = new float3(1.0f);
            var right = new float3(1.0f, 2.0f, 3.0f);
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float SubFloatLeft()
        {
            var left = 2.0f;
            var right = new float3(1.0f, 2.0f, 3.0f);
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float SubFloatRight()
        {
            var left = new float3(1.0f, 2.0f, 3.0f);
            var right = 2.0f;
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float SubByArgs(ref float3 left, ref float3 right)
        {
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Mul()
        {
            var left = new float3(2.0f, 1.0f, 3.0f);
            var right = new float3(1.0f, 2.0f, 3.0f);
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float MulFloatLeft()
        {
            var left = 2.0f;
            var right = new float3(1.0f, 2.0f, 3.0f);
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float MulFloatRight()
        {
            var left = new float3(1.0f, 2.0f, 3.0f);
            var right = 2.0f;
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float MulByArgs(ref float3 left, ref float3 right)
        {
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Div()
        {
            var left = new float3(1.0f, 2.0f, 3.0f);
            var right = new float3(2.0f, 1.0f, 3.0f);
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float DivFloatLeft()
        {
            var left = 15.0f;
            var right = new float3(2.0f, 1.0f, 3.0f);
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float DivFloatRight()
        {
            var left = new float3(2.0f, 1.0f, 3.0f);
            var right = 15.0f;
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float DivByArgs(ref float3 left, ref float3 right)
        {
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Neg()
        {
            var left = new float3(1.0f, 2.0f, 3.0f);
            return Vectors.ConvertToFloat((-left));
        }

        [TestCompiler]
        public static float Positive()
        {
            var left = new float3(1.0f, 2.0f, 3.0f);
            return Vectors.ConvertToFloat((+left));
        }

        // Comparisons
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Equality(float a, float b)
        {
            return Vectors.ConvertToInt((new float3(a) == new float3(b)));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int EqualityFloat3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt((a == b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Inequality(float a, float b)
        {
            return Vectors.ConvertToInt((new float3(a) != new float3(b)));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int InequalityFloat3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt((a != b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThan(float a, float b)
        {
            return Vectors.ConvertToInt((new float3(a) > new float3(b)));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanFloat3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt((a > b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanOrEqual(float a, float b)
        {
            return Vectors.ConvertToInt((new float3(a) >= new float3(b)));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanOrEqualFloat3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt((a >= b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThan(float a, float b)
        {
            return Vectors.ConvertToInt((new float3(a) < new float3(b)));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanFloat3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt((a < b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanOrEqual(float a, float b)
        {
            return Vectors.ConvertToInt((new float3(a) <= new float3(b)));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanOrEqualFloat3(ref float3 a, ref float3 b)
        {
            return Vectors.ConvertToInt((a <= b));
        }

        [TestCompiler(DataRange.Standard)]
        public static float ImplicitFloat(float a)
        {
            // Let float -> float3 implicit conversion
            return Vectors.ConvertToFloat((float3)a);
        }

        [TestCompiler(DataRange.Standard)]
        public static float ImplicitInt4(ref int4 a)
        {
            // Let int4 -> float3 implicit conversion
            return Vectors.ConvertToFloat(a);
        }
    }
}