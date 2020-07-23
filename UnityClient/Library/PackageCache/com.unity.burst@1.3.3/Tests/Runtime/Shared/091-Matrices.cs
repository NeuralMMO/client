using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class Matrices
    {
        [TestCompiler(typeof(ReturnBox))]
        public static unsafe void TestIdentityFloat4x4(float4x4* mat)
        {
            *mat = float4x4.identity;
        }

        [TestCompiler(typeof(ReturnBox))]
        public static unsafe void TestIdentityFloat3x3(float3x3* mat)
        {
            *mat = float3x3.identity;
        }

        [TestCompiler(typeof(ReturnBox))]
        public static unsafe void TestIdentityFloat2x2(float2x2* mat)
        {
            *mat = float2x2.identity;
        }

        [TestCompiler(typeof(ReturnBox))]
        public static unsafe void TestLookAt(float4x4* mat)
        {
            *mat = float4x4.LookAt(new float3(0, 0, 1), new float3(0, 1, 0), new float3(1, 0, 0));
        }

        private static readonly float4x4 StaticMat = new float4x4(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);

        [TestCompiler(typeof(ReturnBox))]
        public static unsafe void TestStaticLoad(float4x4* mat)
        {
            *mat = StaticMat;
        }
    }
}
