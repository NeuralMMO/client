using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst.CompilerServices;
#if !NET_DOTS
using System.Reflection;
#endif

namespace Unity.Collections
{
    public static class CollectionHelper
    {
        public const int CacheLineSize = JobsUtility.CacheLineSize;

        [StructLayout(LayoutKind.Explicit)]
        internal struct LongDoubleUnion
        {
            [FieldOffset(0)]
            public long longValue;
            [FieldOffset(0)]
            public double doubleValue;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Log2Floor(int value)
        {
            return 31 - math.lzcnt((uint)value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int Log2Ceil(int value)
        {
            return 32 - math.lzcnt((uint)value - 1);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckIntPositivePowerOfTwo(int value)
        {
            var valid = (value > 0) && ((value & (value - 1)) == 0);
            if (!valid)
            {
                throw new ArgumentException("Alignment requested: {value} is not a non-zero, positive power of two.");
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="size"></param>
        /// <param name="alignmentPowerOfTwo"></param>
        /// <returns></returns>
        public static int Align(int size, int alignmentPowerOfTwo)
        {
            if (alignmentPowerOfTwo == 0)
                return size;

            CheckIntPositivePowerOfTwo(alignmentPowerOfTwo);

            return (size + alignmentPowerOfTwo - 1) & ~(alignmentPowerOfTwo - 1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        /// <param name="alignmentPowerOfTwo"></param>
        /// <returns></returns>
        public static unsafe bool IsAligned(void* p, int alignmentPowerOfTwo)
        {
            CheckIntPositivePowerOfTwo(alignmentPowerOfTwo);
            return ((ulong)p & ((ulong)alignmentPowerOfTwo - 1)) == 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="alignmentPowerOfTwo"></param>
        /// <returns></returns>
        public static unsafe bool IsAligned(ulong offset, int alignmentPowerOfTwo)
        {
            CheckIntPositivePowerOfTwo(alignmentPowerOfTwo);
            return (offset & ((ulong)alignmentPowerOfTwo - 1)) == 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsPowerOfTwo(int value)
        {
            return (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Returns hash value of memory block. Function is using djb2 (non-cryptographic hash).
        /// </summary>
        public static unsafe uint Hash(void* pointer, int bytes)
        {
            // djb2 - Dan Bernstein hash function
            // http://web.archive.org/web/20190508211657/http://www.cse.yorku.ca/~oz/hash.html
            byte* str = (byte*)pointer;
            ulong hash = 5381;
            while (bytes > 0)
            {
                ulong c = str[--bytes];
                hash = ((hash << 5) + hash) + c;
            }
            return (uint)hash;
        }

        /// <summary>
        /// Throws exeception if type T is managed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [BurstDiscard] // Must use BurstDiscard because UnsafeUtility.IsUnmanaged is not burstable.
        public static void CheckIsUnmanaged<T>()
        {
            if (!UnsafeUtility.IsValidNativeContainerElementType<T>())
                throw new ArgumentException($"{typeof(T)} used in native collection is not blittable, not primitive, or contains a type tagged as NativeContainer");
        }

        internal static void WriteLayout(Type type)
        {
#if !NET_DOTS
            Console.WriteLine("   Offset | Bytes  | Name     Layout: {0}", type.Name);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                Console.WriteLine("   {0, 6} | {1, 6} | {2}"
                    , Marshal.OffsetOf(type, field.Name)
                    , Marshal.SizeOf(field.FieldType)
                    , field.Name
                );
            }
#else
            _ = type;
#endif
        }

        internal static bool ShouldDeallocate(Allocator allocator)
        {
            // Allocator.Invalid == container is not initialized.
            // Allocator.None    == container is initialized, but container doesn't own data.
            return allocator > Allocator.None;
        }

        internal static bool ShouldDeallocate(AllocatorManager.AllocatorHandle allocator)
        {
            // Allocator.Invalid == container is not initialized.
            // Allocator.None    == container is initialized, but container doesn't own data.
            return allocator.Value > (int)Allocator.None;
        }

        /// <summary>
        /// Tell Burst that an integer can be assumed to map to an always positive value.
        /// </summary>
        /// <param name="x">The integer that is always positive.</param>
        /// <returns>Returns `x`, but allows the compiler to assume it is always positive.</returns>
        [return : AssumeRange(0, int.MaxValue)]
        internal static int AssumePositive(int x)
        {
            return x;
        }
    }
}
