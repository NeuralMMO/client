using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

[TestFixture]
internal class UnsafeUtilityTests
{
#pragma warning disable 649
    struct DummyVec
    {
        public uint A, B, C, D;
    }
#pragma warning restore

    private static NativeArray<T> MakeTestArray<T>(params T[] data) where T : struct
    {
        return new NativeArray<T>(data, Allocator.TempJob);
    }

    [Test]
    public void ReinterpretUIntFloat()
    {
        using (var src = MakeTestArray(1.0f, 2.0f, 3.0f))
        {
            var dst = src.Reinterpret<float, uint>();
            Assert.AreEqual(src.Length, dst.Length);
            Assert.AreEqual(0x3f800000u, dst[0]);
            Assert.AreEqual(0x40000000u, dst[1]);
            Assert.AreEqual(0x40400000u, dst[2]);
        }
    }

    [Test]
    public void ReinterpretUInt4Float()
    {
        using (var src = MakeTestArray(1.0f, 2.0f, 3.0f, -1.0f))
        {
            var dst = src.Reinterpret<float, DummyVec>();
            Assert.AreEqual(1, dst.Length);

            var e = dst[0];
            Assert.AreEqual(0x3f800000u, e.A);
            Assert.AreEqual(0x40000000u, e.B);
            Assert.AreEqual(0x40400000u, e.C);
            Assert.AreEqual(0xbf800000u, e.D);
        }
    }

    [Test]
    public void ReinterpretFloatUint4()
    {
        var dummies = new DummyVec[]
        {
            new DummyVec { A = 0x3f800000u, B = 0x40000000u, C = 0x40400000u, D = 0xbf800000u },
            new DummyVec { A = 0xbf800000u, B = 0xc0000000u, C = 0xc0400000u, D = 0x3f800000u },
        };

        using (var src = MakeTestArray(dummies))
        {
            var dst = src.Reinterpret<DummyVec, float>();
            Assert.AreEqual(8, dst.Length);

            Assert.AreEqual(1.0f, dst[0]);
            Assert.AreEqual(2.0f, dst[1]);
            Assert.AreEqual(3.0f, dst[2]);
            Assert.AreEqual(-1.0f, dst[3]);
            Assert.AreEqual(-1.0f, dst[4]);
            Assert.AreEqual(-2.0f, dst[5]);
            Assert.AreEqual(-3.0f, dst[6]);
            Assert.AreEqual(1.0f, dst[7]);
        }
    }

    [Test]
    public void MismatchThrows1()
    {
        using (var src = MakeTestArray(0.0f, 1.0f, 2.0f))
        {
            Assert.Throws<InvalidOperationException>(() => src.Reinterpret<float, DummyVec>());
        }
    }

    [Test]
    public void MismatchThrows2()
    {
        using (var src = MakeTestArray(12))
        {
            Assert.Throws<InvalidOperationException>(() => src.Reinterpret<int, double>());
        }
    }

#if UNITY_2020_2_OR_NEWER
    [Test]
    public void AliasCanBeDisposed()
    {
        using (var src = MakeTestArray(12))
        {
            using (var dst = src.Reinterpret<int, float>())
            {
            }
        }
    }

#else
    [Test]
    [Ignore("Failing due to editor changes, disabled for now: https://unity3d.atlassian.net/browse/DOTS-1442")]
    public void CannotDisposeAlias()
    {
        using (var src = MakeTestArray(12))
        {
            var dst = src.Reinterpret<int, float>();
            Assert.Throws<InvalidOperationException>(() => dst.Dispose());
        }
    }

#endif

    [Test]
    public void CannotUseAliasAfterSourceIsDisposed()
    {
        NativeArray<float> alias;
        using (var src = MakeTestArray(12))
        {
            alias = src.Reinterpret<int, float>();
        }
        Assert.Throws<InvalidOperationException>(() => alias[0] = 1.0f);
    }

    [Test]
    public void MutabilityWorks()
    {
        using (var src = MakeTestArray(0.0f, -1.0f))
        {
            var alias = src.Reinterpret<float, uint>();
            alias[0] = 0x3f800000;
            Assert.AreEqual(1.0f, src[0]);
            Assert.AreEqual(-1.0f, src[1]);
        }
    }

#if UNITY_2020_2_OR_NEWER
    struct AlignOfX
    {
        float x;
        bool y;
    }

    struct AlignOfY
    {
        float x;
        bool y;
        float z;
        bool w;
    }

    struct AlignOfZ
    {
        float4 x;
        bool y;
    }

    struct AlignOfW
    {
        float4 x;
        bool y;
        float4x4 z;
        bool w;
    }

    [Test]
    public void UnsafeUtility_AlignOf()
    {
        Assert.AreEqual(UnsafeUtility.SizeOf<byte>(), UnsafeUtility.AlignOf<byte>());
        Assert.AreEqual(UnsafeUtility.SizeOf<short>(), UnsafeUtility.AlignOf<short>());
        Assert.AreEqual(UnsafeUtility.SizeOf<ushort>(), UnsafeUtility.AlignOf<ushort>());
        Assert.AreEqual(UnsafeUtility.SizeOf<int>(), UnsafeUtility.AlignOf<int>());
        Assert.AreEqual(UnsafeUtility.SizeOf<uint>(), UnsafeUtility.AlignOf<uint>());
        Assert.AreEqual(UnsafeUtility.SizeOf<long>(), UnsafeUtility.AlignOf<long>());
        Assert.AreEqual(UnsafeUtility.SizeOf<ulong>(), UnsafeUtility.AlignOf<ulong>());
        Assert.AreEqual(4, UnsafeUtility.AlignOf<float4>());

        Assert.AreEqual(4, UnsafeUtility.AlignOf<AlignOfX>());
        Assert.AreEqual(4, UnsafeUtility.AlignOf<AlignOfY>());
        Assert.AreEqual(4, UnsafeUtility.AlignOf<AlignOfZ>());
        Assert.AreEqual(4, UnsafeUtility.AlignOf<AlignOfW>());
    }

#endif
}
