using Burst.Compiler.IL.Tests.Helpers;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal partial class VectorsStatics
    {
        [TestCompiler]
        public static float Float4Zero()
        {
            return (float4.zero).x;
        }

        [TestCompiler]
        public static float Float3Zero()
        {
            return (float3.zero).x;
        }

        [TestCompiler]
        public static float Float2Zero()
        {
            return (float2.zero).x;
        }

        [TestCompiler]
        public static int Int4Zero()
        {
            return (int4.zero).x;
        }

        [TestCompiler]
        public static int Int3Zero()
        {
            return (int3.zero).x;
        }

        [TestCompiler]
        public static int Int2Zero()
        {
            return (int2.zero).x;
        }
    }
}