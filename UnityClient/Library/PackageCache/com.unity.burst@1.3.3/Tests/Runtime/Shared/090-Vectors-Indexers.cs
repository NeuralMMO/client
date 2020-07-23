using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class VectorsIndexers
    {
        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(2)]
        [TestCompiler(3)]
        public static float Float4_get_IndexerLocal(int i)
        {
            var vector = new float4(5.0f, 6.0f, 7.0f, 8.0f);
            return vector[i];
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float4_get_IndexerByRef(ref float4 vector)
        {
            return vector[0] + vector[2];
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(2)]
        [TestCompiler(3)]
        public static float Float4_set_IndexerLocal(int i)
        {
            var vector = new float4(0.0f);
            vector[i] = 2.0f * i;
            return vector[0] + vector[2];
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float4_set_IndexerByRef(ref float4 vector)
        {
            vector[0] = 10.0f;
            vector[2] = 15.0f;
            return vector[0] + vector[2];
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(2)]
        public static float Float3_get_IndexerLocal(int i)
        {
            var vector = new float3(5.0f, 6.0f, 7.0f);
            return vector[i];
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float3_get_IndexerByRef(ref float3 vector)
        {
            return vector[0] + vector[2];
        }

        [TestCompiler(0)]
        [TestCompiler(1)]
        [TestCompiler(2)]
        public static float Float3_set_IndexerLocal(int i)
        {
            var vector = new float3(0.0f);
            vector[i] = 2.0f * i;
            return vector[0] + vector[2];
        }

        [TestCompiler(DataRange.Standard)]
        public static float Float3_set_IndexerByRef(ref float3 vector)
        {
            vector[0] = 10.0f;
            vector[2] = 15.0f;
            return vector[0] + vector[2];
        }

        [TestCompiler(DataRange.Standard)]
        public static int Bool_set_Indexer_Indirect(ref float4 vec)
        {
            bool4 result = false;
            for (int i = 0; i < 4; i++)
            {
                result[i] = CheckVector(vec[i]);
            }

            return Vectors.ConvertToInt(result);
        }

        public static bool CheckVector(float value)
        {
            return value < 10.0f;
        }
    }
}