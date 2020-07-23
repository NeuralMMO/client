using NUnit.Framework;

namespace Unity.Entities.Editor.Tests
{
    class StringExtensionsTests
    {
        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Simple")]
        [TestCase("  Simple")]
        [TestCase("Simple  ")]
        [TestCase("`Simple")]
        [TestCase("`String`")]
        public void CanSingleQuoteAString(string value)
        {
            var trimmedValue = value.Trim('\'');
            Assert.That(value.SingleQuoted(), Is.EqualTo('\'' + trimmedValue + '\''));
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Simple")]
        [TestCase("  Simple")]
        [TestCase("Simple  ")]
        [TestCase("`Simple")]
        [TestCase("`String`")]
        [TestCase("\"String`")]
        [TestCase("\"String\"")]
        public void CanDoubleQuoteAString(string value)
        {
            var trimmedValue = value.Trim('\"');
            Assert.That(value.DoubleQuoted(), Is.EqualTo("\"" + trimmedValue + "\""));
        }

        [Test]
        [TestCase("", "")]
        [TestCase("", "Key")]
        [TestCase("Value", "")]
        [TestCase("Value", "Key")]
        public void CanHyperLinkAString(string value, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Assert.That(value.ToHyperLink(string.Empty), Is.EqualTo($"<a>{value}</a>"));
            }
            else
            {
                Assert.That(value.ToHyperLink(key), Is.EqualTo($"<a {key}={value.DoubleQuoted()}>{value}</a>"));
            }
        }

        [Test]
        [TestCase("Simple", "Simple")]
        [TestCase("Simple With Space", "Simple_With_Space")]
        [TestCase("234v54.345", "234v54_345")]
        [TestCase("#Hashtag", "_Hashtag")]
        public void CanConvertStringToIdentifier(string value, string expected)
        {
            Assert.That(value.ToIdentifier(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase("Simple", "Simple")]
        [TestCase("/Simple", "/Simple")]
        [TestCase("Simple/", "Simple/")]
        [TestCase("\\Simple", "/Simple")]
        [TestCase("Simple\\", "Simple/")]
        [TestCase("Simple/Simple/Simple", "Simple/Simple/Simple")]
        [TestCase("Simple/Simple\\Simple", "Simple/Simple/Simple")]
        public void CanConvertStringToForwardSlashes(string value, string expected)
        {
            Assert.That(value.ToForwardSlash(), Is.EqualTo(expected));
        }
    }
}
