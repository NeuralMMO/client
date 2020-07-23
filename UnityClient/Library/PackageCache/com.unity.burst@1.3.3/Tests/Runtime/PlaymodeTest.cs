using System.Collections;
using NUnit.Framework;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.TestTools;
using System;

[TestFixture]
public class PlaymodeTest
{
//    [UnityTest]
    public IEnumerator CheckBurstJobEnabledDisabled()
    {
        BurstCompiler.Options.EnableBurstCompileSynchronously = true;
#if UNITY_2019_3_OR_NEWER
        foreach(var item in CheckBurstJobDisabled()) yield return item;
#endif
        foreach(var item in CheckBurstJobEnabled()) yield return item;
    }

    private IEnumerable CheckBurstJobEnabled()
    {
        BurstCompiler.Options.EnableBurstCompilation = true;

        yield return null;

        using (var jobTester = new BurstJobTester2())
        {
            var result = jobTester.Calculate();
            Assert.AreNotEqual(0.0f, result);
        }
    }

    private IEnumerable CheckBurstJobDisabled()
    {
        BurstCompiler.Options.EnableBurstCompilation = false;

        yield return null;

        using (var jobTester = new BurstJobTester2())
        {
            var result = jobTester.Calculate();
            Assert.AreEqual(0.0f, result);
        }
    }
}
