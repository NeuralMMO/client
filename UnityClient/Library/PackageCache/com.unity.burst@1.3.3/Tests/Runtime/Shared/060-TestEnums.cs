using System;
using NUnit.Framework;

namespace Burst.Compiler.IL.Tests
{
    /// <summary>
    /// Test with enums.
    /// </summary>
    internal partial class TestEnums
    {
        [System.Flags]
        public enum MyEnum
        {
            Hello = 5,
            Something = 10
        }

        [TestCompiler]
        public static int test_enum_cast_to_int()
        {
            MyEnum value = MyEnum.Hello;
            return (int)value;
        }

        [TestCompiler(MyEnum.Hello)]
        [TestCompiler(MyEnum.Something)]
        public static int test_enum_compare(MyEnum value)
        {
            if (value == MyEnum.Hello)
                return 0;
            else
                return 1;
        }

        //[TestCompiler(typeof(StructContainingEnumProvider))]
        //public static int test_enum_in_struct(ref StructContainingEnum myStruct)
        //{
        //    return myStruct.intValue + (int)myStruct.value;
        //}

        [TestCompiler(MyEnum.Hello)]
        [TestCompiler(MyEnum.Something)]
        [Ignore("Failure")]
        public static int test_enum_has_flag(MyEnum value)
        {
            return value.HasFlag(MyEnum.Hello) ? 3 : 4;
        }

        [TestCompiler(MyEnum.Hello)]
        [TestCompiler(MyEnum.Something)]
        public static int test_enum_and_mask(MyEnum value)
        {
            return (value & MyEnum.Hello) != 0 ? 3 : 4;
        }

        [TestCompiler(1)]
        [TestCompiler(2)]
        public static int TestEnumSwitchCase(IntPtr value)
        {
            var enumValue = (SmallEnum) value.ToInt32();
            // Need at least 3 cases to generate a proper switch
            // otherwise Roslyn will generate an if/else
            switch (enumValue)
            {
                case SmallEnum.One:
                    return 7;
                case SmallEnum.Two:
                    return 8;
                case SmallEnum.Three:
                    return 9;
                default:
                    return 10;
            }
        }

        private static int GetToInt32(int value)
        {
            return value;
        }

        [TestCompiler]
        public static int test_enum_sizeof_small_enum()
        {
            return sizeof(SmallEnum);
        }
        

        [TestCompiler(SmallEnum.Three)]
        public static int test_enum_sizeof_small_enum_in_struct_access(SmallEnum value)
        {
            var s = new MySmallEnumStruct
            {
                a = value,
                b = value,
                c = value
            };
            return (int)s.a + (int)s.b + (int)s.c;
        }

        public struct StructContainingEnum
        {
            public MyEnum value;
            public int intValue;
        }


        public enum SmallEnum : byte
        {
            One,
            Two,
            Three
        }


        public struct MySmallEnumStruct
        {
            public SmallEnum a;
            public SmallEnum b;
            public SmallEnum c;
            public SmallEnum d;
        }



        //private struct StructContainingEnumProvider : IArgumentProvider
        //{
        //    public object[] Arguments
        //    {
        //        get
        //        {
        //            var value = new StructContainingEnum();
        //            value.intValue = 5;
        //            value.value = MyEnum.Something;
        //            return new object[] { value };
        //        }
        //    }
        //}
    }
}
