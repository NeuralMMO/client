using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityBenchShared;
using static Unity.Burst.CompilerServices.Aliasing;

namespace Burst.Compiler.IL.Tests
{
    internal class Aliasing
    {
        public unsafe struct NoAliasField
        {
            [NoAlias]
            public int* ptr1;

            [NoAlias]
            public int* ptr2;

            public void Compare(ref NoAliasField other)
            {
                // Check that we can definitely alias with another struct of the same type as us.
                ExpectAliased(in this, in other);
            }

            public void Compare(ref ContainerOfManyNoAliasFields other)
            {
                // Check that we can definitely alias with another struct which contains the same type as ourself.
                ExpectAliased(in this, in other);
            }

            public class Provider : IArgumentProvider
            {
                public object Value => new NoAliasField { ptr1 = null, ptr2 = null };
            }
        }

        public unsafe struct ContainerOfManyNoAliasFields
        {
            public NoAliasField s0;

            public NoAliasField s1;

            [NoAlias]
            public NoAliasField s2;

            [NoAlias]
            public NoAliasField s3;

            public class Provider : IArgumentProvider
            {
                public object Value => new ContainerOfManyNoAliasFields { s0 = new NoAliasField { ptr1 = null, ptr2 = null }, s1 = new NoAliasField { ptr1 = null, ptr2 = null }, s2 = new NoAliasField { ptr1 = null, ptr2 = null }, s3 = new NoAliasField { ptr1 = null, ptr2 = null } };
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Union
        {
            [FieldOffset(0)]
            public ulong a;

            [FieldOffset(1)]
            public int b;

            [FieldOffset(5)]
            public float c;

            public class Provider : IArgumentProvider
            {
                public object Value => new Union { a = 4242424242424242, b = 13131313, c = 42.0f };
            }
        }

        public unsafe struct LinkedList
        {
            public LinkedList* next;

            public class Provider : IArgumentProvider
            {
                public object Value => new LinkedList { next = null };
            }
        }

        [NoAlias]
        public unsafe struct NoAliasWithContentsStruct
        {
            public void* ptr0;
            public void* ptr1;

            public class Provider : IArgumentProvider
            {
                public object Value => new NoAliasWithContentsStruct { ptr0 = null, ptr1 = null };
            }
        }

        [NoAlias]
        public unsafe struct DoesAliasWithSubStructPointersStruct : IDisposable
        {
            public NoAliasWithContentsStruct* s;
            public void* ptr;

            public class Provider : IArgumentProvider
            {
                public object Value
                {
                    get
                    {
                        var noAliasSubStruct = (NoAliasWithContentsStruct*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<NoAliasWithContentsStruct>(), UnsafeUtility.AlignOf<NoAliasWithContentsStruct>(), Allocator.Temp);
                        noAliasSubStruct->ptr0 = null;
                        noAliasSubStruct->ptr1 = null;

                        var s = new DoesAliasWithSubStructPointersStruct { s = noAliasSubStruct, ptr = null };

                        return s;
                    }
                }
            }

