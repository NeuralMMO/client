using System;
using NUnit.Framework;
using Unity.Collections;

internal class BitFieldTests
{
    [Test]
    public void BitField32_Get_Set()
    {
        var test = new BitField32();

        uint bits;

        bits = test.GetBits(0, 32);
        Assert.AreEqual(0x0, bits);

        test.SetBits(0, true);
        bits = test.GetBits(0, 32);
        Assert.AreEqual(0x1, bits);

        test.SetBits(0, true, 32);
        bits = test.GetBits(0, 32);
        Assert.AreEqual(0xffffffff, bits);
        Assert.IsTrue(test.TestAll(0, 32));

        test.SetBits(0, false, 32);
        bits = test.GetBits(0, 32);
        Assert.AreEqual(0x0, bits);

        test.SetBits(15, true, 7);
        Assert.IsTrue(test.TestAll(15, 7));
        test.SetBits(3, true, 3);
        Assert.IsTrue(test.TestAll(3, 3));
        bits = test.GetBits(0, 32);
        Assert.AreEqual(0x3f8038, bits);
        bits = test.GetBits(0, 15);
        Assert.AreEqual(0x38, bits);
        Assert.IsFalse(test.TestNone(0, 32));
        Assert.IsFalse(test.TestAll(0, 32));
        Assert.IsTrue(test.TestAny(0, 32));
    }

    [Test]
    public void BitField32_Count_Leading_Trailing()
    {
        var test = new BitField32();

        Assert.AreEqual(0, test.CountBits());
        Assert.AreEqual(32, test.CountLeadingZeros());
        Assert.AreEqual(32, test.CountTrailingZeros());

        test.SetBits(31, true);
        Assert.AreEqual(1, test.CountBits());
        Assert.AreEqual(0, test.CountLeadingZeros());
        Assert.AreEqual(31, test.CountTrailingZeros());

        test.SetBits(0, true);
        Assert.AreEqual(2, test.CountBits());
        Assert.AreEqual(0, test.CountLeadingZeros());
        Assert.AreEqual(0, test.CountTrailingZeros());

        test.SetBits(31, false);
        Assert.AreEqual(1, test.CountBits());
        Assert.AreEqual(31, test.CountLeadingZeros());
        Assert.AreEqual(0, test.CountTrailingZeros());
    }

    [Test]
    public void BitField32_Throws()
    {
        var test = new BitField32();

        for (byte i = 0; i < 32; ++i)
        {
            Assert.DoesNotThrow(() => { test.GetBits(i, (byte)(32 - i)); });
        }

        Assert.Throws<ArgumentException>(() => { test.GetBits(0, 33); });
        Assert.Throws<ArgumentException>(() => { test.GetBits(1, 32); });
    }

    [Test]
    public void BitField64_Get_Set()
    {
        var test = new BitField64();

        ulong bits;

        bits = test.GetBits(0, 64);
        Assert.AreEqual(0x0, bits);

        test.SetBits(0, true);
        bits = test.GetBits(0, 64);
        Assert.AreEqual(0x1, bits);

        test.SetBits(0, true, 64);
        bits = test.GetBits(0, 64);
        Assert.AreEqual(0xfffffffffffffffful, bits);
        Assert.IsTrue(test.TestAll(0, 64));

        test.SetBits(0, false, 64);
        bits = test.GetBits(0, 64);
        Assert.AreEqual(0x0ul, bits);

        test.SetBits(15, true, 7);
        Assert.IsTrue(test.TestAll(15, 7));
        test.SetBits(3, true, 3);
        Assert.IsTrue(test.TestAll(3, 3));
        bits = test.GetBits(0, 32);
        Assert.AreEqual(0x3f8038, bits);
        bits = test.GetBits(0, 15);
        Assert.AreEqual(0x38, bits);
        Assert.IsFalse(test.TestNone(0, 64));
        Assert.IsFalse(test.TestAll(0, 64));
        Assert.IsTrue(test.TestAny(0, 64));
    }

    [Test]
    public void BitField64_Throws()
    {
        var test = new BitField64();

        for (byte i = 0; i < 64; ++i)
        {
            Assert.DoesNotThrow(() => { test.GetBits(i, (byte)(64 - i)); });
        }

        Assert.Throws<ArgumentException>(() => { test.GetBits(0, 65); });
        Assert.Throws<ArgumentException>(() => { test.GetBits(1, 64); });
    }

    [Test]
    public void BitField64_Count_Leading_Trailing()
    {
        var test = new BitField64();

        Assert.AreEqual(0, test.CountBits());
        Assert.AreEqual(64, test.CountLeadingZeros());
        Assert.AreEqual(64, test.CountTrailingZeros());

        test.SetBits(63, true);
        Assert.AreEqual(1, test.CountBits());
        Assert.AreEqual(0, test.CountLeadingZeros());
        Assert.AreEqual(63, test.CountTrailingZeros());

        test.SetBits(0, true);
        Assert.AreEqual(2, test.CountBits());
        Assert.AreEqual(0, test.CountLeadingZeros());
        Assert.AreEqual(0, test.CountTrailingZeros());

        test.SetBits(63, false);
        Assert.AreEqual(1, test.CountBits());
        Assert.AreEqual(63, test.CountLeadingZeros());
        Assert.AreEqual(0, test.CountTrailingZeros());
    }
}
