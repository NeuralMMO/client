using System;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe
{
    /// <summary>
    /// An unmanaged, untyped, buffer, without any thread safety check features.
    /// </summary>
    public unsafe struct UnsafeAppendBuffer : IDisposable
    {
        /// <summary>
        /// </summary>
        [NativeDisableUnsafePtrRestriction]
        public byte* Ptr;

        /// <summary>
        /// </summary>
        public int Length;

        /// <summary>
        /// </summary>
        public int Capacity;

        /// <summary>
        /// </summary>
        public Allocator Allocator;

        /// <summary>
        /// </summary>
        public readonly int Alignment;

        /// <summary>
        /// Constructs a new container with the specified initial capacity and type of memory allocation.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the list. If the list grows larger than its capacity,
        /// the internal array is copied to a new, larger array.</param>
        /// <param name="alignment"></param>
        /// <param name="allocator">A member of the
        /// [Unity.Collections.Allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html) enumeration.</param>
        public UnsafeAppendBuffer(int initialCapacity, int alignment, Allocator allocator)
        {
            CheckAlignment(alignment);

            Alignment = alignment;
            Allocator = allocator;
            Ptr = null;
            Length = 0;
            Capacity = 0;

            SetCapacity(initialCapacity);
        }

        /// <summary>
        /// Constructs container as view into memory.
        /// </summary>
        /// <param name="ptr">Pointer to data.</param>
        /// <param name="length">Lenght of data in bytes.</param>
        /// <remarks>Internal capacity will be set to lenght, but internal length will be set to zero.</remarks>
        public UnsafeAppendBuffer(void* ptr, int length)
        {
            Alignment = 0;
            Allocator = Allocator.None;
            Ptr = (byte*)ptr;
            Length = 0;
            Capacity = length;
        }

        /// <summary>
        /// Reports whether the container is empty.
        /// </summary>
        public bool IsEmpty => Length == 0;

        /// <summary>
        /// Reports whether memory for the container is allocated.
        /// </summary>
        /// <value>True if this container object's internal storage has been allocated.</value>
        /// <remarks>Note that the container storage is not created if you use the default constructor. You must specify
        /// at least an allocation type to construct a usable container.</remarks>
        public bool IsCreated => Ptr != null;

        /// <summary>
        /// Disposes of this container and deallocates its memory immediately.
        /// </summary>
        public void Dispose()
        {
            if (CollectionHelper.ShouldDeallocate(Allocator))
            {
                UnsafeUtility.Free(Ptr, Allocator);
                Allocator = Allocator.Invalid;
            }

            Ptr = null;
            Length = 0;
            Capacity = 0;
        }

        /// <summary>
        /// Safely disposes of this container and deallocates its memory when the jobs that use it have completed.
        /// </summary>
        /// <remarks>You can call this function dispose of the container immediately after scheduling the job. Pass
        /// the [JobHandle](https://docs.unity3d.com/ScriptReference/Unity.Jobs.JobHandle.html) returned by
        /// the [Job.Schedule](https://docs.unity3d.com/ScriptReference/Unity.Jobs.IJobExtensions.Schedule.html)
        /// method using the `jobHandle` parameter so the job scheduler can dispose the container after all jobs
        /// using it have run.</remarks>
        /// <param name="inputDeps">The job handle or handles for any scheduled jobs that use this container.</param>
        /// <returns>A new job handle containing the prior handles as well as the handle for the job that deletes
        /// the container.</returns>
        public JobHandle Dispose(JobHandle inputDeps)
        {
            if (CollectionHelper.ShouldDeallocate(Allocator))
            {
                var jobHandle = new UnsafeDisposeJob { Ptr = Ptr, Allocator = Allocator }.Schedule(inputDeps);

                Ptr = null;
                Allocator = Allocator.Invalid;

                return jobHandle;
            }

            Ptr = null;

            return inputDeps;
        }

        /// <summary>
        /// Clears the container.
        /// </summary>
        /// <remarks>The container capacity remains unchanged.</remarks>
        public void Reset()
        {
            Length = 0;
        }

        /// <summary>
        /// Set the number of items that can fit in the container.
        /// </summary>
        /// <param name="capacity">The number of items that the container can hold before it resizes its internal storage.</param>
        public void SetCapacity(int capacity)
        {
            if (capacity <= Capacity)
            {
                return;
            }

            capacity = math.max(64, math.ceilpow2(capacity));

            var newPtr = (byte*)UnsafeUtility.Malloc(capacity, Alignment, Allocator);
            if (Ptr != null)
            {
                UnsafeUtility.MemCpy(newPtr, Ptr, Length);
                UnsafeUtility.Free(Ptr, Allocator);
            }

            Ptr = newPtr;
            Capacity = capacity;
        }

        /// <summary>
        /// Changes the container length, resizing if necessary, without initializing memory.
        /// </summary>
        /// <param name="length">The new length of the container.</param>
        public void ResizeUninitialized(int length)
        {
            SetCapacity(length);
            Length = length;
        }

        /// <summary>
        /// Adds an element to the container.
        /// </summary>
        /// <typeparam name="T">Source type of elements.</typeparam>
        /// <param name="value">The struct to be added at the end of the container.</param>
        public void Add<T>(T value) where T : struct
        {
            var structSize = UnsafeUtility.SizeOf<T>();

            SetCapacity(Length + structSize);
            UnsafeUtility.CopyStructureToPtr(ref value, Ptr + Length);
            Length += structSize;
        }

        /// <summary>
        /// Adds the element to this container.
        /// </summary>
        /// <param name="ptr">A pointer to copy into the container.</param>
        /// <param name="structSize">Structure size in bytes.</param>
        public void Add(void* ptr, int structSize)
        {
            SetCapacity(Length + structSize);
            UnsafeUtility.MemCpy(Ptr + Length, ptr, structSize);
            Length += structSize;
        }

        /// <summary>
        /// Adds the elements to this container.
        /// </summary>
        /// <typeparam name="T">Source type of elements.</typeparam>
        /// <param name="ptr">A pointer to copy into the container.</param>
        /// <param name="length">The number of elements to add to the container.</param>
        public void AddArray<T>(void* ptr, int length) where T : struct
        {
            Add(length);

            if (length != 0)
                Add(ptr, length * UnsafeUtility.SizeOf<T>());
        }

        /// <summary>
        /// Adds elements from a NativeArray to this container.
        /// </summary>
        /// <typeparam name="T">Source type of elements.</typeparam>
        /// <param name="value">Other container to copy elements from.</param>
        public void Add<T>(NativeArray<T> value) where T : struct
        {
            Add(value.Length);
            Add(NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(value), UnsafeUtility.SizeOf<T>() * value.Length);
        }

        /// <summary>
        /// Adds string into the container.
        /// </summary>
        /// <param name="value">String to copy to container.</param>
        public void Add(string value)
        {
            if (value != null)
            {
                Add(value.Length);
                fixed(char* ptr = value)
                {
                    Add(ptr, sizeof(char) * value.Length);
                }
            }
            else
            {
                Add(-1);
            }
        }

        /// <summary>
        /// Retrieve and remove element from the end of buffer.
        /// </summary>
        /// <typeparam name="T">Source type of elements.</typeparam>
        /// <returns>Returns value.</returns>
        public T Pop<T>() where T : struct
        {
            int structSize = UnsafeUtility.SizeOf<T>();
            long ptr = (long)Ptr;
            long size = Length;
            long addr = ptr + size - structSize;

            var data = UnsafeUtility.ReadArrayElement<T>((void*)addr, 0);
            Length -= structSize;
            return data;
        }

        /// <summary>
        /// Retrieve and remove element from the end of buffer.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="structSize"></param>
        public void Pop(void* ptr, int structSize)
        {
            long data = (long)Ptr;
            long size = Length;
            long addr = data + size - structSize;

            UnsafeUtility.MemCpy(ptr, (void*)addr, structSize);
            Length -= structSize;
        }

        /// <summary>
        /// Copy contents of the container into byte array.
        /// </summary>
        /// <returns>Array of bytes.</returns>
        public byte[] ToBytes()
        {
            var dst = new byte[Length];
            fixed(byte* dstPtr = dst)
            {
                UnsafeUtility.MemCpy(dstPtr, Ptr, Length);
            }
            return dst;
        }

        /// <summary>
        /// Returns buffer reader instance.
        /// </summary>
        /// <returns>Returns buffer reader instance.</returns>
        public Reader AsReader()
        {
            return new Reader(ref this);
        }

        /// <summary>
        /// Buffer reader.
        /// </summary>
        public unsafe struct Reader
        {
            public readonly byte* Ptr;
            public readonly int Size;
            public int Offset;

            /// <summary>
            /// Reader constructor.
            /// </summary>
            /// <param name="buffer"></param>
            public Reader(ref UnsafeAppendBuffer buffer)
            {
                Ptr = buffer.Ptr;
                Size = buffer.Length;
                Offset = 0;
            }

            /// <summary>
            /// Reader constructor.
            /// </summary>
            /// <param name="ptr"></param>
            /// <param name="length"></param>
            public Reader(void* ptr, int length)
            {
                Ptr = (byte*)ptr;
                Size = length;
                Offset = 0;
            }

            /// <summary>
            /// Returns true if end of buffer is reached.
            /// </summary>
            public bool EndOfBuffer => Offset == Size;

            /// <summary>
            /// Read data from buffer.
            /// </summary>
            /// <typeparam name="T">Source type of elements.</typeparam>
            /// <param name="value"></param>
            public void ReadNext<T>(out T value) where T : struct
            {
                var structSize = UnsafeUtility.SizeOf<T>();
                CheckBounds(structSize);

                UnsafeUtility.CopyPtrToStructure<T>(Ptr + Offset, out value);
                Offset += structSize;
            }

            /// <summary>
            /// Read data from buffer.
            /// </summary>
            /// <typeparam name="T">Source type of elements.</typeparam>
            /// <returns></returns>
            public T ReadNext<T>() where T : struct
            {
                var structSize = UnsafeUtility.SizeOf<T>();
                CheckBounds(structSize);

                T value = UnsafeUtility.ReadArrayElement<T>(Ptr + Offset, 0);
                Offset += structSize;
                return value;
            }

            /// <summary>
            /// Read data from buffer.
            /// </summary>
            /// <param name="structSize"></param>
            /// <returns></returns>
            public void* ReadNext(int structSize)
            {
                CheckBounds(structSize);

                var value = (void*)((IntPtr)Ptr + Offset);
                Offset += structSize;
                return value;
            }

            /// <summary>
            /// Read data from buffer.
            /// </summary>
            /// <typeparam name="T">Source type of elements.</typeparam>
            /// <param name="value"></param>
            /// <param name="allocator">A member of the
            /// [Unity.Collections.Allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html) enumeration.</param>
            public void ReadNext<T>(out NativeArray<T> value, Allocator allocator) where T : struct
            {
                var length = ReadNext<int>();
                value = new NativeArray<T>(length, allocator);
                var size = length * UnsafeUtility.SizeOf<T>();
                if (size > 0)
                {
                    var ptr = ReadNext(size);
                    UnsafeUtility.MemCpy(NativeArrayUnsafeUtility.GetUnsafePtr(value), ptr, size);
                }
            }

            /// <summary>
            /// Read data from buffer.
            /// </summary>
            /// <typeparam name="T">Source type of elements.</typeparam>
            /// <param name="length"></param>
            /// <returns></returns>
            public void* ReadNextArray<T>(out int length) where T : struct
            {
                length = ReadNext<int>();
                return (length == 0) ? null : ReadNext(length * UnsafeUtility.SizeOf<T>());
            }

#if !NET_DOTS
            /// <summary>
            /// Read string data from buffer.
            /// </summary>
            /// <param name="value">Output string value.</param>
            public void ReadNext(out string value)
            {
                int length;
                ReadNext(out length);

                if (length != -1)
                {
                    value = new string('0', length);

                    fixed(char* buf = value)
                    {
                        int bufLen = length * sizeof(char);
                        UnsafeUtility.MemCpy(buf, ReadNext(bufLen), bufLen);
                    }
                }
                else
                {
                    value = null;
                }
            }

#endif

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void CheckBounds(int structSize)
            {
                if (Offset + structSize > Size)
                {
                    throw new ArgumentException($"Requested value outside bounds of UnsafeAppendOnlyBuffer. Remaining bytes: {Size - Offset} Requested: {structSize}");
                }
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void CheckAlignment(int alignment)
        {
            var zeroAlignment = alignment == 0;
            var powTwoAlignment = ((alignment - 1) & alignment) == 0;
            var validAlignment = (!zeroAlignment) && powTwoAlignment;

            if (!validAlignment)
            {
                throw new ArgumentException($"Specified alignment must be non-zero positive power of two. Requested: {alignment}");
            }
        }
    }
}
