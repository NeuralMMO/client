using System;
using NUnit.Framework;

namespace Unity.Entities.Tests
{
#if UNITY_DOTSPLAYER
    public class StandaloneFixmeAttribute : IgnoreAttribute
    {
        public StandaloneFixmeAttribute() : base("Need to fix for Tiny.")
        {
        }
    }
#else
    public class StandaloneFixmeAttribute : Attribute
    {
    }
#endif

#if UNITY_PORTABLE_TEST_RUNNER
    internal class IgnoreInPortableTests : IgnoreAttribute
    {
        public IgnoreInPortableTests(string reason) : base(reason)
        {
        }
    }
#else
    internal class IgnoreInPortableTests : Attribute
    {
        public IgnoreInPortableTests(string reason)
        {
        }
    }
#endif
}
