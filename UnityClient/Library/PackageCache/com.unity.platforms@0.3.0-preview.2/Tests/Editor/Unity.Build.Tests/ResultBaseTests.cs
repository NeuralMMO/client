using NUnit.Framework;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Build.Tests
{
    [TestFixture]
    class ResultBaseTests : BuildTestsBase
    {
        const string ComplexString = @"{}{{}}{0}{s}%s%%\s±@£¢¤¬¦²³¼½¾";

        class TestResult : ResultBase
        {
            public override string ToString() => Message;
        }

        [Test]
        public void LogResult_Success()
        {
            var result = new TestResult
            {
                Succeeded = true,
                Message = ComplexString
            };

            LogAssert.Expect(LogType.Log, new Regex($".{{{ComplexString.Length}}}"));
            Assert.DoesNotThrow(() => result.LogResult());
        }

        [Test]
        public void LogResult_Failure()
        {
            var result = new TestResult
            {
                Succeeded = false,
                Message = ComplexString
            };

            LogAssert.Expect(LogType.Error, new Regex($".{{{ComplexString.Length}}}"));
            Assert.DoesNotThrow(() => result.LogResult());
        }

        [Test]
        public void LogResult_Exception()
        {
            var result = new TestResult
            {
                Succeeded = false,
                Exception = new InvalidOperationException(ComplexString)
            };

            LogAssert.Expect(LogType.Exception, new Regex($".{{{ComplexString.Length}}}"));
            Assert.DoesNotThrow(() => result.LogResult());
        }
    }
}
