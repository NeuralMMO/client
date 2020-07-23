using Burst.Compiler.IL.Tests.Helpers;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityBenchShared;
using static Unity.Burst.Intrinsics.Common;

namespace Burst.Compiler.IL.Tests
{
    internal partial class IntrinsicsCommon
    {
#if BURST_INTERNAL || UNITY_BURST_EXPERIMENTAL_PAUSE_INTRINSIC
        [TestCompiler]
        public static void CheckPause()
        {
            Pause();
        }
#endif

        public unsafe struct Buffer : IDisposable
        {
            public int* Data;
            public int Length;

            public void Dispose()
            {
                UnsafeUtility.Free(Data, Allocator.Persistent);
            }

            public class Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var length = 1024;
                        var data = (int*)UnsafeUtility.Malloc(sizeof(int) * length, UnsafeUtility.AlignOf<int>(), Allocator.Persistent);

                        for (var i = 0; i < length; i++)
                        {
                            data[i] = i;
                        }

                        return new Buffer { Data = data, Length = length };
                    }
                }
            }
        }

#if BURST_INTERNAL || UNITY_BURST_EXPERIMENTAL_PREFETCH_INTRINSIC
        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe int CheckPrefetchReadNoTemporalLocality(ref Buffer buffer)
        {
            int total = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                total += buffer.Data[i];
                Prefetch(buffer.Data + i + 1, ReadWrite.Read, Locality.NoTemporalLocality);
            }
            return total;
        }

        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe int CheckPrefetchReadLowTemporalLocality(ref Buffer buffer)
        {
            int total = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                total += buffer.Data[i];
                Prefetch(buffer.Data + i + 1, ReadWrite.Read, Locality.LowTemporalLocality);
            }
            return total;
        }

        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe int CheckPrefetchReadModerateTemporalLocality(ref Buffer buffer)
        {
            int total = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                total += buffer.Data[i];
                Prefetch(buffer.Data + i + 1, ReadWrite.Read, Locality.ModerateTemporalLocality);
            }
            return total;
        }

        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe int CheckPrefetchReadHighTemporalLocality(ref Buffer buffer)
        {
            int total = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                total += buffer.Data[i];
                Prefetch(buffer.Data + i + 1, ReadWrite.Read, Locality.HighTemporalLocality);
            }
            return total;
        }

        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe void CheckPrefetchWriteNoTemporalLocality(ref Buffer buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer.Data[i] = i;
                Prefetch(buffer.Data + i + 1, ReadWrite.Write, Locality.NoTemporalLocality);
            }
        }

        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe void CheckPrefetchWriteLowTemporalLocality(ref Buffer buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer.Data[i] = i;
                Prefetch(buffer.Data + i + 1, ReadWrite.Write, Locality.LowTemporalLocality);
            }
        }

        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe void CheckPrefetchWriteModerateTemporalLocality(ref Buffer buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer.Data[i] = i;
                Prefetch(buffer.Data + i + 1, ReadWrite.Write, Locality.ModerateTemporalLocality);
            }
        }

        [TestCompiler(typeof(Buffer.Provider))]
        public static unsafe void CheckPrefetchWriteHighTemporalLocality(ref Buffer buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer.Data[i] = i;
                Prefetch(buffer.Data + i + 1, ReadWrite.Write, Locality.HighTemporalLocality);
            }
        }
#endif

        [TestCompiler(typeof(ReturnBox), ulong.MaxValue, ulong.MaxValue)]
        [TestCompiler(typeof(ReturnBox), ulong.MinValue, ulong.MaxValue)]
        [TestCompiler(typeof(ReturnBox), ulong.MaxValue, ulong.MinValue)]
        [TestCompiler(typeof(ReturnBox), ulong.MinValue, ulong.MinValue)]
        [TestCompiler(typeof(ReturnBox), DataRange.Standard, DataRange.Standard)]
        public static unsafe ulong Checkumul128(ulong* high, ulong x, ulong y)
        {
            var result = umul128(x, y, out var myHigh);
            *high = myHigh;
            return result;
        }
    }
}
