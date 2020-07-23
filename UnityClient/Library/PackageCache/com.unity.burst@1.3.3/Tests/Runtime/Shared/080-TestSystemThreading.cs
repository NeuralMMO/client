using System;
using System.Threading;
using UnityBenchShared;

namespace Burst.Compiler.IL.Tests
{
    /// <summary>
    /// Tests of the <see cref="System.Threading"/> functions.
    /// </summary>
    internal class TestSystemThreading
    {
        [TestCompiler]
        public static void TestMemoryBarrier()
        {
            Thread.MemoryBarrier();
        }

        [TestCompiler]
        public static int TestReadBool()
        {
            var data = false;
            return (Volatile.Read(ref data) ? 1 : 0) + (Volatile.Read(ref data) ? 1 : 0);
        }

        [TestCompiler((byte)42)]
        public static int TestReadByte(ref byte data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler((sbyte)42)]
        public static int TestReadSByte(ref sbyte data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler((short)42)]
        public static int TestReadShort(ref short data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler((ushort)42)]
        public static int TestReadUShort(ref ushort data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler(42)]
        public static int TestReadInt(ref int data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler(42u)]
        public static uint TestReadUInt(ref uint data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler((long)42)]
        public static long TestReadLong(ref long data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler((ulong)42)]
        public static ulong TestReadULong(ref ulong data)
        {
            return Volatile.Read(ref data) + Volatile.Read(ref data);
        }

        [TestCompiler(42.0f)]
        public static float TestReadFloat(ref float data)
        {
            return Volatile.Read(ref data);
        }

        [TestCompiler(42.0)]
        public static double TestReadDouble(ref double data)
        {
            return Volatile.Read(ref data);
        }

        public struct UIntPtrProvider : IArgumentProvider
        {
            public object Value => UIntPtr.Zero;
        }

        [TestCompiler(typeof(UIntPtrProvider))]
        public static UIntPtr TestReadUIntPtr(ref UIntPtr data)
        {
            return Volatile.Read(ref data);
        }

        [TestCompiler]
        public static int TestWriteBool()
        {
            var data = false;
            Volatile.Write(ref data, true);
            return data ? 1 : 0;
        }

        [TestCompiler((byte)42)]
        public static int TestWriteByte(ref byte data)
        {
            var result = data;
            Volatile.Write(ref data, 1);
            return result + data;
        }

        [TestCompiler((sbyte)42)]
        public static int TestWriteSByte(ref sbyte data)
        {
            var result = data;
            Volatile.Write(ref data, 2);
            return result + data;
        }

        [TestCompiler((short)42)]
        public static int TestWriteShort(ref short data)
        {
            var result = data;
            Volatile.Write(ref data, 3);
            return result + data;
        }

        [TestCompiler((ushort)42)]
        public static int TestWriteUShort(ref ushort data)
        {
            var result = data;
            Volatile.Write(ref data, 4);
            return result + data;
        }

        [TestCompiler(42)]
        public static int TestWriteInt(ref int data)
        {
            var result = data;
            Volatile.Write(ref data, 5);
            return result + data;
        }

        /* Workaround IL2CPP bug https://fogbugz.unity3d.com/f/cases/1240541/
        [TestCompiler(42u)]
        public static uint TestWriteUInt(ref uint data)
        {
            var result = data;
            Volatile.Write(ref data, 6);
            return result + data;
        }
        */

        [TestCompiler((long)42)]
        public static long TestWriteLong(ref long data)
        {
            var result = data;
            Volatile.Write(ref data, 7);
            return result + data;
        }

        /* Workaround IL2CPP bug https://fogbugz.unity3d.com/f/cases/1240541/
        [TestCompiler((ulong)42)]
        public static ulong TestWriteULong(ref ulong data)
        {
            var result = data;
            Volatile.Write(ref data, 8);
            return result + data;
        }
        */

        [TestCompiler(42.0f)]
        public static float TestWriteFloat(ref float data)
        {
            var result = data;
            Volatile.Write(ref data, 9);
            return result + data;
        }

        [TestCompiler(42.0)]
        public static double TestWriteDouble(ref double data)
        {
            var result = data;
            Volatile.Write(ref data, 10);
            return result + data;
        }

        [TestCompiler(typeof(UIntPtrProvider))]
        public static UIntPtr TestWriteUIntPtr(ref UIntPtr data)
        {
            var result = data;
            Volatile.Write(ref data, new UIntPtr(11));
            return result;
        }
    }
}
