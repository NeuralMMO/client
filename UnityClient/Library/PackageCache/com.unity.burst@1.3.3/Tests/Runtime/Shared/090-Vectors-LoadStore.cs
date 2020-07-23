using System.Runtime.CompilerServices;
using Burst.Compiler.IL.Tests.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class VectorsLoadStore
    {
        public struct StructWithFloat4
        {
            public float4 Vec4;
        }

        [TestCompiler(DataRange.Standard)]
        public static float TestReturnFloat4(ref float4 result)
        {
            var value = Process(result);
            return value.x + value.y;
        }

        private static float4 Process(float4 vec)
        {
            vec.x += 5;
            return vec;
        }

        [TestCompiler]
        public static float FieldLoadStoreLocalFloat4()
        {
            var v = new float4(0);
            var v1 = v.x;
            v.y = 5;
            return v.y + v1;
        }

        [TestCompiler]
        public static float FieldLoadStoreIndirectFloat4()
        {
            var localStruct = new StructWithFloat4();
            localStruct.Vec4 = new float4(1.0f, 2.0f, 3.0f, 4.0f);
            return Vectors.ConvertToFloat(localStruct.Vec4);
        }

        [TestCompiler]
        public static float FieldLoadStoreLocalByRefFloat4()
        {
            var v = new float4(2);
            ChangeFloat4(ref v);
            return Vectors.ConvertToFloat(v);
        }

        private static void ChangeFloat4(ref float4 vect)
        {
            vect.x += 5;
        }

        [TestCompiler]
        public static float FieldStoreByOutFloat4()
        {
            float4 v;
            OutputFloat4(out v);
            return Vectors.ConvertToFloat(v);
        }

        private static void OutputFloat4(out float4 float4)
        {
            float4 = new float4(1, 2, 3, 4);
        }

        [TestCompiler]
        public static float FieldLoadStoreLocalByRefFloat3()
        {
            var v = new float3(2);
            ChangeFloat3(ref v);
            return Vectors.ConvertToFloat(v);
        }

        private static void ChangeFloat3(ref float3 vect)
        {
            vect.z += 5;
        }


        [TestCompiler]
        public static float FieldLoadStoreByRefFloat3()
        {
            var v = new float3(2);
            float3 result;
            LoadAndChangeFloat3(ref v, out result);
            return Vectors.ConvertToFloat(result);
        }

        private static void LoadAndChangeFloat3(ref float3 vect, out float3 result)
        {
            var local = vect;
            local.z += 5;
            result = local;
        }
    }
}