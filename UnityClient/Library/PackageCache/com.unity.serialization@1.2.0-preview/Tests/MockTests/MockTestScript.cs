using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MockTestScript
    {
        // A Test behaves as an ordinary method
        [Test]
        public void ScriptSimplePasses()
        {
            Assert.IsTrue(true);
        }
    
    }
}
