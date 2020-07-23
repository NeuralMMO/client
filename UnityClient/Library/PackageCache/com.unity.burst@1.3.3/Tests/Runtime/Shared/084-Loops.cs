using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;

namespace Burst.Compiler.IL.Tests
{
#if BURST_INTERNAL || UNITY_BURST_EXPERIMENTAL_LOOP_INTRINSICS
    internal class LoopIntrinsics
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectVectorizedImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectVectorized();

                a[i] += b[i];
            }
        }

        [TestCompiler(100)]
        [OptimizationsOnly("Loops are not vectorized when optimizations are disabled")]
        public static unsafe void CheckExpectVectorized(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectVectorizedImpl(a, b, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static unsafe void CheckExpectVectorizedNoOptimizationsImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectVectorized();

                a[i] += b[i];
            }
        }

        [TestCompiler(100, ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_LoopUnexpectedAutoVectorization)]
        public static unsafe void CheckExpectVectorizedNoOptimizations(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectVectorizedNoOptimizationsImpl(a, b, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static unsafe void CheckExpectNotVectorizedNoOptimizationsImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectNotVectorized();

                a[i] += b[i];
            }
        }

        [TestCompiler(100)]
        public static unsafe void CheckExpectNotVectorizedNoOptimizations(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectNotVectorizedNoOptimizationsImpl(a, b, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectVectorizedOptimizationsDisabledImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectVectorized();

                a[i] += b[i];
            }
        }

        [TestCompiler(100, ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_LoopUnexpectedAutoVectorization)]
        [OptimizationsDisabled("Test Loop.ExpectVectorized behavior when optimizations are disabled")]
        public static unsafe void CheckExpectVectorizedOptimizationsDisabled(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectVectorizedOptimizationsDisabledImpl(a, b, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectNotVectorizedImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectNotVectorized();

                if (a[i] > b[i])
                {
                    break;
                }

                a[i] += b[i];
            }
        }

        [TestCompiler(100)]
        public static unsafe void CheckExpectNotVectorized(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectNotVectorizedImpl(a, b, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectVectorizedFailImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectVectorized();

                if (a[i] > b[i])
                {
                    break;
                }

                a[i] += b[i];
            }
        }

        [TestCompiler(100, ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_LoopUnexpectedAutoVectorization)]
        public static unsafe void CheckExpectVectorizedFail(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectVectorizedFailImpl(a, b, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectNotVectorizedFailImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectNotVectorized();

                a[i] += b[i];
            }
        }

        [TestCompiler(100, ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_LoopUnexpectedAutoVectorization)]
        [OptimizationsOnly("Loops are not vectorized when optimizations are disabled")]
        public static unsafe void CheckExpectNotVectorizedFail(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectNotVectorizedFailImpl(a, b, count);
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_LoopIntrinsicMustBeCalledInsideLoop)]
        public static unsafe void CheckExpectVectorizedOutsideLoop()
        {
            Loop.ExpectVectorized();
        }

        [TestCompiler(ExpectCompilerException = true, ExpectedDiagnosticId = DiagnosticId.ERR_LoopIntrinsicMustBeCalledInsideLoop)]
        public static unsafe void CheckExpectNotVectorizedOutsideLoop()
        {
            Loop.ExpectNotVectorized();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectVectorizedMultipleCallsImpl([NoAlias] int* a, [NoAlias] int* b, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectVectorized();

                a[i] += b[i];

                Loop.ExpectVectorized();
            }
        }

        [TestCompiler(100)]
        [OptimizationsOnly("Loops are not vectorized when optimizations are disabled")]
        public static unsafe void CheckExpectVectorizedMultipleCalls(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectVectorizedMultipleCallsImpl(a, b, count);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectVectorizedUnrolledLoopImpl([NoAlias] int* a, [NoAlias] int* b)
        {
            for (var i = 0; i < 4; i++)
            {
                Loop.ExpectVectorized();

                if (a[i] > b[i])
                {
                    a[i] += b[i];
                }
            }
        }

        [TestCompiler(100, ExpectedDiagnosticId = DiagnosticId.WRN_LoopIntrinsicCalledButLoopOptimizedAway)]
        [OptimizationsOnly("Loops are not vectorized when optimizations are disabled")]
        public static unsafe void CheckExpectVectorizedUnrolledLoop(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];

            CheckExpectVectorizedUnrolledLoopImpl(a, b);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe int CheckExpectVectorizedPartiallyUnrolledLoopImpl(int* a, int count)
        {
            var sum = 0;

            for (var i = 0; i < count; i++)
            {
                Loop.ExpectVectorized();

                sum += a[i];
            }

            return sum;
        }

        [TestCompiler(100)]
        [OptimizationsOnly("Loops are not vectorized when optimizations are disabled")]
        public static unsafe int CheckExpectVectorizedPartiallyUnrolledLoop(int count)
        {
            var a = stackalloc int[count];

            a[0] = 8;
            a[10] = 16;

            return CheckExpectVectorizedPartiallyUnrolledLoopImpl(a, count);
        }

        [TestCompiler(100, ExpectedDiagnosticId = DiagnosticId.WRN_LoopIntrinsicCalledButLoopOptimizedAway)]
        [OptimizationsOnly("Loops are not vectorized when optimizations are disabled")]
        public static unsafe void CheckExpectVectorizedRemovedLoop(int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectVectorized();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void CheckExpectVectorizedNestedImpl(
            [NoAlias] int* a, [NoAlias] int* b,
            [NoAlias] int* c, [NoAlias] int* d,
            int count)
        {
            for (var i = 0; i < count; i++)
            {
                Loop.ExpectNotVectorized();

                if (a[i] > b[i])
                {
                    break;
                }

                a[i] += b[i];

                for (var j = i; j < count; j++)
                {
                    Loop.ExpectVectorized();

                    c[j] += d[j];
                }
            }
        }

        [TestCompiler(100)]
        [OptimizationsOnly("Loops are not vectorized when optimizations are disabled")]
        public static unsafe void CheckExpectVectorizedNested(int count)
        {
            var a = stackalloc int[count];
            var b = stackalloc int[count];
            var c = stackalloc int[count];
            var d = stackalloc int[count];

            CheckExpectVectorizedNestedImpl(a, b, c, d, count);
        }
    }
#endif
}
