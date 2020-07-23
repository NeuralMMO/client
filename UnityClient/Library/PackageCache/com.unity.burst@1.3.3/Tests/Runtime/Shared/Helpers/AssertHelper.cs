using System;
using System.Numerics;
using NUnit.Framework;

#if BURST_INTERNAL
using System.Text;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
#endif

namespace Burst.Compiler.IL.Tests.Helpers
{
    internal static class AssertHelper
    {
#if BURST_INTERNAL
        // Workaround for Mono broken Equals() on v64/v128/v256
        private static bool AreVectorsEqual(v64 a, v64 b)
        {
            return a.SLong0 == b.SLong0;
        }

        private static bool AreVectorsEqual(v128 a, v128 b)
        {
            return a.SLong0 == b.SLong0 && a.SLong1 == b.SLong1;
        }

        private static bool AreVectorsEqual(v256 a, v256 b)
        {
            return AreVectorsEqual(a.Lo128, b.Lo128) && AreVectorsEqual(a.Hi128, b.Hi128);
        }
#endif

        /// <summary>
        /// AreEqual handling specially precision for float and intrinsic vector types
        /// </summary>
        /// <param name="expected">The expected result</param>
        /// <param name="result">the actual result</param>
        public static void AreEqual(object expected, object result, int maxUlp)
        {
            if (expected is float && result is float)
            {
                var expectedF = (float)expected;
                var resultF = (float)result;
                Assert.True(NearEqualFloat(expectedF, resultF, maxUlp, out var ulp), $"Expected: {expectedF} != Result: {resultF}, ULPs: {ulp}");
                return;
            }

            if (expected is double && result is double)
            {
                var expectedF = (double)expected;
                var resultF = (double)result;
                Assert.True(NearEqualDouble(expectedF, resultF, maxUlp, out var ulp), $"Expected: {expectedF} != Result: {resultF}, ULPs: {ulp}");
                return;
            }

#if BURST_INTERNAL
            if (expected is float2 && result is float2)
            {
                var expectedF = (float2)expected;
                var resultF = (float2)result;
                Assert.True(NearEqualFloat(expectedF.x, resultF.x, maxUlp, out var ulp), $"Expected: {expectedF}.x != Result: {resultF}.x, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.y, resultF.y, maxUlp, out ulp), $"Expected: {expectedF}.y != Result: {resultF}.y, ULPs: {ulp}");
                return;
            }

            if (expected is float3 && result is float3)
            {
                var expectedF = (float3)expected;
                var resultF = (float3)result;
                Assert.True(NearEqualFloat(expectedF.x, resultF.x, maxUlp, out var ulp), $"Expected: {expectedF}.x != Result: {resultF}.x, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.y, resultF.y, maxUlp, out ulp), $"Expected: {expectedF}.y != Result: {resultF}.y, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.z, resultF.z, maxUlp, out ulp), $"Expected: {expectedF}.z != Result: {resultF}.z, ULPs: {ulp}");
                return;
            }

            if (expected is float4 && result is float4)
            {
                var expectedF = (float4)expected;
                var resultF = (float4)result;
                Assert.True(NearEqualFloat(expectedF.x, resultF.x, maxUlp, out var ulp), $"Expected: {expectedF}.x != Result: {resultF}.x, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.y, resultF.y, maxUlp, out ulp), $"Expected: {expectedF}.y != Result: {resultF}.y, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.z, resultF.z, maxUlp, out ulp), $"Expected: {expectedF}.z != Result: {resultF}.z, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.w, resultF.w, maxUlp, out ulp), $"Expected: {expectedF}.w != Result: {resultF}.w, ULPs: {ulp}");
                return;
            }

            if (expected is float4x2 && result is float4x2)
            {
                var expectedF = (float4x2)expected;
                var resultF = (float4x2)result;
                Assert.True(NearEqualFloat(expectedF.c0.x, resultF.c0.x, maxUlp, out var ulp), $"Expected: {expectedF}.c0.x != Result: {resultF}.c0.x, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.c0.y, resultF.c0.y, maxUlp, out ulp), $"Expected: {expectedF}.c0.y != Result: {resultF}.c0.y, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.c0.z, resultF.c0.z, maxUlp, out ulp), $"Expected: {expectedF}.c0.z != Result: {resultF}.c0.z, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.c0.w, resultF.c0.w, maxUlp, out ulp), $"Expected: {expectedF}.c0.w != Result: {resultF}.c0.w, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.c1.x, resultF.c1.x, maxUlp, out ulp), $"Expected: {expectedF}.c1.x != Result: {resultF}.c1.x, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.c1.y, resultF.c1.y, maxUlp, out ulp), $"Expected: {expectedF}.c1.y != Result: {resultF}.c1.y, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.c1.z, resultF.c1.z, maxUlp, out ulp), $"Expected: {expectedF}.c1.z != Result: {resultF}.c1.z, ULPs: {ulp}");
                Assert.True(NearEqualFloat(expectedF.c1.w, resultF.c1.w, maxUlp, out ulp), $"Expected: {expectedF}.c1.w != Result: {resultF}.c1.w, ULPs: {ulp}");
                return;
            }

            if (expected is double2 && result is double2)
            {
                var expectedF = (double2)expected;
                var resultF = (double2)result;
                Assert.True(NearEqualDouble(expectedF.x, resultF.x, maxUlp, out var ulp), $"Expected: {expectedF}.x != Result: {resultF}.x, ULPs: {ulp}");
                Assert.True(NearEqualDouble(expectedF.y, resultF.y, maxUlp, out ulp), $"Expected: {expectedF}.y != Result: {resultF}.y, ULPs: {ulp}");
                return;
            }

            if (expected is double3 && result is double3)
            {
                var expectedF = (double3)expected;
                var resultF = (double3)result;
                Assert.True(NearEqualDouble(expectedF.x, resultF.x, maxUlp, out var ulp), $"Expected: {expectedF}.x != Result: {resultF}.x, ULPs: {ulp}");
                Assert.True(NearEqualDouble(expectedF.y, resultF.y, maxUlp, out ulp), $"Expected: {expectedF}.y != Result: {resultF}.y, ULPs: {ulp}");
                Assert.True(NearEqualDouble(expectedF.z, resultF.z, maxUlp, out ulp), $"Expected: {expectedF}.z != Result: {resultF}.z, ULPs: {ulp}");
                return;
            }

            if (expected is double4 && result is double4)
            {
                var expectedF = (double4)expected;
                var resultF = (double4)result;
                Assert.True(NearEqualDouble(expectedF.x, resultF.x, maxUlp, out var ulp), $"Expected: {expectedF}.x != Result: {resultF}.x, ULPs: {ulp}");
                Assert.True(NearEqualDouble(expectedF.y, resultF.y, maxUlp, out ulp), $"Expected: {expectedF}.y != Result: {resultF}.y, ULPs: {ulp}");
                Assert.True(NearEqualDouble(expectedF.z, resultF.z, maxUlp, out ulp), $"Expected: {expectedF}.z != Result: {resultF}.z, ULPs: {ulp}");
                Assert.True(NearEqualDouble(expectedF.w, resultF.w, maxUlp, out ulp), $"Expected: {expectedF}.w != Result: {resultF}.w, ULPs: {ulp}");
                return;
            }

            if (expected is v64 && result is v64)
            {
                if (!AreVectorsEqual((v64)expected, (v64)result))
                {
                    Assert.Fail(FormatVectorFailure64((v64)expected, (v64)result));
                }
                return;
            }

            if (expected is v128 && result is v128)
            {
                if (!AreVectorsEqual((v128)expected, (v128)result))
                {
                    Assert.Fail(FormatVectorFailure128((v128)expected, (v128)result));
                }
                return;
            }

            if (expected is v256 && result is v256)
            {
                if (!AreVectorsEqual((v256)expected, (v256)result))
                {
                    Assert.Fail(FormatVectorFailure256((v256)expected, (v256)result));
                }
                return;
            }
