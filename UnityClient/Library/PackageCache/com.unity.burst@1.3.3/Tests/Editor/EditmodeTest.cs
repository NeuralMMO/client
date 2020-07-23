using System;
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.TestTools;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;
using UnityEditor;
using Debug = UnityEngine.Debug;

[TestFixture]
public class EditModeTest
{
    private const int MaxIterations = 500;

//    [UnityTest]
    public IEnumerator CheckBurstJobEnabledDisabled() {
        BurstCompiler.Options.EnableBurstCompileSynchronously = true;
#if UNITY_2019_3_OR_NEWER
        foreach(var item in CheckBurstJobDisabled()) yield return item;
        foreach(var item in CheckBurstJobEnabled()) yield return item;
#else
        foreach(var item in CheckBurstJobEnabled()) yield return item;
        foreach(var item in CheckBurstJobDisabled()) yield return item;
#endif
        BurstCompiler.Options.EnableBurstCompilation = true;
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

#if UNITY_2019_3_OR_NEWER
    [UnityTest]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor)]
    public IEnumerator CheckJobWithNativeArray()
    {
        BurstCompiler.Options.EnableBurstCompileSynchronously = true;
        BurstCompiler.Options.EnableBurstCompilation = true;

        yield return null;

        var job = new BurstJobTester2.MyJobCreatingAndDisposingNativeArray()
        {
            Length = 128,
            Result = new NativeArray<int>(16, Allocator.TempJob)
        };
        var handle = job.Schedule();
        handle.Complete();
        try
        {
            Assert.AreEqual(job.Length, job.Result[0]);
        }
        finally
        {
            job.Result.Dispose();
        }
    }
#endif


#if UNITY_BURST_BUG_FUNCTION_POINTER_FIXED
    [UnityTest]
    public IEnumerator CheckBurstFunctionPointerException()
    {
        BurstCompiler.Options.EnableBurstCompileSynchronously = true;
        BurstCompiler.Options.EnableBurstCompilation = true;

        yield return null;

        using (var jobTester = new BurstJobTester())
        {
            var exception = Assert.Throws<InvalidOperationException>(() => jobTester.CheckFunctionPointer());
            StringAssert.Contains("Exception was thrown from a function compiled with Burst", exception.Message);
        }
    }
#endif

    [BurstCompile(CompileSynchronously = true)]
    private struct HashTestJob : IJob
    {
        public NativeArray<int> Hashes;

        public void Execute()
        {
            Hashes[0] = BurstRuntime.GetHashCode32<int>();
            Hashes[1] = TypeHashWrapper.GetIntHash();

            Hashes[2] = BurstRuntime.GetHashCode32<TypeHashWrapper.SomeStruct<int>>();
            Hashes[3] = TypeHashWrapper.GetGenericHash<int>();
        }
    }

    [Test]
    public static void TestTypeHash()
    {
        HashTestJob job = new HashTestJob
        {
            Hashes = new NativeArray<int>(4, Allocator.TempJob)
        };
        job.Schedule().Complete();

        var hash0 = job.Hashes[0];
        var hash1 = job.Hashes[1];

        var hash2 = job.Hashes[2];
        var hash3 = job.Hashes[3];

        job.Hashes.Dispose();

        Assert.AreEqual(hash0, hash1, "BurstRuntime.GetHashCode32<int>() has returned two different hashes");
        Assert.AreEqual(hash2, hash3, "BurstRuntime.GetHashCode32<SomeStruct<int>>() has returned two different hashes");
    }

#if UNITY_2019_3_OR_NEWER
    [UnityTest]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor)]
    public IEnumerator CheckSafetyChecksWithDomainReload()
    {
        {
            var job = new SafetyCheckJobWithDomainReload();
            {
                // Run with safety-checks true
                BurstCompiler.Options.EnableBurstSafetyChecks = true;
                job.Result = new NativeArray<int>(1, Allocator.TempJob);
                try
                {
                    var handle = job.Schedule();
                    handle.Complete();
                    Assert.AreEqual(2, job.Result[0]);
                }
                finally
                {
                    job.Result.Dispose();
                }
            }

            {
                // Run with safety-checks false
                BurstCompiler.Options.EnableBurstSafetyChecks = false;
                job.Result = new NativeArray<int>(1, Allocator.TempJob);
                bool hasException = false;
                try
                {
                    var handle = job.Schedule();
                    handle.Complete();
                    Assert.AreEqual(1, job.Result[0]);
                }
                catch
                {
                    hasException = true;
                    throw;
                }
                finally
                {
                    job.Result.Dispose();
                    if (hasException)
                    {
                        BurstCompiler.Options.EnableBurstSafetyChecks = true;
                    }
                }
            }
        }

        // Ask for domain reload
        EditorUtility.RequestScriptReload();

        // Wait for the domain reload to be completed
        yield return new WaitForDomainReload();

        {
            // The safety checks should have been disabled by the previous code
            Assert.False(BurstCompiler.Options.EnableBurstSafetyChecks);
            // Restore safety checks
            BurstCompiler.Options.EnableBurstSafetyChecks = true;
        }
    }
#endif

    [BurstCompile(CompileSynchronously = true)]
    private struct DebugLogJob : IJob
    {
        public int Value;

        public void Execute()
        {
            Debug.Log($"This is a string logged from a job with burst with the following {Value}");
        }
    }

    [Test]
    public static void TestDebugLog()
    {
        var job = new DebugLogJob
        {
            Value = 256
        };
        job.Schedule().Complete();
    }


    [BurstCompile(CompileSynchronously = true)]
    private struct SafetyCheckJobWithDomainReload : IJob
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            Result[0] = 1;
            SetResultWithSafetyChecksOnly();
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void SetResultWithSafetyChecksOnly()
        {
            Result[0] = 2;
        }
    }
}
