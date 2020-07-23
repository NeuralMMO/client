using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal partial class VectorsBinOpFloat4
    {
        [TestCompiler]
        public static float Add()
        {
            var left = new float4(1.0f);
            var right = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float AddFloatRight()
        {
            var left = new float4(1.0f);
            var right = 2.0f;
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float AddFloatLeft()
        {
            var left = 2.0f;
            var right = new float4(1.0f);
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float AddByArgs(ref float4 left, ref float4 right)
        {
            var result = left + right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Sub()
        {
            var left = new float4(1.0f);
            var right = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float SubFloatLeft()
        {
            var left = 2.0f;
            var right = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float SubFloatRight()
        {
            var left = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var right = 2.0f;
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float SubByArgs(ref float4 left, ref float4 right)
        {
            var result = left - right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Mul()
        {
            var left = new float4(2.0f, 1.0f, 3.0f, 5.0f);
            var right = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float MulFloatLeft()
        {
            var left = 2.0f;
            var right = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float MulFloatRight()
        {
            var left = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var right = 2.0f;
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float MulByArgs(ref float4 left, ref float4 right)
        {
            var result = left * right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Div()
        {
            var left = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var right = new float4(2.0f, 1.0f, 3.0f, 5.0f);
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float DivFloatLeft()
        {
            var left = 15.0f;
            var right = new float4(2.0f, 1.0f, 3.0f, 5.0f);
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float DivFloatRight()
        {
            var left = new float4(2.0f, 1.0f, 3.0f, 5.0f);
            var right = 15.0f;
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static float DivByArgs(ref float4 left, ref float4 right)
        {
            var result = left / right;
            return Vectors.ConvertToFloat(result);
        }

        [TestCompiler]
        public static float Neg()
        {
            var left = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            return Vectors.ConvertToFloat((-left));
        }

        [TestCompiler]
        public static float Positive()
        {
            var left = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            return Vectors.ConvertToFloat((+left));
        }

        // Comparisons
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Equality(float a, float b)
        {
            return Vectors.ConvertToInt(new float4(a) == new float4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int EqualityFloat4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Inequality(float a, float b)
        {
            return Vectors.ConvertToInt(new float4(a) != new float4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int InequalityFloat4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThan(float a, float b)
        {
            return Vectors.ConvertToInt(new float4(a) > new float4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanFloat4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a > b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanFloat4Float(ref float4 a, float b)
        {
            return Vectors.ConvertToInt(a > b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static unsafe bool GreaterThanFloat4FloatUnsafe(ref float4 a, float b)
        {
            float4 x = a;
            float4* start = &x;
            int axis = 0;

            return CompareViaIndexer(start, axis, b);
        }

        private static unsafe bool CompareViaIndexer(float4* start, int axis, float b)
        {
            float4 copy = *start;
            return copy[axis] >= b;
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanOrEqual(float a, float b)
        {
            return Vectors.ConvertToInt(new float4(a) >= new float4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanOrEqualFloat4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a >= b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThan(float a, float b)
        {
            return Vectors.ConvertToInt(new float4(a) < new float4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanFloat4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a < b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanFloat4Float(ref float4 a, float b)
        {
            return Vectors.ConvertToInt(a < b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanOrEqual(float a, float b)
        {
            return Vectors.ConvertToInt(new float4(a) <= new float4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanOrEqualFloat4(ref float4 a, ref float4 b)
        {
            return Vectors.ConvertToInt(a <= b);
        }

        [TestCompiler(DataRange.Standard)]
        public static float ImplicitFloat(float a)
        {
            // Let float -> float4 implicit conversion
            return Vectors.ConvertToFloat((float4) a);
        }

        [TestCompiler(DataRange.Standard)]
        public static float ImplicitInt4(ref int4 a)
        {
            // Let int4 -> float4 implicit conversion
            return Vectors.ConvertToFloat(a);
        }
    }
}