#endif

            Assert.AreEqual(expected, result);
        }

#if BURST_INTERNAL
        private unsafe static string FormatVectorFailure64(v64 expected, v64 result)
        {
            var b = new StringBuilder();
            b.AppendLine("64-bit vectors differ!");
            b.AppendLine("Expected:");
            FormatVector(b, (void*)&expected, 8);
            b.AppendLine();
            b.AppendLine("But was :");
            FormatVector(b, (void*)&result, 8);
            b.AppendLine();
            return b.ToString();
        }

        private unsafe static string FormatVectorFailure128(v128 expected, v128 result)
        {
            var b = new StringBuilder();
            b.AppendLine("128-bit vectors differ!");
            b.AppendLine("Expected:");
            FormatVector(b, (void*)&expected, 16);
            b.AppendLine();
            b.AppendLine("But was :");
            FormatVector(b, (void*)&result, 16);
            b.AppendLine();
            return b.ToString();
        }

        private unsafe static string FormatVectorFailure256(v256 expected, v256 result)
        {
            var b = new StringBuilder();
            b.AppendLine("256-bit vectors differ!");
            b.AppendLine("Expected:");
            FormatVector(b, (void*)&expected, 32);
            b.AppendLine();
            b.AppendLine("But was :");
            FormatVector(b, (void*)&result, 32);
            b.AppendLine();
            return b.ToString();
        }

        private unsafe static void FormatVector(StringBuilder b, void* v, int bytes)
        {
            b.Append("Double: ");
            for (int i = 0; i < bytes / 8; ++i)
            {
                if (i > 0)
                    b.AppendFormat(" | ");
                b.AppendFormat("{0:G17}", ((double*)v)[i]);
            }
            b.AppendLine();
            b.Append("Float : ");
            for (int i = 0; i < bytes / 4; ++i)
            {
                if (i > 0)
                    b.AppendFormat(" | ");
                b.AppendFormat("{0:G15}", ((float*)v)[i]);
            }

            b.AppendLine();
            b.Append("UInt32: ");
            for (int i = 0; i < bytes / 4; ++i)
            {
                if (i > 0)
                    b.AppendFormat(" | ");
                b.AppendFormat("{0:X8}", ((uint*)v)[i]);
            }
            b.AppendLine();
        }
