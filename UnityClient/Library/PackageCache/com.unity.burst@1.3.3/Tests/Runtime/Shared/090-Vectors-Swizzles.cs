using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class VectorsSwizzles
    {
        public struct StructWithFloat4
        {
            public float4 Vec4;
        }

        [TestCompiler]
        public static float SwizzleLoadLocalXyz()
        {
            var v4 = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var v3 = v4.xyz;
            return v3.x + v3.y * 10 + v3.z * 100;
        }

        [TestCompiler]
        public static float SwizzleLoadLoadlZyx()
        {
            var v4 = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var v3 = v4.zyx;
            return v3.x + v3.y * 10 + v3.z * 100;
        }

        [TestCompiler]
        public static float SwizzleStoreLocalZyx()
        {
            var v4 = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            v4.zyx = new float3(10.0f, 20.0f, 30.0f);
            return v4.x + v4.y * 10.0f + v4.z * 100.0f + v4.w * 1000.0f;
        }

        [TestCompiler]
        public static float SwizzleLoadIndirectXyz()
        {
            var localStruct = new StructWithFloat4();
            localStruct.Vec4 = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            var v3 = localStruct.Vec4.zyx;
            return v3.x + v3.y * 10 + v3.z * 100;
        }

        [TestCompiler]
        public static float SwizzleStoreIndirectXyz()
        {
            var localStruct = new StructWithFloat4();
            localStruct.Vec4 = new float4(4.0f, 5.0f, 6.0f, 7.0f);

            localStruct.Vec4.zyx = new float3(1.0f, 2.0f, 3.0f);
            var v3 = localStruct.Vec4;
            return v3.x + v3.y * 10 + v3.z * 100 + v3.w * 1000;
        }
    }
}