using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Jobs.Tests.ManagedJobs
{
    public class JobTests : JobTestsFixture
    {
        /*
         * these two tests used to test that a job that inherited from both IJob and IJobParallelFor would work as expected
         * but that's probably crazy.
         */
        /*[Test]
        public void Scheduling()
        {
            var job = data.Schedule();
            job.Complete();
            ExpectOutputSumOfInput0And1();
        }*/


        /*[Test]

        public void Scheduling_With_Dependencies()
        {
            data.input0 = input0;
            data.input1 = input1;
            data.output = output2;
            var job1 = data.Schedule();

            // Schedule job2 with dependency against the first job
            data.input0 = output2;
            data.input1 = input2;
            data.output = output;
            var job2 = data.Schedule(job1);

            // Wait for completion
            job2.Complete();
            ExpectOutputSumOfInput0And1And2();
        }*/

        [Test]
        public void ForEach_Scheduling_With_Dependencies()
        {
            data.input0 = input0;
            data.input1 = input1;
            data.output = output2;
            var job1 = data.Schedule(output.Length, 1);

            // Schedule job2 with dependency against the first job
            data.input0 = output2;
            data.input1 = input2;
            data.output = output;
            var job2 = data.Schedule(output.Length, 1, job1);

            // Wait for completion
            job2.Complete();
            ExpectOutputSumOfInput0And1And2();
        }

        struct EmptyComputeParallelForJob : IJobParallelFor
        {
            public void Execute(int i)
            {
            }
        }

        [Test]
        public void ForEach_Scheduling_With_Zero_Size()
        {
            var test = new EmptyComputeParallelForJob();
            var job = test.Schedule(0, 1);
            job.Complete();
        }

        [Test]
#if UNITY_DOTSPLAYER_IL2CPP
        // https://unity3d.atlassian.net/browse/DOTSR-1365
        [Ignore("DOTSPLAYER_IL2CPP throws exception on AddrOfPinnedObject() in NativeArray")]
#endif
        public void Deallocate_Temp_NativeArray_From_Job()
        {
            TestDeallocateNativeArrayFromJob(Allocator.TempJob);
        }

        [Test]
#if UNITY_DOTSPLAYER_IL2CPP
        // https://unity3d.atlassian.net/browse/DOTSR-1365
        [Ignore("DOTSPLAYER_IL2CPP throws exception on AddrOfPinnedObject() in NativeArray")]
#endif
        public void Deallocate_Persistent_NativeArray_From_Job()
        {
            TestDeallocateNativeArrayFromJob(Allocator.Persistent);
        }

        private void TestDeallocateNativeArrayFromJob(Allocator label)
        {
            var tempNativeArray = new NativeArray<int>(expectedInput0, label);

            var copyAndDestroyJob = new CopyAndDestroyNativeArrayParallelForJob
            {
                input = tempNativeArray,
                output = output
            };

            // NativeArray can safely be accessed before scheduling
            Assert.AreEqual(10, tempNativeArray.Length);

            tempNativeArray[0] = tempNativeArray[0];

            var job = copyAndDestroyJob.Schedule(copyAndDestroyJob.input.Length, 1);

            job.Complete();

            Assert.AreEqual(expectedInput0, copyAndDestroyJob.output.ToArray());
        }

        public struct NestedDeallocateStruct
        {
            [DeallocateOnJobCompletion]
            public NativeArray<int> input;
        }

        public struct TestDeallocateNested : IJob
        {
            public NestedDeallocateStruct nested;

            public NativeArray<int> output;

            public void Execute()
            {
                for (int i = 0; i < nested.input.Length; ++i)
                    output[i] = nested.input[i];
            }
        }

        [Test]
        public void TestNestedDeallocateOnJobCompletion()
        {
            var tempNativeArray = new NativeArray<int>(10, Allocator.TempJob);
            var outNativeArray = new NativeArray<int>(10, Allocator.TempJob);
            for (int i = 0; i < 10; i++)
                tempNativeArray[i] = i;

            var job = new TestDeallocateNested
            {
                nested = new NestedDeallocateStruct() { input = tempNativeArray },
                output = outNativeArray
            };

            var handle = job.Schedule();
            handle.Complete();

            outNativeArray.Dispose();
            // TODO how to better assert that the tempNativeArray was actually disposed?
            Assert.Throws<System.InvalidOperationException>(() => tempNativeArray.Dispose());
        }
    }
}
