using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    [TestFixture]
    internal partial class VectorsSizeOfs
    {
        [TestCompiler]
        public static int Float4()
        {
            return UnsafeUtility.SizeOf<float4>();
        }
        [TestCompiler]
        public static int Float3()
        {
            return UnsafeUtility.SizeOf<float3>();
        }
        [TestCompiler]
        public static int Float2()
        {
            return UnsafeUtility.SizeOf<float2>();
        }
        [TestCompiler]
        public static int UInt4()
        {
            return UnsafeUtility.SizeOf<uint4>();
        }
        [TestCompiler]
        public static int UInt3()
        {
            return UnsafeUtility.SizeOf<uint3>();
        }
        [TestCompiler]
        public static int UInt2()
        {
            return UnsafeUtility.SizeOf<uint2>();
        }
        [TestCompiler]
        public static int Int4()
        {
            return UnsafeUtility.SizeOf<int4>();
        }
        [TestCompiler]
        public static int Int3()
        {
            return UnsafeUtility.SizeOf<int3>();
        }
        [TestCompiler]
        public static int Int2()
        {
            return UnsafeUtility.SizeOf<int2>();
        }
        [TestCompiler]
        public static int Bool4()
        {
            return UnsafeUtility.SizeOf<bool4>();
        }
        [TestCompiler]
        public static int Bool3()
        {
            return UnsafeUtility.SizeOf<bool3>();
        }
        [TestCompiler]
        public static int Bool2()
        {
            return UnsafeUtility.SizeOf<bool2>();
        }
    }
}