#endif

        /// <summary>
        /// The value for which all absolute numbers smaller than are considered equal to zero.
        /// </summary>
        public const float ZeroTolerance = 4 * float.Epsilon;

        /// <summary>
        /// The value for which all absolute numbers smaller than are considered equal to zero.
        /// </summary>
        public const double ZeroToleranceDouble = 4 * double.Epsilon;

        public static bool NearEqualFloat(float a, float b, int maxUlp, out int ulp)
        {
            ulp = 0;
            if (Math.Abs(a - b) < ZeroTolerance) return true;

            ulp = GetUlpFloatDistance(a, b);
            return ulp <= maxUlp;
        }

        public static unsafe int GetUlpFloatDistance(float a, float b)
        {
            // Save work if the floats are equal.
            // Also handles +0 == -0
            if (a == b)
            {
                return 0;
            }

            if (float.IsNaN(a) && float.IsNaN(b))
            {
                return 0;
            }

            if (float.IsInfinity(a) && float.IsInfinity(b))
            {
                return 0;
            }

            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            if ((aInt < 0) != (bInt < 0)) return int.MaxValue;

            // Because we would have an overflow below while trying to do -(int.MinValue)
            // We modify it here so that we don't overflow
            var ulp = (long)aInt - bInt;

            if (ulp <= int.MinValue) return int.MaxValue;
            if (ulp > int.MaxValue) return int.MaxValue;

            // We know for sure that numbers are in the range ]int.MinValue, int.MaxValue]
            return (int)Math.Abs(ulp);
        }

        public static bool NearEqualDouble(double a, double b, int maxUlp, out long ulp)
        {
            ulp = 0;
            if (Math.Abs(a - b) < ZeroTolerance) return true;

            ulp = GetUlpDoubleDistance(a, b);
            return ulp <= maxUlp;
        }

        private static readonly long LongMinValue = long.MinValue;
        private static readonly long LongMaxValue = long.MaxValue;

        public static unsafe long GetUlpDoubleDistance(double a, double b)
        {
            // Save work if the floats are equal.
            // Also handles +0 == -0
            if (a == b)
            {
                return 0;
            }

            if (double.IsNaN(a) && double.IsNaN(b))
            {
                return 0;
            }

            if (double.IsInfinity(a) && double.IsInfinity(b))
            {
                return 0;
            }

            long aInt = *(long*)&a;
            long bInt = *(long*)&b;

            if ((aInt < 0) != (bInt < 0)) return long.MaxValue;

            var ulp = aInt - bInt;

            if (ulp <= LongMinValue) return long.MaxValue;
            if (ulp > LongMaxValue) return long.MaxValue;

            return Math.Abs((long)ulp);
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(double a)
        {
            return Math.Abs(a) < ZeroToleranceDouble;
        }
    }
}