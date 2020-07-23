
using UnityBenchShared;

namespace Burst.Compiler.IL.Tests
{
    internal class TestFixed
    {
        public unsafe struct SomeStruct
        {
            public static readonly int[] Ints = new int[4] { 1, 2, 3, 4 };

            public struct OtherStruct
            {
                public int x;
            }

            public static readonly OtherStruct[] Structs = new OtherStruct[2] { new OtherStruct { x = 42 }, new OtherStruct { x = 13 } };

            public fixed ushort array[42];

            public struct Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var s = new SomeStruct();

                        for (ushort i = 0; i < 42; i++)
                        {
                            s.array[i] = i;
                        }

                        return s;
                    }
                }
            }
        }

        [TestCompiler]
        public static unsafe int ReadInts()
        {
            fixed (int* ptr = SomeStruct.Ints)
            {
                return ptr[2];
            }
        }

        [TestCompiler]
        public static unsafe int ReadIntsElement()
        {
            fixed (int* ptr = &SomeStruct.Ints[1])
            {
                return ptr[0];
            }
        }

        [TestCompiler]
        public static unsafe int ReadStructs()
        {
            fixed (SomeStruct.OtherStruct* ptr = SomeStruct.Structs)
            {
                return ptr[1].x;
            }
        }

        [TestCompiler]
        public static unsafe int ReadStructsElement()
        {
            fixed (SomeStruct.OtherStruct* ptr = &SomeStruct.Structs[1])
            {
                return ptr[0].x;
            }
        }

        [TestCompiler(typeof(SomeStruct.Provider))]
        public static unsafe ushort ReadFromFixedArray(ref SomeStruct s)
        {
            fixed (ushort* ptr = s.array)
            {
                ushort total = 0;

                for (ushort i = 0; i < 42; i++)
                {
                    total += ptr[i];
                }

                return total;
            }
        }
    }
}
