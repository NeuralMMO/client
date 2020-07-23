using System;
using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal partial class VectorsBinOpInt4
    {
        [TestCompiler]
        public static int Add()
        {
            var left = new int4(1);
            var right = new int4(1, 2, 3, 4);
            var result = left + right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int AddIntRight()
        {
            var left = new int4(1);
            var right = 2;
            var result = left + right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int AddIntLeft()
        {
            var left = 2;
            var right = new int4(1);
            var result = left + right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int AddByArgs(ref int4 left, ref int4 right)
        {
            var result = left + right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int Sub()
        {
            var left = new int4(1);
            var right = new int4(1, 2, 3, 4);
            var result = left - right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int SubIntLeft()
        {
            var left = 2;
            var right = new int4(1, 2, 3, 4);
            var result = left - right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int SubIntRight()
        {
            var left = new int4(1, 2, 3, 4);
            var right = 2;
            var result = left - right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int SubByArgs(ref int4 left, ref int4 right)
        {
            var result = left - right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int Mul()
        {
            var left = new int4(2, 1, 3, 5);
            var right = new int4(1, 2, 3, 4);
            var result = left * right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int MulIntLeft()
        {
            var left = 2;
            var right = new int4(1, 2, 3, 4);
            var result = left * right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int MulIntRight()
        {
            var left = new int4(1, 2, 3, 4);
            var right = 2;
            var result = left * right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int MulByArgs(ref int4 left, ref int4 right)
        {
            var result = left * right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int Div()
        {
            var left = new int4(1, 2, 3, 4);
            var right = new int4(2, 1, 3, 5);
            var result = left / right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int DivIntLeft()
        {
            var left = 15;
            var right = new int4(2, 1, 3, 5);
            var result = left / right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int DivIntRight()
        {
            var left = new int4(2, 1, 3, 5);
            var right = 15;
            var result = left / right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int LeftShift()
        {
            var left = new int4(2, 1, 3, 5);
            var right = 15;
            var result = left << right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int RightShift()
        {
            var left = new int4(2, -17, 3, Int32.MinValue);
            var right = 31;
            var result = left >> right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard & ~DataRange.Zero)]
        public static int DivByArgs(ref int4 left, ref int4 right)
        {
            var result = left / right;
            return Vectors.ConvertToInt(result);
        }

        [TestCompiler]
        public static int Neg()
        {
            var left = new int4(1, 2, 3, 4);
            return Vectors.ConvertToInt((-left));
        }

        [TestCompiler]
        public static int Positive()
        {
            var left = new int4(1, 2, 3, 4);
            return Vectors.ConvertToInt((+left));
        }

        // Comparisons
        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Equality(int a, int b)
        {
            return Vectors.ConvertToInt(new int4(a) == new int4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int EqualityInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a == b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Inequality(int a, int b)
        {
            return Vectors.ConvertToInt(new int4(a) != new int4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int InequalityInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a != b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThan(int a, int b)
        {
            return Vectors.ConvertToInt(new int4(a) > new int4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a > b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanOrEqual(int a, int b)
        {
            return Vectors.ConvertToInt(new int4(a) >= new int4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int GreaterThanOrEqualInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a >= b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThan(int a, int b)
        {
            return Vectors.ConvertToInt(new int4(a) < new int4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a < b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanOrEqual(int a, int b)
        {
            return Vectors.ConvertToInt(new int4(a) <= new int4(b));
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int LessThanOrEqualInt4(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a <= b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int And(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a & b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Or(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a | b);
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static int Xor(ref int4 a, ref int4 b)
        {
            return Vectors.ConvertToInt(a ^ b);
        }

        [TestCompiler(DataRange.Standard)]
        public static int ImplicitBitwiseNot(int a)
        {
            // Let int -> int4 implicit conversion
            return Vectors.ConvertToInt(~(int4) a);
        }

        [TestCompiler(DataRange.Standard)]
        public static int ImplicitInt(int a)
        {
            // Let int -> int4 implicit conversion
            return Vectors.ConvertToInt((int4) a);
        }

        [TestCompiler(DataRange.Standard)]
        public static int ImplicitInt4(ref int4 a)
        {
            // Let int4 -> int4 implicit conversion
            return Vectors.ConvertToInt(a);
        }
    }
}