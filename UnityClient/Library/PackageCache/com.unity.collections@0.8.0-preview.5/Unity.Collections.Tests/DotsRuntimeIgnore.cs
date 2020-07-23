using System;
using NUnit.Framework;

namespace Unity.Collections.Tests
{
#if UNITY_DOTSPLAYER
    internal class DotsRuntimeIgnore : IgnoreAttribute
    {
        public DotsRuntimeIgnore() : base("Need to fix for DotsRuntime.")
        {
        }
    }
#else
    internal class DotsRuntimeIgnore : Attribute
    {
    }
#endif
}
