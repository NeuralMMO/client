#if !NET_DOTS // can't use Burst function pointers from DOTS runtime (yet)
#define CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
#endif

#pragma warning disable 0649

using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Collections
{
    public static class AllocatorManager
    {
        public unsafe static void* Allocate(AllocatorHandle handle, int itemSizeInBytes, int alignmentInBytes, int items = 1)
        {
            Block block = default;
            block.Range.Allocator = handle;
            block.Range.Items = items;
            block.Range.Pointer = IntPtr.Zero;
            block.BytesPerItem = itemSizeInBytes;
            block.Alignment = alignmentInBytes;
            var error = Try(ref block);
            if (error != 0)
                throw new ArgumentException("failed to allocate");
            return (void*)block.Range.Pointer;
        }

        public unsafe static T* Allocate<T>(AllocatorHandle handle, int items = 1) where T : unmanaged
        {
            return (T*)Allocate(handle, UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), items);
        }

        public unsafe static void Free(AllocatorHandle handle, void* pointer, int itemSizeInBytes, int alignmentInBytes,
            int items = 1)
        {
            if (pointer == null)
                return;
            Block block = default;
            block.Range.Allocator = handle;
            block.Range.Items = 0;
            block.Range.Pointer = (IntPtr)pointer;
            block.BytesPerItem = itemSizeInBytes;
            block.Alignment = alignmentInBytes;
            var error = Try(ref block);
            if (error != 0)
                throw new ArgumentException("failed to free");
        }

        public unsafe static void Free(AllocatorHandle handle, void* pointer)
        {
            Free(handle, pointer, 1, 1, 1);
        }

        public unsafe static void Free<T>(AllocatorHandle handle, T* pointer, int items = 1) where T : unmanaged
        {
            Free(handle, pointer, UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), items);
        }

        /// <summary>
        /// Corresponds to Allocator.Invalid.
        /// </summary>
        public static readonly AllocatorHandle Invalid = new AllocatorHandle {Value = 0};

        /// <summary>
        /// Corresponds to Allocator.None.
        /// </summary>
        public static readonly AllocatorHandle None = new AllocatorHandle {Value = 1};

        /// <summary>
        /// Corresponds to Allocator.Temp.
        /// </summary>
        public static readonly AllocatorHandle Temp = new AllocatorHandle {Value = 2};

        /// <summary>
        /// Corresponds to Allocator.TempJob.
        /// </summary>
        public static readonly AllocatorHandle TempJob = new AllocatorHandle {Value = 3};

        /// <summary>
        /// Corresponds to Allocator.Persistent.
        /// </summary>
        public static readonly AllocatorHandle Persistent = new AllocatorHandle {Value = 4};

        /// <summary>
        /// Corresponds to Allocator.AudioKernel.
        /// </summary>
        public static readonly AllocatorHandle AudioKernel = new AllocatorHandle {Value = 5};

        #region Allocator Parts
        /// <summary>
        /// Delegate used for calling an allocator's allocation function.
        /// </summary>
        public delegate int TryFunction(IntPtr allocatorState, ref Block block);

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallAllocatorHandle
        {
            public static implicit operator SmallAllocatorHandle(Allocator a) => new SmallAllocatorHandle {Value = (ushort)a};
            public static implicit operator SmallAllocatorHandle(AllocatorHandle a) => new SmallAllocatorHandle {Value = (ushort)a.Value};
            public static implicit operator AllocatorHandle(SmallAllocatorHandle a) => new AllocatorHandle {Value = a.Value};

            /// <summary>
            /// Index into a function table of allocation functions.
            /// </summary>
            public ushort Value;
        }

        /// <summary>
        /// Which allocator a Block's Range allocates from.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct AllocatorHandle
        {
            public static implicit operator AllocatorHandle(Allocator a) => new AllocatorHandle {Value = (int)a};

            /// <summary>
            /// Index into a function table of allocation functions.
            /// </summary>
            public int Value;

            /// <summary>
            /// Allocates a Block of memory from this allocator with requested number of items of a given type.
            /// </summary>
            /// <typeparam name="T">Type of item to allocate.</typeparam>
            /// <param name="block">Block of memory to allocate within.</param>
            /// <param name="Items">Number of items to allocate.</param>
            /// <returns>Error code from the given Block's allocate function.</returns>
            public int TryAllocate<T>(out Block block, int Items) where T : struct
            {
                block = new Block
                {
                    Range = new Range { Items = Items, Allocator = new AllocatorHandle { Value = Value } },
                    BytesPerItem = UnsafeUtility.SizeOf<T>(),
                    Alignment = 1 << math.min(3, math.tzcnt(UnsafeUtility.SizeOf<T>()))
                };
                var returnCode = Try(ref block);
                return returnCode;
            }

            /// <summary>
            /// Allocates a Block of memory from this allocator with requested number of items of a given type.
            /// </summary>
            /// <typeparam name="T">Type of item to allocate.</typeparam>
            /// <param name="Items">Number of items to allocate.</param>
            /// <returns>A Block of memory.</returns>
            public Block Allocate<T>(int Items) where T : struct
            {
                var error = TryAllocate<T>(out Block block, Items);
                if (error != 0)
                    throw new ArgumentException($"Error {error}: Failed to Allocate {block}");
                return block;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BlockHandle { public ushort Value; }

        /// <summary>
        /// Pointer for the beginning of a block of memory, number of items in it, which allocator it belongs to, and which block this is.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Range : IDisposable
        {
            public IntPtr Pointer; //  0
            public int Items; //  8
            public SmallAllocatorHandle Allocator; // 12
            public BlockHandle Block; // 14

            public void Dispose()
            {
                Block block = new Block { Range = this };
                block.Dispose();
                this = block.Range;
            }
        }

        /// <summary>
        /// A block of memory with a Range and metadata for size in bytes of each item in the block, number of allocated items, and alignment.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Block : IDisposable
        {
            public Range Range;
            public int BytesPerItem; // number of bytes in each item requested
            public int AllocatedItems; // how many items were actually allocated
            public byte Log2Alignment; // (1 << this) is the byte alignment
            public byte Padding0;
            public ushort Padding1;
            public uint Padding2;

            public long Bytes => BytesPerItem * Range.Items;

            public int Alignment
            {
                get => 1 << Log2Alignment;
                set => Log2Alignment = (byte)(32 - math.lzcnt(math.max(1, value) - 1));
            }

            public void Dispose()
            {
                TryFree();
            }

            public int TryAllocate()
            {
                Range.Pointer = IntPtr.Zero;
                return Try(ref this);
            }

            public int TryFree()
            {
                Range.Items = 0;
                return Try(ref this);
            }

            public void Allocate()
            {
                var error = TryAllocate();
                if (error != 0)
                    throw new ArgumentException($"Error {error}: Failed to Allocate {this}");
            }

            public void Free()
            {
                var error = TryFree();
                if (error != 0)
                    throw new ArgumentException($"Error {error}: Failed to Free {this}");
            }
        }

        /// <summary>
        /// An allocator with a tryable allocate/free/realloc function pointer.
        /// </summary>
        public interface IAllocator
        {
            TryFunction Function { get; }
            int Try(ref Block block);
            long BudgetInBytes { get; }
            long AllocatedBytes { get; }
        }

        static unsafe int TryLegacy(ref Block block)
        {
            if (block.Range.Pointer == IntPtr.Zero) // Allocate
            {
                block.Range.Pointer =
                    (IntPtr)UnsafeUtility.Malloc(block.Bytes, block.Alignment, (Allocator)block.Range.Allocator.Value);
                block.AllocatedItems = block.Range.Items;
                return (block.Range.Pointer == IntPtr.Zero) ? -1 : 0;
            }
            if (block.Bytes == 0) // Free
            {
                UnsafeUtility.Free((void*)block.Range.Pointer, (Allocator)block.Range.Allocator.Value);
                block.Range.Pointer = IntPtr.Zero;
                block.AllocatedItems = 0;
                return 0;
            }
            // Reallocate (keep existing pointer and change size if possible. otherwise, allocate new thing and copy)
            return -1;
        }

        /// <summary>
        /// Looks up an allocator's allocate, free, or realloc function pointer from a table and invokes the function.
        /// </summary>
        /// <param name="block">Block to allocate memory for.</param>
        /// <returns>Error code of invoked function.</returns>
        public static unsafe int Try(ref Block block)
        {
#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
            if (block.Range.Allocator.Value < FirstUserIndex)
#endif
            return TryLegacy(ref block);
#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
            TableEntry tableEntry = default;
            fixed(TableEntry65536* tableEntry65536 = &StaticFunctionTable.Ref.Data)
            tableEntry = ((TableEntry*)tableEntry65536)[block.Range.Allocator.Value];
            var function = new FunctionPointer<TryFunction>(tableEntry.function);
            // this is really bad in non-Burst C#, it generates garbage each time we call Invoke
            return function.Invoke(tableEntry.state, ref block);
#endif
        }

        #endregion
        #region Allocators

        /// <summary>
        /// Stack allocator with no backing storage.
        /// </summary>
#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
        [BurstCompile(CompileSynchronously = true)]
#endif
        internal struct StackAllocator : IAllocator, IDisposable
        {
            public Block m_storage;
            public long m_top;

            /// <summary>
            /// Upper limit on how many bytes this allocator is allowed to allocate.
            /// </summary>
            public long budgetInBytes;
            public long BudgetInBytes => budgetInBytes;

            /// <summary>
            /// Number of currently allocated bytes for this allocator.
            /// </summary>
            public long allocatedBytes;
            public long AllocatedBytes => allocatedBytes;

            public unsafe int Try(ref Block block)
            {
                if (block.Range.Pointer == IntPtr.Zero) // Allocate
                {
                    if (m_top + block.Bytes > m_storage.Bytes)
                    {
                        return -1;
                    }

                    block.Range.Pointer = (IntPtr)((byte*)m_storage.Range.Pointer + m_top);
                    block.AllocatedItems = block.Range.Items;
                    allocatedBytes += block.Bytes;
                    m_top += block.Bytes;
                    return 0;
                }

                if (block.Bytes == 0) // Free
                {
                    if ((byte*)block.Range.Pointer - (byte*)m_storage.Range.Pointer == (long)(m_top - block.Bytes))
                    {
                        m_top -= block.Bytes;
                        var blockSizeInBytes = block.AllocatedItems * block.BytesPerItem;
                        allocatedBytes -= blockSizeInBytes;
                        block.Range.Pointer = IntPtr.Zero;
                        block.AllocatedItems = 0;
                        return 0;
                    }

                    return -1;
                }

                // Reallocate (keep existing pointer and change size if possible. otherwise, allocate new thing and copy)
                return -1;
            }

#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
            [BurstCompile(CompileSynchronously = true)]
#endif
            public static unsafe int Try(IntPtr allocatorState, ref Block block)
            {
                return ((StackAllocator*)allocatorState)->Try(ref block);
            }

            public TryFunction Function => Try;

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// Slab allocator with no backing storage.
        /// </summary>
#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
        [BurstCompile(CompileSynchronously = true)]
#endif
        internal struct SlabAllocator : IAllocator, IDisposable
        {
            public Block Storage;
            public int Log2SlabSizeInBytes;
            public FixedListInt4096 Occupied;

            /// <summary>
            /// Upper limit on how many bytes this allocator is allowed to allocate.
            /// </summary>
            public long budgetInBytes;
            public long BudgetInBytes => budgetInBytes;

            /// <summary>
            /// Number of currently allocated bytes for this allocator.
            /// </summary>
            public long allocatedBytes;
            public long AllocatedBytes => allocatedBytes;

            public int SlabSizeInBytes
            {
                get => 1 << Log2SlabSizeInBytes;
                set => Log2SlabSizeInBytes = (byte)(32 - math.lzcnt(math.max(1, value) - 1));
            }

            public int Slabs => (int)(Storage.Bytes >> Log2SlabSizeInBytes);

            public SlabAllocator(Block storage, int slabSizeInBytes, long budget)
            {
                Assert.IsTrue((slabSizeInBytes & (slabSizeInBytes - 1)) == 0);
                Storage = storage;
                Log2SlabSizeInBytes = 0;
                Occupied = default;
                budgetInBytes = budget;
                allocatedBytes = 0;
                SlabSizeInBytes = slabSizeInBytes;
                Occupied.Length = (Slabs + 31) / 32;
            }

            public int Try(ref Block block)
            {
                if (block.Range.Pointer == IntPtr.Zero) // Allocate
                {
                    if (block.Bytes + allocatedBytes > budgetInBytes)
                        return -2; //over allocator budget
                    if (block.Bytes > SlabSizeInBytes)
                        return -1;
                    for (var wordIndex = 0; wordIndex < Occupied.Length; ++wordIndex)
                    {
                        var word = Occupied[wordIndex];
                        if (word == -1)
                            continue;
                        for (var bitIndex = 0; bitIndex < 32; ++bitIndex)
                            if ((word & (1 << bitIndex)) == 0)
                            {
                                Occupied[wordIndex] |= 1 << bitIndex;
                                block.Range.Pointer = Storage.Range.Pointer +
                                    (int)(SlabSizeInBytes * (wordIndex * 32U + bitIndex));
                                block.AllocatedItems = SlabSizeInBytes / block.BytesPerItem;
                                allocatedBytes += block.Bytes;
                                return 0;
                            }
                    }

                    return -1;
                }

                if (block.Bytes == 0) // Free
                {
                    var slabIndex = ((ulong)block.Range.Pointer - (ulong)Storage.Range.Pointer) >>
                        Log2SlabSizeInBytes;
                    int wordIndex = (int)(slabIndex >> 5);
                    int bitIndex = (int)(slabIndex & 31);
                    Occupied[wordIndex] &= ~(1 << bitIndex);
                    block.Range.Pointer = IntPtr.Zero;
                    var blockSizeInBytes = block.AllocatedItems * block.BytesPerItem;
                    allocatedBytes -= blockSizeInBytes;
                    block.AllocatedItems = 0;
                    return 0;
                }

                // Reallocate (keep existing pointer and change size if possible. otherwise, allocate new thing and copy)
                return -1;
            }

#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
            [BurstCompile(CompileSynchronously = true)]
#endif
            public static unsafe int Try(IntPtr allocatorState, ref Block block)
            {
                return ((SlabAllocator*)allocatorState)->Try(ref block);
            }

            public TryFunction Function => Try;

            public void Dispose()
            {
            }
        }
        #endregion
        #region AllocatorManager state and state functions
        /// <summary>
        /// Mapping between a Block, AllocatorHandle, and an IAllocator.
        /// </summary>
        /// <typeparam name="T">Type of allocator to install functions for.</typeparam>
        public struct AllocatorInstallation<T> : IDisposable
            where T : unmanaged, IAllocator, IDisposable
        {
            public Block MBlock;
            public AllocatorHandle m_handle;
            private unsafe T* t => (T*)MBlock.Range.Pointer;

            public ref T Allocator
            {
                get
                {
                    unsafe
                    {
                        return ref UnsafeUtilityEx.AsRef<T>(t);
                    }
                }
            }

            /// <summary>
            /// Creates a Block for an allocator, associates that allocator with an AllocatorHandle, then installs the allocator's function into the function table.
            /// </summary>
            /// <param name="Handle">Index into function table at which to install this allocator's function pointer.</param>
            public AllocatorInstallation(AllocatorHandle Handle)
            {
                // Allocate an allocator of type T using UnsafeUtility.Malloc with Allocator.Persistent.
                MBlock = Persistent.Allocate<T>(1);
                m_handle = Handle;
                unsafe
                {
                    UnsafeUtility.MemSet(t, 0, UnsafeUtility.SizeOf<T>());
                }

                unsafe
                {
                    Install(m_handle, (IntPtr)t, t->Function);
                }
            }

            public void Dispose()
            {
                Install(m_handle, IntPtr.Zero, null);
                unsafe
                {
                    t->Dispose();
                }

                MBlock.Dispose();
            }
        }

        struct TableEntry
        {
            public IntPtr function;
            public IntPtr state;
        }

        struct TableEntry16
        {
            public TableEntry f0;
            public TableEntry f1;
            public TableEntry f2;
            public TableEntry f3;
            public TableEntry f4;
            public TableEntry f5;
            public TableEntry f6;
            public TableEntry f7;
            public TableEntry f8;
            public TableEntry f9;
            public TableEntry f10;
            public TableEntry f11;
            public TableEntry f12;
            public TableEntry f13;
            public TableEntry f14;
            public TableEntry f15;
        }

        struct TableEntry256
        {
            public TableEntry16 f0;
            public TableEntry16 f1;
            public TableEntry16 f2;
            public TableEntry16 f3;
            public TableEntry16 f4;
            public TableEntry16 f5;
            public TableEntry16 f6;
            public TableEntry16 f7;
            public TableEntry16 f8;
            public TableEntry16 f9;
            public TableEntry16 f10;
            public TableEntry16 f11;
            public TableEntry16 f12;
            public TableEntry16 f13;
            public TableEntry16 f14;
            public TableEntry16 f15;
        }

        struct TableEntry4096
        {
            public TableEntry256 f0;
            public TableEntry256 f1;
            public TableEntry256 f2;
            public TableEntry256 f3;
            public TableEntry256 f4;
            public TableEntry256 f5;
            public TableEntry256 f6;
            public TableEntry256 f7;
            public TableEntry256 f8;
            public TableEntry256 f9;
            public TableEntry256 f10;
            public TableEntry256 f11;
            public TableEntry256 f12;
            public TableEntry256 f13;
            public TableEntry256 f14;
            public TableEntry256 f15;
        }

        struct TableEntry65536
        {
            public TableEntry4096 f0;
            public TableEntry4096 f1;
            public TableEntry4096 f2;
            public TableEntry4096 f3;
            public TableEntry4096 f4;
            public TableEntry4096 f5;
            public TableEntry4096 f6;
            public TableEntry4096 f7;
            public TableEntry4096 f8;
            public TableEntry4096 f9;
            public TableEntry4096 f10;
            public TableEntry4096 f11;
            public TableEntry4096 f12;
            public TableEntry4096 f13;
            public TableEntry4096 f14;
            public TableEntry4096 f15;
        }

        /// <summary>
        /// SharedStatic that holds array of allocation function pointers for each allocator.
        /// </summary>
        private sealed class StaticFunctionTable
        {
#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
            public static readonly SharedStatic<TableEntry65536> Ref =
                SharedStatic<TableEntry65536>.GetOrCreate<StaticFunctionTable>();
#endif
        }

        /// <summary>
        /// Initializes SharedStatic allocator function table and allocator table, and installs default allocators.
        /// </summary>
        public static void Initialize()
        {
        }

        /// <summary>
        /// Creates and saves allocators' function pointers into function table.
        /// </summary>
        /// <param name="handle">AllocatorHandle to allocator to install function for.</param>
        /// <param name="allocatorState">IntPtr to allocator's custom state.</param>
        /// <param name="function">Function pointer to create or save in function table.</param>
        public static unsafe void Install(AllocatorHandle handle, IntPtr allocatorState, TryFunction function)
        {
#if CUSTOM_ALLOCATOR_BURST_FUNCTION_POINTER
            var functionPointer = (function == null)
                ? new FunctionPointer<TryFunction>(IntPtr.Zero)
                : BurstCompiler.CompileFunctionPointer(function);
            var tableEntry = new TableEntry {state = allocatorState, function = functionPointer.Value};
            fixed(TableEntry65536* tableEntry65536 = &StaticFunctionTable.Ref.Data)
                ((TableEntry*)tableEntry65536)[handle.Value] = tableEntry;
#endif
        }

        public static void Shutdown()
        {
        }

        #endregion
        /// <summary>
        /// User-defined allocator index.
        /// </summary>
        public const ushort FirstUserIndex = 32;
    }
}

#pragma warning restore 0649