            public void Dispose()
            {
                UnsafeUtility.Free(s, Allocator.Temp);
            }
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckNoAliasFieldWithItself(ref NoAliasField s)
        {
            // Check that they correctly alias with themselves.
            ExpectAliased(s.ptr1, s.ptr1);
            ExpectAliased(s.ptr2, s.ptr2);
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckNoAliasFieldWithAnotherPointer(ref NoAliasField s)
        {
            // Check that they do not alias each other because of the [NoAlias] on the ptr1 field above.
            ExpectNotAliased(s.ptr1, s.ptr2);
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckNoAliasFieldWithNull(ref NoAliasField s)
        {
            // Check that comparing a pointer with null is no alias.
            ExpectNotAliased(s.ptr1, null);
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckAliasFieldWithNull(ref NoAliasField s)
        {
            // Check that comparing a pointer with null is no alias.
            ExpectNotAliased(s.ptr2, null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void NoAliasInfoSubFunctionAlias(int* a, int* b)
        {
            ExpectAliased(a, b);
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckNoAliasFieldSubFunctionAlias(ref NoAliasField s)
        {
            NoAliasInfoSubFunctionAlias(s.ptr1, s.ptr1);
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckCompareWithItself(ref NoAliasField s)
        {
            s.Compare(ref s);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void AliasInfoSubFunctionNoAlias([NoAlias] int* a, int* b)
        {
            ExpectNotAliased(a, b);
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckNoAliasFieldSubFunctionWithNoAliasParameter(ref NoAliasField s)
        {
            AliasInfoSubFunctionNoAlias(s.ptr1, s.ptr1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe void AliasInfoSubFunctionTwoSameTypedStructs(ref NoAliasField s0, ref NoAliasField s1)
        {
            // Check that they do not alias within their own structs.
            ExpectNotAliased(s0.ptr1, s0.ptr2);
            ExpectNotAliased(s1.ptr1, s1.ptr2);

            // But that they do alias across structs.
            ExpectAliased(s0.ptr1, s1.ptr1);
            ExpectAliased(s0.ptr1, s1.ptr2);
            ExpectAliased(s0.ptr2, s1.ptr1);
            ExpectAliased(s0.ptr2, s1.ptr2);

        }

        [TestCompiler(typeof(NoAliasField.Provider), typeof(NoAliasField.Provider))]
        public static unsafe void CheckNoAliasFieldAcrossTwoSameTypedStructs(ref NoAliasField s0, ref NoAliasField s1)
        {
            AliasInfoSubFunctionTwoSameTypedStructs(ref s0, ref s1);
        }

        [TestCompiler(4, 13)]
        public static void CheckNoAliasRefs([NoAlias] ref int a, ref int b)
        {
            ExpectAliased(in a, in a);
            ExpectAliased(in b, in b);
            ExpectNotAliased(in a, in b);
        }

        [TestCompiler(4, 13.53f)]
        public static void CheckNoAliasRefsAcrossTypes([NoAlias] ref int a, ref float b)
        {
            ExpectNotAliased(in a, in b);
        }

        [TestCompiler(typeof(Union.Provider))]
        public static void CheckNoAliasRefsInUnion(ref Union u)
        {
            ExpectAliased(in u.a, in u.b);
            ExpectAliased(in u.a, in u.c);
            ExpectNotAliased(in u.b, in u.c);
        }

        [TestCompiler(typeof(ContainerOfManyNoAliasFields.Provider))]
        public static unsafe void CheckNoAliasOfSubStructs(ref ContainerOfManyNoAliasFields s)
        {
            // Since ptr1 and ptr2 have [NoAlias], they do not alias within the same struct instance.
            ExpectNotAliased(s.s0.ptr1, s.s0.ptr2);
            ExpectNotAliased(s.s1.ptr1, s.s1.ptr2);
            ExpectNotAliased(s.s2.ptr1, s.s2.ptr2);
            ExpectNotAliased(s.s3.ptr1, s.s3.ptr2);

            // Across s0 and s1 their pointers can alias each other though.
            ExpectAliased(s.s0.ptr1, s.s1.ptr1);
            ExpectAliased(s.s0.ptr1, s.s1.ptr2);
            ExpectAliased(s.s0.ptr2, s.s1.ptr1);
            ExpectAliased(s.s0.ptr2, s.s1.ptr2);

            // Also s2 can alias with s0 and s1 (because they do not have [NoAlias]).
            ExpectAliased(s.s2.ptr1, s.s0.ptr1);
            ExpectAliased(s.s2.ptr1, s.s0.ptr2);
            ExpectAliased(s.s2.ptr2, s.s1.ptr1);
            ExpectAliased(s.s2.ptr2, s.s1.ptr2);

            // Also s3 can alias with s0 and s1 (because they do not have [NoAlias]).
            ExpectAliased(s.s3.ptr1, s.s0.ptr1);
            ExpectAliased(s.s3.ptr1, s.s0.ptr2);
            ExpectAliased(s.s3.ptr2, s.s1.ptr1);
            ExpectAliased(s.s3.ptr2, s.s1.ptr2);

            // But s2 and s3 cannot alias each other (they both have [NoAlias]).
            ExpectNotAliased(s.s2.ptr1, s.s3.ptr1);
            ExpectNotAliased(s.s2.ptr1, s.s3.ptr2);
            ExpectNotAliased(s.s2.ptr2, s.s3.ptr1);
            ExpectNotAliased(s.s2.ptr2, s.s3.ptr2);
        }

        [TestCompiler(typeof(ContainerOfManyNoAliasFields.Provider))]
        public static unsafe void CheckNoAliasFieldCompareWithParentStruct(ref ContainerOfManyNoAliasFields s)
        {
            s.s0.Compare(ref s);
            s.s1.Compare(ref s);
            s.s2.Compare(ref s);
            s.s3.Compare(ref s);
        }

        [TestCompiler(typeof(LinkedList.Provider))]
        public static unsafe void CheckStructPointerOfSameTypeInStruct(ref LinkedList l)
        {
            ExpectAliased(in l, l.next);
        }

        [TestCompiler(typeof(NoAliasWithContentsStruct.Provider))]
        public static unsafe void CheckStructWithNoAlias(ref NoAliasWithContentsStruct s)
        {
            // Since NoAliasWithContentsStruct has [NoAlias] on the struct definition, it cannot alias with any pointers within the struct.
            ExpectNotAliased(in s, s.ptr0);
            ExpectNotAliased(in s, s.ptr1);
        }

        [TestCompiler(typeof(DoesAliasWithSubStructPointersStruct.Provider))]
        public static unsafe void CheckStructWithNoAliasAndSubStructs(ref DoesAliasWithSubStructPointersStruct s)
        {
            // Since DoesAliasWithSubStructPointersStruct has [NoAlias] on the struct definition, it cannot alias with any pointers within the struct.
            ExpectNotAliased(in s, s.s);
            ExpectNotAliased(in s, s.ptr);

            // s.s is a [NoAlias] struct, so it shouldn't alias with pointers within it.
            ExpectNotAliased(s.s, s.s->ptr0);
            ExpectNotAliased(s.s, s.s->ptr1);

            // But we don't know whether s.s and s.ptr alias.
            ExpectAliased(s.s, s.ptr);

            // And we cannot assume that s does not alias with the sub-pointers of s.s.
            ExpectAliased(in s, s.s->ptr0);
            ExpectAliased(in s, s.s->ptr1);
        }

        private unsafe struct AliasingWithSelf
        {
            public AliasingWithSelf* ptr;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void CheckAlias()
            {
                ExpectAliased(in this, ptr);
            }
        }

        [TestCompiler]
        public static unsafe void CheckAliasingWithSelf()
        {
            var s = new AliasingWithSelf { ptr = null };
            s.ptr = (AliasingWithSelf*) &s;
            s.CheckAlias();
        }

        private unsafe struct AliasingWithHiddenSelf
        {
            public void* ptr;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void CheckAlias()
            {
                ExpectAliased(in this, ptr);
            }
        }

        [TestCompiler]
        public static unsafe void CheckAliasingWithHiddenSelf()
        {
            var s = new AliasingWithHiddenSelf { ptr = null };
            s.ptr = &s;
            s.CheckAlias();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [return: NoAlias]
        private static unsafe int* NoAliasReturn(int size)
        {
            return (int*)UnsafeUtility.Malloc(size, 16, Allocator.Temp);
        }

        [TestCompiler(typeof(NoAliasField.Provider))]
        public static unsafe void CheckNoAliasReturn(ref NoAliasField s)
        {
            int* ptr1 = NoAliasReturn(40);
            int* ptr2 = NoAliasReturn(4);
            int* ptr3 = ptr2 + 4;
            byte* ptr4 = (byte*)ptr3 + 1;

            // Obviously it still aliases with itself even it we bitcast.
            ExpectAliased((char*)ptr1 + 4, ptr1 + 1);

            // We know that both allocations can't point to the same memory as
            // they are derived from Malloc!).
            ExpectNotAliased(ptr1, ptr2);

            // Since ptr3 derives from ptr2 it cannot alias with ptr1.
            ExpectNotAliased(ptr3, ptr1);

            // And the derefenced memory locations at ptr3 and ptr2 cannot alias
            // since ptr3 does not overlap the allocation in ptr2.
            ExpectNotAliased(in *ptr3, in *ptr2);

            // The pointers pt4 and ptr3 have overlapping ranges so they do alias.
            ExpectAliased(in *ptr4, in *ptr3);

            // The pointers cannot alias with anything else too!
            ExpectNotAliased(ptr1, in s);
            ExpectNotAliased(ptr1, s.ptr1);
            ExpectNotAliased(ptr1, s.ptr2);
            ExpectNotAliased(ptr2, in s);
            ExpectNotAliased(ptr2, s.ptr1);
            ExpectNotAliased(ptr2, s.ptr2);
            ExpectNotAliased(ptr3, in s);
            ExpectNotAliased(ptr3, s.ptr1);
            ExpectNotAliased(ptr3, s.ptr2);
            ExpectNotAliased(ptr4, in s);
            ExpectNotAliased(ptr4, s.ptr1);
            ExpectNotAliased(ptr4, s.ptr2);

            UnsafeUtility.Free(ptr1, Allocator.Temp);
            UnsafeUtility.Free(ptr2, Allocator.Temp);
        }

        [TestCompiler]
        public static unsafe void CheckMallocIsNoAlias()
        {
            int* ptr1 = (int*)UnsafeUtility.Malloc(sizeof(int) * 4, 16, Allocator.Temp);
            int* ptr2 = (int*)UnsafeUtility.Malloc(sizeof(int), 16, Allocator.Temp);

            ExpectNotAliased(ptr1, ptr2);

            UnsafeUtility.Free(ptr1, Allocator.Temp);
            UnsafeUtility.Free(ptr2, Allocator.Temp);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [return: NoAlias]
        private static unsafe int* BumpAlloc(int* alloca)
        {
            int location = alloca[0]++;
            return alloca + location;
        }

        [TestCompiler]
        public static unsafe void CheckBumpAllocIsNoAlias()
        {
            int* alloca = stackalloc int[128];

            // Store our size at the start of the alloca.
            alloca[0] = 1;

            int* ptr1 = BumpAlloc(alloca);
            int* ptr2 = BumpAlloc(alloca);

            // Our bump allocator will never return the same address twice.
            ExpectNotAliased(ptr1, ptr2);
        }

        [TestCompiler(42, 13, 56)]
        public static unsafe void CheckInRefOut(in int a, ref int b, out int c)
        {
            c = 42;

            // They obviously alias with themselves.
            ExpectAliased(in a, in a);
            ExpectAliased(in b, in b);
            ExpectAliased(in c, in c);

            // And alias with each other too.
            ExpectAliased(in a, in b);
            ExpectAliased(in a, in c);
            ExpectAliased(in b, in c);
        }

        [TestCompiler(42, 13)]
        public static unsafe void CheckOutOut(out int a, out int b)
        {
            a = 56;
            b = -4;

            ExpectAliased(in a, in b);
        }
    }
}
