using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.Tests;

internal class UnsafeCounterTests
{
    [Test, DotsRuntimeIgnore]
    public unsafe void UnsafeAtomicCounter32_AddSub()
    {
        int value = 0;
        var counter = new UnsafeAtomicCounter32(&value);

        Assert.AreEqual(0, counter.Add(123));
        Assert.AreEqual(123, counter.Add(0));
        Assert.AreEqual(123, counter.Sub(0));
        Assert.AreEqual(123, counter.AddSat(1, 123));
        Assert.AreEqual(123, counter.SubSat(1, 123));

        counter.AddSat(0xffff, 256);
        Assert.AreEqual(256, counter.Add(0));

        counter.SubSat(0xffff, -256);
        Assert.AreEqual(-256, counter.Add(0));
    }

    [Test, DotsRuntimeIgnore]
    public unsafe void UnsafeAtomicCounter64_AddSub()
    {
        long value = 0;
        var counter = new UnsafeAtomicCounter64(&value);

        Assert.AreEqual(0, counter.Add(123));
        Assert.AreEqual(123, counter.Add(0));
        Assert.AreEqual(123, counter.Sub(0));
        Assert.AreEqual(123, counter.AddSat(1, 123));
        Assert.AreEqual(123, counter.SubSat(1, 123));

        counter.AddSat(0xffff, 256);
        Assert.AreEqual(256, counter.Add(0));

        counter.SubSat(0xffff, -256);
        Assert.AreEqual(-256, counter.Add(0));
    }
}
