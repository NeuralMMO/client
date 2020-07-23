using System;
using NUnit.Framework;

namespace Unity.Entities.Editor.Tests
{
    class TimeSpanExtensionsTests
    {
        [Test]
        [TestCase(0, 0, 0, 0, 0, 0, "0ms")]
        [TestCase(0, 0, 0, 0, 1, 0, "1ms")]
        [TestCase(0, 0, 0, 0, 999, 0, "999ms")]
        [TestCase(0, 0, 0, 0, 1000, 0, "1s")]
        [TestCase(0, 0, 0, 1, 5, 0, "1s")]
        [TestCase(0, 0, 0, 59, 5, 0, "59s")]
        [TestCase(0, 0, 0, 60, 0, 0, "1m")]
        [TestCase(0, 0, 1, 60, 0, 0, "2m")]
        [TestCase(0, 0, 60, 0, 0, 0, "1h")]
        [TestCase(0, 1, 0, 0, 0, 0, "1h")]
        [TestCase(0, 24, 0, 0, 0, 0, "1d")]
        [TestCase(1, 0, 0, 0, 0, 0, "1d")]
        [TestCase(0, 0, 0, 2, 230, 0, "2s")]
        [TestCase(0, 0, 0, 2, 230, 1, "2.2s")]
        [TestCase(0, 0, 0, 2, 230, 2, "2.23s")]
        [TestCase(0, 0, 0, 2, 234, 3, "2.234s")]
        [TestCase(0, 0, 0, 2, 750, 0, "3s")]
        [TestCase(0, 0, 0, 2, 750, 1, "2.8s")]
        [TestCase(0, 0, 0, 2, 750, 2, "2.75s")]
        [TestCase(0, 0, 0, 2, 750, 3, "2.750s")]
        public void ShortStringForTimeSpan(int days, int hours, int minutes, int seconds, int milliseconds, int decimals, string expected)
        {
            var span = new TimeSpan(days, hours, minutes, seconds, milliseconds);
            Assert.That(span.ToShortString((uint)decimals), Is.EqualTo(expected));
        }
    }
}
