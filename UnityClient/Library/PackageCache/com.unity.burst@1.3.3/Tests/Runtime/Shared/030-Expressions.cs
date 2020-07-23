using System;
using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal partial class Expressions
    {
        [TestCompiler((uint)(1 << 20))]
        [TestCompiler((uint)(1 << 15))]
        [TestCompiler(UInt32.MaxValue)]
        public static float ConvertUIntToFloat(uint rx)
        {
            var x = 2 * ((float)rx / uint.MaxValue - 0.5f);
            return x;
        }

        [TestCompiler((int)(1 << 20))]
        [TestCompiler((int)(1 << 15))]
        [TestCompiler(int.MinValue)]
        [TestCompiler(int.MaxValue)]
        public static float ConvertIntToFloat(int rx)
        {
            return (float) rx / -((float) int.MinValue);
        }

        [TestCompiler((int)-1, (uint)17)]
        [TestCompiler((int)(1 << 20), (uint)17)]
        [TestCompiler((int)(1 << 15), (uint)17)]
        [TestCompiler(int.MinValue, (uint)17)]
        [TestCompiler(int.MaxValue, (uint)17)]
        public static double ConvertIntToDouble(int rx, uint ry)
        {
            return (double)(rx  + (int)ry) * 0.5;
        }

        [TestCompiler((int)-1, (uint)17)]
        [TestCompiler((int)(1 << 20), (uint)17)]
        [TestCompiler((int)(1 << 15), (uint)17)]
        [TestCompiler(int.MinValue, (uint)17)]
        [TestCompiler(int.MaxValue, (uint)17)]
        public static double ConvertIntToDouble2(int rx, uint ry)
        {
            return (double)((uint)rx + ry) * 0.5;
        }

        [TestCompiler(int.MinValue)]
        [TestCompiler(-15)]
        [TestCompiler(-1)]
        [TestCompiler(1)]
        [TestCompiler(15)]
        [TestCompiler(int.MaxValue)]
        public static long ConvertIntToLong(int value)
        {
            return value;
        }

        [TestCompiler(int.MinValue)]
        [TestCompiler(-15)]
        [TestCompiler(-1)]
        [TestCompiler(1)]
        [TestCompiler(15)]
        [TestCompiler(int.MaxValue)]
        public static ulong ConvertIntToULong(int value)
        {
            return (ulong)value;
        }

        [TestCompiler()]
        public static ulong ConvertIntToLongConst()
        {
            return int.MaxValue;
        }

        [TestCompiler(1U)]
        [TestCompiler(15U)]
        [TestCompiler(uint.MaxValue)]
        public static long ConvertUIntToLong(uint value)
        {
            return value;
        }

        [TestCompiler()]
        public static ulong ConvertUIntToLongConst()
        {
            return uint.MaxValue;
        }

        [TestCompiler(1U)]
        [TestCompiler(15U)]
        [TestCompiler(uint.MaxValue)]
        public static ulong ConvertUIntToULong(uint value)
        {
            return value;
        }

        [TestCompiler()]
        public static ulong ConvertUIntToULongConst()
        {
            return uint.MaxValue;
        }

        [TestCompiler(short.MinValue)]
        [TestCompiler((short)-15)]
        [TestCompiler((short)-1)]
        [TestCompiler((short)1)]
        [TestCompiler((short)15)]
        [TestCompiler(short.MaxValue)]
        public static int ConvertShortToInt(short value)
        {
            return value;
        }

        [TestCompiler()]
        public static int ConvertShortToIntConstMin()
        {
            return short.MinValue;
        }

        [TestCompiler()]
        public static int ConvertShortToIntConstMax()
        {
            return short.MaxValue;
        }

        [TestCompiler()]
        public static int ConvertUShortToIntConstMax()
        {
            return ushort.MaxValue;
        }

        [TestCompiler()]
        public static uint ConvertUShortToUIntConstMax()
        {
            return ushort.MaxValue;
        }

        [TestCompiler(short.MinValue)]
        [TestCompiler((short)-15)]
        [TestCompiler((short)-1)]
        [TestCompiler((short)1)]
        [TestCompiler((short)15)]
        [TestCompiler(short.MaxValue)]
        public static long ConvertShortToLong(short value)
        {
            return value;
        }

        [TestCompiler()]
        public static long ConvertShortToLongConstMin()
        {
            return short.MinValue;
        }

        [TestCompiler()]
        public static long ConvertShortToLongConstMax()
        {
            return short.MaxValue;
        }

        [TestCompiler(short.MinValue)]
        [TestCompiler((short)-15)]
        [TestCompiler((short)-1)]
        [TestCompiler((short)1)]
        [TestCompiler((short)15)]
        [TestCompiler(short.MaxValue)]
        public static ulong ConvertShortToULong(short value)
        {
            return (ulong)value;
        }

        [TestCompiler()]
        public static ulong ConvertShortToULongConstMin()
        {
            return unchecked((ulong)short.MinValue);
        }

        [TestCompiler()]
        public static ulong ConvertShortToULongConstMax()
        {
            return (ulong)short.MaxValue;
        }

        [TestCompiler(sbyte.MinValue)]
        [TestCompiler((sbyte)-15)]
        [TestCompiler((sbyte)-1)]
        [TestCompiler((sbyte)1)]
        [TestCompiler((sbyte)15)]
        [TestCompiler(sbyte.MaxValue)]
        public static long ConvertSbyteToLong(sbyte value)
        {
            return value;
        }

        [TestCompiler]
        public static long ConvertSbyteToLongConstMin()
        {
            return sbyte.MinValue;
        }

        [TestCompiler]
        public static long ConvertSbyteToLongConstMax()
        {
            return sbyte.MinValue;
        }

        [TestCompiler(sbyte.MinValue)]
        [TestCompiler((sbyte)-15)]
        [TestCompiler((sbyte)-1)]
        [TestCompiler((sbyte)1)]
        [TestCompiler((sbyte)15)]
        [TestCompiler(sbyte.MaxValue)]
        public static uint ConvertSbyteToUInt(sbyte value)
        {
            return (uint)value;
        }

        [TestCompiler]
        public static uint ConvertSbyteToUIntConstMin()
        {
            return unchecked((uint)sbyte.MinValue);
        }

        [TestCompiler]
        public static uint ConvertSbyteToUIntConstMax()
        {
            return unchecked((uint)sbyte.MinValue);
        }

        [Ignore("Incorrect results in mono")]
        [TestCompiler(0.0f)]
        [TestCompiler(1.0f)]
        [TestCompiler(0.5f)]
        [TestCompiler(0.1f)]
        [TestCompiler(0.9f, OverrideResultOnMono = 135)]
        public static byte ConvertFloatToByte(float value)
        {
            return (byte) (150 * value);
        }

        [TestCompiler(true, true)]
        [TestCompiler(true, false)]
        [TestCompiler(false, true)]
        [TestCompiler(false, false)]
        public static bool CompareEqualBool(bool left, bool right)
        {
            return left == right;
        }

        [TestCompiler(true, true)]
        [TestCompiler(true, false)]
        [TestCompiler(false, true)]
        [TestCompiler(false, false)]
        public static bool CompareNotEqualBool(bool left, bool right)
        {
            return left != right;
        }

        [TestCompiler(true)]
        [TestCompiler(false)]
        public static bool CompareBoolWithConst(bool left)
        {
            return left == false;
        }

        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(-1, -1)]
        [TestCompiler(0, 0)]
        public static bool CompareEqualInt32(int left, int right)
        {
            return left == right;
        }

        [TestCompiler(1)]
        [TestCompiler(0)]
        [TestCompiler(-1)]
        public static bool CompareEqualInt32WithConst(int left)
        {
            return left == -1;
        }

        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(-1, -1)]
        [TestCompiler(0, 0)]
        public static bool CompareNotEqualInt32(int left, int right)
        {
            return left != right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(-1, 1)]
        [TestCompiler(0, 1)]
        public static bool CompareLessThanInt32(int left, int right)
        {
            return left < right;
        }

        [TestCompiler(1L, 5)]
        [TestCompiler(1L, 1)]
        [TestCompiler(0L, -1)]
        [TestCompiler(-1L, 1)]
        [TestCompiler(0L, 1)]
        public static bool CompareLessThanInt64Int32(long left, int right)
        {
            return left < right;
        }

        [TestCompiler(1U, 5)]
        [TestCompiler(1U, 1)]
        [TestCompiler(0U, -1)]
        [TestCompiler(0x80000000U, 1)]
        [TestCompiler(0xFFFFFFFFU, 1)]
        [TestCompiler(0U, 1)]
        public static bool CompareLessThanUInt32Int32(uint left, int right)
        {
            return left < right;
        }

        [TestCompiler(1U, 5)]
        [TestCompiler(1U, 1)]
        [TestCompiler(0U, -1)]
        [TestCompiler(0x80000000U, 1)]
        [TestCompiler(0xFFFFFFFFU, 1)]
        [TestCompiler(0U, 1)]
        public static bool CompareGreaterThanUInt32Int32(uint left, int right)
        {
            return left > right;
        }

        [TestCompiler(5, 1U)]
        [TestCompiler(1, 1U)]
        [TestCompiler(-1, 0U)]
        [TestCompiler(1, 0x80000000U)]
        [TestCompiler(1, 0xFFFFFFFFU)]
        [TestCompiler(1, 0U)]
        public static bool CompareLessThanInt32UInt32(int left, uint right)
        {
            return left < right;
        }

        [TestCompiler(5, 1U)]
        [TestCompiler(1, 1U)]
        [TestCompiler(-1, 0U)]
        [TestCompiler(1, 0x80000000U)]
        [TestCompiler(1, 0xFFFFFFFFU)]
        [TestCompiler(1, 0U)]
        public static bool CompareGreaterThanInt32UInt32(int left, uint right)
        {
            return left > right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(-1, 1)]
        [TestCompiler(0, 1)]
        public static bool CompareGreaterThanInt32(int left, int right)
        {
            return left > right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static bool CompareGreaterOrEqualInt32(int left, int right)
        {
            return left >= right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static bool CompareLessOrEqualInt32(int left, int right)
        {
            return left <= right;
        }

        [TestCompiler]
        public static bool CompareEqualFloatConstant()
        {
            return 0 == float.NaN;
        }

        [TestCompiler]
        public static bool CompareNotEqualFloatConstant()
        {
            return 0 != float.NaN;
        }

        [TestCompiler]
        public static bool CompareLessThanFloatConstant()
        {
            return 0 < float.NaN;
        }

        [TestCompiler]
        public static bool CompareLessThanEqualFloatConstant()
        {
            return 0 <= float.NaN;
        }

        [TestCompiler]
        public static bool CompareGreaterThanFloatConstant()
        {
            return 0 > float.NaN;
        }

        [TestCompiler]
        public static bool CompareLGreaterThanEqualFloatConstant()
        {
            return 0 >= float.NaN;
        }

        [TestCompiler(DataRange.Minus100To100, DataRange.Minus100To100)]
        [TestCompiler(0, float.NaN)]
        [TestCompiler(float.NaN, float.NaN)]
        [TestCompiler(float.PositiveInfinity, float.NegativeInfinity)]
        [TestCompiler(float.PositiveInfinity, float.PositiveInfinity)]
        [TestCompiler(float.NegativeInfinity, float.NegativeInfinity)]
        public static bool CompareEqualFloat(float left, float right)
        {
            return left == right;
        }

        [TestCompiler(DataRange.Minus100To100, DataRange.Minus100To100)]
        [TestCompiler(0, -0)]
        [TestCompiler(0, float.NaN)]
        [TestCompiler(float.NaN, float.NaN)]
        [TestCompiler(float.PositiveInfinity, float.NegativeInfinity)]
        [TestCompiler(float.PositiveInfinity, float.PositiveInfinity)]
        [TestCompiler(float.NegativeInfinity, float.NegativeInfinity)]
        public static bool CompareNotEqualFloat(float left, float right)
        {
            return left != right;
        }

        [TestCompiler(DataRange.Minus100To100, DataRange.Minus100To100)]
        [TestCompiler(0, -0)]
        [TestCompiler(0, float.NaN)]
        [TestCompiler(float.NaN, float.NaN)]
        [TestCompiler(float.PositiveInfinity, float.NegativeInfinity)]
        [TestCompiler(float.PositiveInfinity, float.PositiveInfinity)]
        [TestCompiler(float.NegativeInfinity, float.NegativeInfinity)]
        public static bool CompareLessThanFloat(float left, float right)
        {
            return left < right;
        }

        [TestCompiler(DataRange.Minus100To100, DataRange.Minus100To100)]
        [TestCompiler(0, -0)]
        [TestCompiler(0, float.NaN)]
        [TestCompiler(float.NaN, float.NaN)]
        [TestCompiler(float.PositiveInfinity, float.NegativeInfinity)]
        [TestCompiler(float.PositiveInfinity, float.PositiveInfinity)]
        [TestCompiler(float.NegativeInfinity, float.NegativeInfinity)]
        public static bool CompareLessThanEqualFloat(float left, float right)
        {
            return left <= right;
        }

        [TestCompiler(DataRange.Minus100To100, DataRange.Minus100To100)]
        [TestCompiler(0, -0)]
        [TestCompiler(0, float.NaN)]
        [TestCompiler(float.NaN, float.NaN)]
        [TestCompiler(float.PositiveInfinity, float.NegativeInfinity)]
        [TestCompiler(float.PositiveInfinity, float.PositiveInfinity)]
        [TestCompiler(float.NegativeInfinity, float.NegativeInfinity)]
        public static bool CompareGreaterThanFloat(float left, float right)
        {
            return left > right;
        }

        [TestCompiler(DataRange.Minus100To100, DataRange.Minus100To100)]
        [TestCompiler(0, -0)]
        [TestCompiler(0, float.NaN)]
        [TestCompiler(float.NaN, float.NaN)]
        [TestCompiler(float.PositiveInfinity, float.NegativeInfinity)]
        [TestCompiler(float.PositiveInfinity, float.PositiveInfinity)]
        [TestCompiler(float.NegativeInfinity, float.NegativeInfinity)]
        public static bool CompareGreaterThanEqualFloat(float left, float right)
        {
            return left >= right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericAdd(int left, int right)
        {
            return left + right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericSub(int left, int right)
        {
            return left - right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericDiv(int left, int right)
        {
            return left / right;
        }

        [TestCompiler(1L, 5)]
        [TestCompiler(1L, 1)]
        [TestCompiler(0L, -1)]
        [TestCompiler(40L, -1)]
        [TestCompiler(-10L, 3)]
        [TestCompiler(0L, 13)]
        [TestCompiler(4L, 7)]
        [TestCompiler(125L, 7)]
        public static long BinaryNumericDiv64(long left, int right)
        {
            return left / right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericRem(int left, int right)
        {
            return left % right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericMul(int left, int right)
        {
            return left * right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericAnd(int left, int right)
        {
            return left & right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericOr(int left, int right)
        {
            return left | right;
        }

        [TestCompiler(1, 5)]
        [TestCompiler(1, 1)]
        [TestCompiler(0, -1)]
        [TestCompiler(40, -1)]
        [TestCompiler(-10, 3)]
        [TestCompiler(0, 13)]
        [TestCompiler(4, 7)]
        [TestCompiler(125, 7)]
        public static int BinaryNumericXor(int left, int right)
        {
            return left ^ right;
        }

        [TestCompiler(1, 0)]
        [TestCompiler(1, 5)]
        [TestCompiler(7, 10)]
        [TestCompiler(-1, 1)]
        public static int BinaryNumericShiftLeft(int left, int right)
        {
            return left << right;
        }

        [TestCompiler(1, 0)]
        [TestCompiler(1, 5)]
        [TestCompiler(7, 10)]
        [TestCompiler(-1, 1)]
        public static int BinaryNumericShiftRight(int left, int right)
        {
            return left >> right;
        }

        [TestCompiler(1U, 0)]
        [TestCompiler(1U, 5)]
        [TestCompiler(7U, 10)]
        [TestCompiler(0xFFFFFFFFU, 1)]
        public static uint BinaryNumericShiftLeftUInt32(uint left, int right)
        {
            return left << right;
        }

        [TestCompiler(1U, 0)]
        [TestCompiler(1U, 5)]
        [TestCompiler(7U, 10)]
        [TestCompiler(0x80000000U, 1)]
        [TestCompiler(0xFFFFFFFFU, 1)]
        public static uint BinaryNumericShiftRightUInt32(uint left, int right)
        {
            return left >> right;
        }

        [TestCompiler(1U, 0)]
        [TestCompiler(1U, 5)]
        [TestCompiler(7U, 10)]
        [TestCompiler(0x80000000U, 1)]
        [TestCompiler(0xFFFFFFFFU, 1)]
        public static int BinaryNumericShiftRightUIntToInt32(uint left, int right)
        {
            return ((int)left) >> right;
        }

        [TestCompiler]
        public static int ConstantMinus1()
        {
            return -1;
        }

        [TestCompiler]
        public static int Constant1()
        {
            return 1;
        }

        [TestCompiler]
        public static int Constant2()
        {
            return 2;
        }

        [TestCompiler]
        public static int Constant3()
        {
            return 3;
        }

        [TestCompiler]
        public static int Constant4()
        {
            return 4;
        }

        [TestCompiler]
        public static int Constant5()
        {
            return 5;
        }

        [TestCompiler]
        public static int Constant6()
        {
            return 6;
        }

        [TestCompiler]
        public static int Constant7()
        {
            return 7;
        }

        [TestCompiler]
        public static int Constant8()
        {
            return 8;
        }

        [TestCompiler]
        public static int Constant121()
        {
            return 121;
        }

        [TestCompiler]
        public static bool ReturnBoolTrue()
        {
            return true;
        }

        [TestCompiler]
        public static bool ReturnBoolFalse()
        {
            return false;
        }

        [TestCompiler((int)0x10203040)]
        [TestCompiler((int)0x20203040)]
        [TestCompiler((int)0x30203040)]
        [TestCompiler((int)0x40203040)]
        [TestCompiler((int)0x50203040)]
        [TestCompiler((int)0x60203040)]
        [TestCompiler((int)0x70203040)]
        public static int AddOverflowInt(int x)
        {
            x += 0x70506070;
            return x;
        }

        [TestCompiler]
        public static int test_expr_add_one_to_zero()
        {
            var x = 0;
            x++;
            return x;
        }

        [TestCompiler(1f)]
        public static float test_expr_add_multiples(float a)
        {
            return a + a + a + a;
        }


        [TestCompiler(1f, 2f)]
        public static float test_expr_add_two_arguments(float a, float b)
        {
            return a + b;
        }

        [TestCompiler(3f, 4f)]
        public static float test_expr_multiply_two_arguments(float a, float b)
        {
            return a * b;
        }

        [TestCompiler(3f)]
        [TestCompiler(-4f)]
        [TestCompiler(0f)]
        public static float test_expr_negateResult_float(float a)
        {
            return -a;
        }

        [TestCompiler((sbyte)3)]
        [TestCompiler((sbyte)-4)]
        [TestCompiler((sbyte)0)]
        [TestCompiler(sbyte.MinValue)]
        [TestCompiler(sbyte.MaxValue)]
        public static int test_expr_negateResult_sbyte(sbyte a)
        {
            return -a;
        }

        [TestCompiler((byte)3)]
        [TestCompiler((byte)0)]
        [TestCompiler(byte.MaxValue, OverrideManagedResult = -255)] // TODO: IL2CPP on macOS currently produces incorrect result of "1". Remove this OverrideManagedResult when that bug is fixed.
        public static int test_expr_negateResult_byte(byte a)
        {
            return -a;
        }

        [TestCompiler((short)3)]
        [TestCompiler((short)-4)]
        [TestCompiler((short)0)]
        [TestCompiler(short.MinValue)]
        [TestCompiler(short.MaxValue)]
        public static int test_expr_negateResult_short(short a)
        {
            return -a;
        }

        [TestCompiler((ushort)3)]
        [TestCompiler((ushort)0)]
        [TestCompiler(ushort.MaxValue, OverrideManagedResult = -65535)] // TODO: IL2CPP on macOS currently produces incorrect result of "1". Remove this OverrideManagedResult when that bug is fixed.
        public static int test_expr_negateResult_ushort(ushort a)
        {
            return -a;
        }

        [TestCompiler(3)]
        [TestCompiler(-4)]
        [TestCompiler(0)]
        [TestCompiler(int.MinValue)]
        [TestCompiler(int.MaxValue)]
        public static int test_expr_negateResult_int(int a)
        {
            return -a;
        }

        [TestCompiler(3u)]
        [TestCompiler(0u)]
        [TestCompiler(uint.MaxValue)]
        public static long test_expr_negateResult_uint(uint a)
        {
            return -a;
        }

        [TestCompiler((long)3)]
        [TestCompiler((long)-4)]
        [TestCompiler((long)0)]
        [TestCompiler(long.MinValue)]
        [TestCompiler(long.MaxValue)]
        public static long test_expr_negateResult_long(long a)
        {
            return -a;
        }

        [TestCompiler]
        public static float test_expr_return_constant()
        {
            return 12f;
        }

        [TestCompiler]
        public static float test_multiple_assigment()
        {
            float x, y, z;
            x = y = z = 5.0F;
            return x + y + z;
        }

        [TestCompiler(4f, 9f)]
        public static float test_expr_various_math(float a, float b)
        {
            return (a + b) * b * b * 0.4f / (a + a + a * 0.2f);
        }

        [TestCompiler(4.1f)]
        public static float test_expr_multiply_int_by_float(float a)
        {
            var i = 18;
            return i * a;
        }

        [TestCompiler(4f)]
        public static int test_expr_cast_float_to_int(float a)
        {
            return (int)a;
        }

        [TestCompiler(4)]
        public static float test_expr_cast_int_to_float(int a)
        {
            return a;
        }

        [TestCompiler(5)]
        public static int test_expr_assign_to_argument(int a)
        {
            a = a * a;
            return a;
        }

        [TestCompiler(7)]
        public static int test_expr_postincrement(int input)
        {
            var a = input++;
            return a + input;
        }

        [TestCompiler(2)]
        [TestCompiler(3)]
        public static int test_expr_mod(int input)
        {
            return input % 2;
        }

        [TestCompiler(0, 0)]
        [TestCompiler(0, 1)]
        [TestCompiler(1, 0)]
        [TestCompiler(1, 1)]
        public static int test_expr_xor(int a, int b)
        {
            return a ^ b;
        }

        [TestCompiler(1, 2)]
        [TestCompiler(0, 0)]
        [TestCompiler(1, 0)]
        [TestCompiler(1, 1)]
        public static int test_expr_or(int a, int b)
        {
            return a | b;
        }

        [TestCompiler(1, 3)]
        [TestCompiler(0, 0)]
        [TestCompiler(1, 0)]
        [TestCompiler(1, 1)]
        public static int test_expr_and(int a, int b)
        {
            return a & b;
        }

        [TestCompiler(-100000.0F)]
        public static float test_math_large_values(float a)
        {
            return (a * a) + ((a + 3.0F) * a);
        }

        [TestCompiler(1)]
        [TestCompiler(150)]
        [TestCompiler(-1)]
        [TestCompiler(-150)]
        public static int test_expr_shift_right(int n)
        {
            return n >> 3;
        }

        [TestCompiler(1)]
        [TestCompiler(31)]
        public static int test_expr_shift(int n)
        {
            int a = 5;
            a <<= n;
            a += (a >> 31);
            return a;
        }

        [TestCompiler(2)]
        [TestCompiler(-3)]
        public static int test_expr_complement(int input)
        {
            return ~input;
        }

        [TestCompiler]
        public static int test_expr_sizeof_int()
        {
            return sizeof(int);
        }

        [TestCompiler(-1)]
        [TestCompiler(0)]
        [TestCompiler(12)]
        public static int test_expr_generic_equatable(int a)
        {
            if (EqualityTester<int>.Check(a, 12))
                return 1;
            else
                return 0;
        }

        struct EqualityTester<TKey> where TKey : IEquatable<TKey>
        {
            public static bool Check(TKey value1, TKey value2)
            {
                return value1.Equals(value2);
            }
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(-1)]
        public static bool test_expr_bool_passing(int a)
        {
            return a == 0;
        }

        const int ConstValue = 5;
        [TestCompiler()]
        public static int test_expr_load_static_constant()
        {
            return ConstValue;
        }

        [TestCompiler(1)]
        [TestCompiler(-1)]
        public static int OutInt32(int a)
        {
            int b;
            OutputInt32(out b);
            return a + b;
        }

        [TestCompiler(-1)]
        [TestCompiler(1)]
        public static int CallPushAndPop(int a)
        {
            int result = 0;
            int value;

            TryAdd(a, out value);
            result += value * 10;
            TryAdd(a * 2, out value);
            result += value * 10;
            TryAdd(a * 3, out value);
            result += value * 10;
            TryAdd(a * 4, out value);
            result += value * 10;
            return result;
        }

        private static readonly Yoyo[] StaticArray2 = new Yoyo[5];

        struct Yoyo
        {
#pragma warning disable 0169, 0649
            public int a;
            private int b;
#pragma warning restore 0169, 0649
        }

        private static bool TryAdd(int a, out int result)
        {
            result = a  + 5;
            return true;
        }

        public static void OutputInt32(out int value)
        {
            value = 5;
        }

        [TestCompiler]
        public static long TypeConversionAndOverflow()
        {
            byte ba = 0xFF;
            byte bb = 1;
            sbyte sba = 127;
            sbyte sbb = 1;
            short sa = 0x7FFF;
            short sb = 1;
            ushort usa = 0xFFFF;
            ushort usb = 1;
            uint x = 0xFFFFFFFF;
            int y = 1;
            long z = 1;
            return (ba + bb) + (sba + sbb) + (sa + sb) + (usa + usb) + (x + y) + (x + z);
        }

        private static void AssignValue(int switchValue, ref float value)
        {
            value = 0.0F;

            if (switchValue == 0)
                return;

            value = 1.0F;

            if (switchValue == 1)
                return;

            value = 2.0F;

            return;
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(2)]
        public static float test_expr_return_from_branch(int test)
        {
            float ret_val = -1.0F;
            AssignValue(test, ref ret_val);
            return ret_val;
        }

        [TestCompiler(DataRange.Standard, DataRange.Standard)]
        public static bool BoolOrFunction(bool left, int x)
        {
            return left | ReturnBool(x);
        }

        private static bool ReturnBool(int x)
        {
            return x > 5;
        }

        [TestCompiler()]
        public static unsafe uint TestStackAlloc()
        {
            uint* result = stackalloc uint[4];
            for (uint i = 0; i < 4; i++)
            {
                result[i] = i + 1;
            }

            uint sum = 0;
            for (uint i = 0; i < 4; i++)
            {
                sum += result[i];
            }

            return sum;
        }

        public static int BoolRefUser(ref bool isDone)
        {
            return 1;
        }

        [TestCompiler()]
        public static int LocalBoolPassedByRef()
        {
            var isDone = false;
            return BoolRefUser(ref isDone);
        }

        public enum TestEnum
        {
            v0 = 0,
            v1 = 1,
            v2 = 2,
            v3 = 3,
            v4 = 4,
            v5 = 5,
            v6 = 6,
        }

        public static float SameCode(TestEnum val1, TestEnum val2)
        {
            float diff = val2 - val1;
            return diff;
        }

        [TestCompiler()]
        public static float EnumToFloatConversion()
        {
            return SameCode(TestEnum.v6, TestEnum.v0);
        }

        public enum SByteEnum : sbyte
        {
            A = 0, B = 6, C = -128
        }

        [TestCompiler(SByteEnum.C)]
        public static float TestSByteEnum(SByteEnum a)
        {
            return (float) a;
        }

        public enum UnsignedEnum : uint
        {
            A = 0, B = 6, C = 0xFFFFFFFF
        }

        [TestCompiler(UnsignedEnum.C)]
        public static float TestUnsignedEnum(UnsignedEnum a)
        {
            return (float) a;
        }

        [TestCompiler(1)]
        public static int AddOvf(int x)
        {
            return checked(x + 1);
        }

        [TestCompiler(1)]
        public static int MulOvf(int x)
        {
            return checked(x * 2);
        }

        [TestCompiler(1)]
        public static int SubOvf(int x)
        {
            return checked(x - 1);
        }

        [TestCompiler(1u)]
        public static uint SubOvfUn(uint x)
        {
            return checked(x - 1);
        }

        [TestCompiler(1u)]
        public static uint BgeUn(uint x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x >= 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x;
        }

        [TestCompiler(1)]
        public static int Bgt(int x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x > 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x;
        }

        [TestCompiler(1)]
        public static int Beq(int x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x == 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x;
        }

        [TestCompiler(1)]
        public static int Bge(int x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x >= 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x;
        }

        [TestCompiler(1)]
        public static int Ble(int x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x <= 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x;
        }

        [TestCompiler(1)]
        public static int Blt(int x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x < 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                                x * x * x * x;
        }

        [TestCompiler(1u)]
        public static uint BgtUn(uint x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x > 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x;
        }

        [TestCompiler(1u)]
        public static uint BgtUnS(uint x)
        {
            return x > 1 ? 1 : x;
        }

        [TestCompiler(1u)]
        public static uint BleUn(uint x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x <= 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x;
        }

        [TestCompiler(1u)]
        public static uint BltUn(uint x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x < 1 ? 1 : x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x * x *
                               x * x * x * x;
        }

        [TestCompiler(1u)]
        public static uint BltUnS(uint x)
        {
            // We need a non-short opcode, therefore the branch has to be big enough for the offset to not fit in a byte:
            return x < 1 ? 1 : 0u;
        }

        [TestCompiler(true)]
        public static int Brtrue(bool x)
        {
            return x ? 1 : 0;
        }

        [TestCompiler(false)]
        public static int Brfalse(bool x)
        {
            return !x ? 1 : 0;
        }

        [TestCompiler(1)]
        public static sbyte ConvI1(int x)
        {
            return (sbyte) x;
        }

        [TestCompiler(1)]
        public static short ConvI2(int x)
        {
            return (short) x;
        }

        [TestCompiler(1u)]
        public static sbyte ConvOvfI1Un(uint x)
        {
            return checked((sbyte) x);
        }

        [TestCompiler(1u)]
        public static short ConvOvfI2Un(uint x)
        {
            return checked((short) x);
        }

        [TestCompiler(1u)]
        public static int ConvOvfI4Un(uint x)
        {
            return checked((int) x);
        }

        [TestCompiler(1ul)]
        public static long ConvOvfI8Un(ulong x)
        {
            return checked((long) x);
        }

        [TestCompiler(1u)]
        public static byte ConvOvfU1Un(uint x)
        {
            return checked((byte) x);
        }

        [TestCompiler(1u)]
        public static ushort ConvOvfU2Un(uint x)
        {
            return checked((ushort) x);
        }

        [TestCompiler(1ul)]
        public static uint ConvOvfU4Un(ulong x)
        {
            return checked((uint) x);
        }

        [TestCompiler(1)]
        public static sbyte ConvOvfI1(int x)
        {
            return checked((sbyte) x);
        }

        [TestCompiler(1)]
        public static short ConvOvfI2(int x)
        {
            return checked((short) x);
        }

        [TestCompiler(1)]
        public static int ConvOvfI4(long x)
        {
            return checked((int) x);
        }

        [TestCompiler(1)]
        public static long ConvOvfI8(double x)
        {
            return checked((long) x);
        }

        [TestCompiler(1)]
        public static byte ConvOvfU1(int x)
        {
            return checked((byte) x);
        }

        [TestCompiler(1)]
        public static ushort ConvOvfU2(int x)
        {
            return checked((ushort) x);
        }

        [TestCompiler(1)]
        public static uint ConvOvfU4(int x)
        {
            return checked((uint) x);
        }

        [TestCompiler(1)]
        public static ulong ConvOvfU8(double x)
        {
            return checked((ulong) x);
        }

        private static readonly int[] MyArray = { 0, 1, 2 };

        [TestCompiler((byte)0)]
        public static int LdelemByte(byte index) => MyArray[index];

        [TestCompiler((ushort)0)]
        public static int LdelemUInt16(ushort index) => MyArray[index];

        [TestCompiler((uint)0)]
        public static int LdelemUInt32(uint index) => MyArray[index];

        [TestCompiler((ulong)0)]
        public static int LdelemUInt64(ulong index) => MyArray[index];

        [TestCompiler((short)0)]
        public static int LdelemInt16(short index) => MyArray[index];

        [TestCompiler(0)]
        public static int LdelemInt32(int index) => MyArray[index];

        [TestCompiler((long)0)]
        public static int LdelemInt64(long index) => MyArray[index];

        [TestCompiler(1.0f)]
        public static float FSubByDenormBecomesFAdd(float x)
        {
            return x - 1.40129846432481707092e-45f;
        }

        [TestCompiler(1.0f)]
        public static float FSubByDenormBecomesFAddWithVec(float x)
        {
            var r = x - new float2(1.40129846432481707092e-45f, -1.40129846432481707092e-45f);
            return r.x * r.y;
        }
    }
}
