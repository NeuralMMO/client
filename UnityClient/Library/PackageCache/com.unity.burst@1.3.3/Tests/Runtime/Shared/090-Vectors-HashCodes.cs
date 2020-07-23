using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class VectorsHashCodes
    {
        // TODO: Add tests for Uint4/3/2, Bool4/3/2

        [TestCompiler(DataRange.Standard)]
        public static int Float4GetHashCode(ref float4 a)
        {
            return a.GetHashCode();
        }
        [TestCompiler(DataRange.Standard)]
        public static int Float3GetHashCode(ref float3 a)
        {
            return a.GetHashCode();
        }
        [TestCompiler(DataRange.Standard)]
        public static int Float2GetHashCode(ref float2 a)
        {
            return a.GetHashCode();
        }
        [TestCompiler(DataRange.Standard)]
        public static int Int4GetHashCode(ref int4 a)
        {
            return a.GetHashCode();
        }
        [TestCompiler(DataRange.Standard)]
        public static int Int3GetHashCode(ref int3 a)
        {
            return a.GetHashCode();
        }
        [TestCompiler(DataRange.Standard)]
        public static int Int2GetHashCode(ref int2 a)
        {
            return a.GetHashCode();
        }
    }
}