using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Rendering.Tests
{
    public class SparseUploaderTests
    {
        struct ExampleStruct
        {
            public int someData;
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void NoUploads()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var buffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<float>());
            var uploader = new SparseUploader(buffer);

            var tsu = uploader.Begin(1024, 1);
            uploader.EndAndCommit(tsu);

            uploader.Dispose();
            buffer.Dispose();
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void SmallUpload()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var initialData = new ExampleStruct[64];

            for (int i = 0; i < initialData.Length; ++i)
            {
                initialData[i] = new ExampleStruct { someData = 0 };
            }

            var buffer = new ComputeBuffer(initialData.Length, UnsafeUtility.SizeOf<ExampleStruct>());
            buffer.SetData(initialData);

            var uploader = new SparseUploader(buffer);

            {
                var tsu = uploader.Begin(UnsafeUtility.SizeOf<ExampleStruct>() * initialData.Length, initialData.Length);
                for (int i = 0; i < initialData.Length; ++i)
                {
                    tsu.AddUpload(new ExampleStruct { someData = i }, i * 4);
                }
                uploader.EndAndCommit(tsu);
            }

            var resultingData = new ExampleStruct[initialData.Length];
            buffer.GetData(resultingData);

            for (int i = 0; i < resultingData.Length; ++i)
            {
                Assert.AreEqual(i, resultingData[i].someData);
            }

            uploader.Dispose();
            buffer.Dispose();
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void BasicUploads()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var initialData = new ExampleStruct[1024];

            for (int i = 0; i < initialData.Length; ++i)
            {
                initialData[i] = new ExampleStruct {someData = i};
            }

            var buffer = new ComputeBuffer(initialData.Length, UnsafeUtility.SizeOf<ExampleStruct>());
            buffer.SetData(initialData);

            var uploader = new SparseUploader(buffer);

            {
                var tsu = uploader.Begin(UnsafeUtility.SizeOf<ExampleStruct>(), 1);
                tsu.AddUpload(new ExampleStruct {someData = 7}, 4);
                uploader.EndAndCommit(tsu);
            }

            var resultingData = new ExampleStruct[initialData.Length];
            buffer.GetData(resultingData);

            Assert.AreEqual(0, resultingData[0].someData);
            Assert.AreEqual(7, resultingData[1].someData);
            Assert.AreEqual(2, resultingData[2].someData);

            {
                var tsu = uploader.Begin(UnsafeUtility.SizeOf<ExampleStruct>(), 1);
                tsu.AddUpload(new ExampleStruct {someData = 13}, 8);
                uploader.EndAndCommit(tsu);
            }

            buffer.GetData(resultingData);

            Assert.AreEqual(0, resultingData[0].someData);
            Assert.AreEqual(7, resultingData[1].someData);
            Assert.AreEqual(13, resultingData[2].someData);
            Assert.AreEqual(3, resultingData[3].someData);

            uploader.Dispose();
            buffer.Dispose();
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void BigUploads()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var initialData = new ExampleStruct[4 * 1024];

            for (int i = 0; i < initialData.Length; ++i)
            {
                initialData[i] = new ExampleStruct {someData = i};
            }

            var buffer = new ComputeBuffer(initialData.Length, UnsafeUtility.SizeOf<ExampleStruct>());
            buffer.SetData(initialData);

            var uploader = new SparseUploader(buffer);


            var newData = new ExampleStruct[312];
            for (int i = 0; i < newData.Length; ++i)
            {
                newData[i] = new ExampleStruct {someData = i + 3000};
            }

            var newData2 = new ExampleStruct[316];
            for (int i = 0; i < newData2.Length; ++i)
            {
                newData2[i] = new ExampleStruct {someData = i + 4000};
            }

            var tsu = uploader.Begin(UnsafeUtility.SizeOf<ExampleStruct>() * (newData.Length + newData2.Length), 2);
            unsafe
            {
                fixed(void* ptr = newData)
                {
                    tsu.AddUpload(ptr, newData.Length * 4, 512 * 4);
                }

                fixed(void* ptr2 = newData2)
                {
                    tsu.AddUpload(ptr2, newData2.Length * 4, 1136 * 4);
                }
            }

            uploader.EndAndCommit(tsu);

            var resultingData = new ExampleStruct[initialData.Length];
            buffer.GetData(resultingData);

            for (int i = 0; i < resultingData.Length; ++i)
            {
                if (i < 512)
                    Assert.AreEqual(i, resultingData[i].someData);
                else if (i < 824)
                    Assert.AreEqual(i - 512 + 3000, resultingData[i].someData);
                else if (i < 1136)
                    Assert.AreEqual(i, resultingData[i].someData);
                else if (i < 1452)
                    Assert.AreEqual(i - 1136 + 4000, resultingData[i].someData);
                else
                    Assert.AreEqual(i, resultingData[i].someData);
            }

            uploader.Dispose();
            buffer.Dispose();
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void SplatUpload()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var initialData = new ExampleStruct[64];

            for (int i = 0; i < initialData.Length; ++i)
            {
                initialData[i] = new ExampleStruct { someData = 0 };
            }

            var buffer = new ComputeBuffer(initialData.Length, UnsafeUtility.SizeOf<ExampleStruct>());
            buffer.SetData(initialData);

            var uploader = new SparseUploader(buffer);

            {
                var tsu = uploader.Begin(UnsafeUtility.SizeOf<ExampleStruct>(), 1);
                tsu.AddUpload(new ExampleStruct { someData = 1 }, 0, 64);
                uploader.EndAndCommit(tsu);
            }

            var resultingData = new ExampleStruct[initialData.Length];
            buffer.GetData(resultingData);

            for (int i = 0; i < resultingData.Length; ++i)
            {
                Assert.AreEqual(1, resultingData[i].someData);
            }

            uploader.Dispose();
            buffer.Dispose();
        }

        struct UploadJob : IJobParallelFor
        {
            public ThreadedSparseUploader uploader;

            public void Execute(int index)
            {
                uploader.AddUpload(new ExampleStruct {someData = index}, index * 4);
            }
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void UploadFromJobs()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var initialData = new ExampleStruct[4 * 1024];
            var stride = UnsafeUtility.SizeOf<ExampleStruct>();

            for (int i = 0; i < initialData.Length; ++i)
            {
                initialData[i] = new ExampleStruct {someData = 0};
            }

            var buffer = new ComputeBuffer(initialData.Length, stride);
            buffer.SetData(initialData);

            var uploader = new SparseUploader(buffer);

            var job = new UploadJob();
            job.uploader = uploader.Begin(initialData.Length * stride, initialData.Length);
            job.Schedule(initialData.Length, 64).Complete();

            uploader.EndAndCommit(job.uploader);

            var resultingData = new ExampleStruct[initialData.Length];
            buffer.GetData(resultingData);

            for (int i = 0; i < resultingData.Length; ++i)
            {
                Assert.AreEqual(i, resultingData[i].someData);
            }

            uploader.Dispose();
            buffer.Dispose();
        }

        static void CompareFloats(float expected, float actual)
        {
            Assert.LessOrEqual(math.abs(expected - actual), 0.00001f);
        }

        static void CompareMatrices(float4x4 expected, float4x4 actual)
        {
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    CompareFloats(expected[i][j], actual[i][j]);
                }
            }
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void MatrixUploads()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var numMatrices = 1025;
            var initialData = new float4x4[numMatrices];
            var stride = UnsafeUtility.SizeOf<float4x4>();

            for (int i = 0; i < numMatrices; ++i)
            {
                initialData[i] = float4x4.identity;
            }

            var buffer = new ComputeBuffer(initialData.Length, stride);
            buffer.SetData(initialData);

            var uploader = new SparseUploader(buffer);

            {
                var tsu = uploader.Begin(numMatrices * UnsafeUtility.SizeOf<float4x4>(), 1);
                var deltaData = new NativeArray<float4x4>(numMatrices, Allocator.Temp);
                for (int i = 0; i < numMatrices; ++i)
                {
                    var trans = float4x4.Translate(new float3(i * 0.2f, -i * 0.4f, math.cos(i * math.PI * 0.02f)));
                    var rot = float4x4.EulerXYZ(i * 0.1f, math.PI * 0.5f, -i * 0.3f);
                    deltaData[i] = trans * rot;
                }

                unsafe
                {
                    tsu.AddMatrixUpload(deltaData.GetUnsafeReadOnlyPtr(), numMatrices, 0, numMatrices * 64);
                }
                uploader.EndAndCommit(tsu);

                deltaData.Dispose();
            }

            var resultingData = new float4x4[initialData.Length];
            buffer.GetData(resultingData);

            for (int i = 0; i < numMatrices; ++i)
            {
                var trans = float4x4.Translate(new float3(i * 0.2f, -i * 0.4f, math.cos(i * math.PI * 0.02f)));
                var rot = float4x4.EulerXYZ(i * 0.1f, math.PI * 0.5f, -i * 0.3f);
                var mat = trans * rot;

                CompareMatrices(mat, resultingData[i]);
            }

            uploader.Dispose();
            buffer.Dispose();
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
#endif
        public void InverseMatrixUploads()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Assert.Ignore("Skipped due to platform/computer not supporting compute shaders");
                return;
            }

            var numMatrices = 2;
            var initialData = new float4x4[numMatrices * 2];
            var stride = UnsafeUtility.SizeOf<float4x4>();

            for (int i = 0; i < numMatrices; ++i)
            {
                initialData[numMatrices * 0 + i] = float4x4.identity;
                initialData[numMatrices * 1 + i] = float4x4.identity;
            }

            var buffer = new ComputeBuffer(initialData.Length, stride);
            buffer.SetData(initialData);

            var uploader = new SparseUploader(buffer);

            {
                var tsu = uploader.Begin(numMatrices * UnsafeUtility.SizeOf<float4x4>(), 1);
                var deltaData = new NativeArray<float4x4>(numMatrices, Allocator.Temp);
                for (int i = 0; i < numMatrices; ++i)
                {
                    var trans = float4x4.Translate(new float3(i * 0.2f, -i * 0.4f, math.cos(i * math.PI * 0.02f)));
                    var rot = float4x4.EulerXYZ(i * 0.1f, math.PI * 0.5f, -i * 0.3f);
                    deltaData[i] = trans * rot;
                }

                unsafe
                {
                    tsu.AddMatrixUpload(deltaData.GetUnsafeReadOnlyPtr(), numMatrices, 0, numMatrices * 64);
                }
                uploader.EndAndCommit(tsu);

                deltaData.Dispose();
            }

            var resultingData = new float4x4[initialData.Length];
            buffer.GetData(resultingData);

            for (int i = 0; i < numMatrices; ++i)
            {
                var trans = float4x4.Translate(new float3(i * 0.2f, -i * 0.4f, math.cos(i * math.PI * 0.02f)));
                var rot = float4x4.EulerXYZ(i * 0.1f, math.PI * 0.5f, -i * 0.3f);
                var mat = trans * rot;
                var matInv = math.inverse(mat);

                CompareMatrices(mat, resultingData[numMatrices * 0 + i]);
                CompareMatrices(matInv, resultingData[numMatrices * 1 + i]);
            }

            uploader.Dispose();
            buffer.Dispose();
        }
    }
}
