using Unity.Burst;
using Unity.Jobs;
using NUnit.Framework;
using Unity.Collections;

#pragma warning disable 0649

internal class NativeContainerTests_ValidateTypes : NativeContainerTests_ValidateTypesFixture
{
    enum MyTestEnum
    {
        HellWorld
    }

    struct Vector3
    {
        public float x, y, z;
    }

    struct Matrix4x4
    {
        public float m0, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12, m13, m14, m15;
    }

    [BurstCompile(CompileSynchronously = true)]
    struct StructWithVariousStructsAndValueTypesJob : IJob
    {
        [ReadOnly]
        public NativeArray<float> nativeArrayRO;
        public NativeArray<float> nativeArrayRW;

        Vector3     vec3;
        float       floatVal;
        Matrix4x4   matrix;
        int         intValue;
        byte        byteValue;
        short       shortValue;
        MyTestEnum  myEnum;

        public void Execute() {}
    }

    [BurstCompile(CompileSynchronously = true)]
    struct StructEmptyJob : IJob
    {
        public void Execute() {}
    }

    [Test]
    public void Scheduling_With_Supported_Types()
    {
        var temp1 = new NativeArray<float>(1, Allocator.TempJob);
        var temp2 = new NativeArray<float>(1, Allocator.TempJob);

        var types = new StructWithVariousStructsAndValueTypesJob();
        types.nativeArrayRO = temp1;
        types.nativeArrayRW = temp2;
        types.Schedule().Complete();
        new StructEmptyJob().Schedule().Complete();

        temp1.Dispose();
        temp2.Dispose();
    }
}
