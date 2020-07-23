using System;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe
{
    internal struct RingControl
    {
        internal RingControl(int capacity)
        {
            Capacity = capacity;
            Current = 0;
            Write = 0;
            Read = 0;
        }

        internal void Reset()
        {
            Current = 0;
            Write = 0;
            Read = 0;
        }

        internal int Distance(int from, int to)
        {
            var diff = to - from;
            return diff < 0 ? Capacity - math.abs(diff) : diff;
        }

        internal int Available()
        {
            return Distance(Read, Current);
        }

        internal int Reserve(int count)
        {
            var dist = Distance(Write, Read) - 1;
            var maxCount = dist < 0 ? Capacity - 1 : dist;
            var absCount = math.abs(count);
            var test = absCount - maxCount;
            count = test < 0 ? count : maxCount;
            Write = (Write + count) % Capacity;

            return count;
        }

        internal int Commit(int count)
        {
            var maxCount = Distance(Current, Write);
            var absCount = math.abs(count);
            var test = absCount - maxCount;
            count = test < 0 ? count : maxCount;
            Current = (Current + count) % Capacity;

            return count;
        }

        internal int Consume(int count)
        {
            var maxCount = Distance(Read, Current);
            var absCount = math.abs(count);
            var test = absCount - maxCount;
            count = test < 0 ? count : maxCount;
            Read = (Read + count) % Capacity;

            return count;
        }

        internal int Length => Distance(Read, Write);

        internal readonly int Capacity;
        internal int Current;
        internal int Write;
        internal int Read;
    }

    /// <summary>
    /// Fixed-size circular buffer.
    /// </summary>
    /// <typeparam name="T">Source type of elements.</typeparam>
    [DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}")]
    [DebuggerTypeProxy(typeof(UnsafeRingQueueDebugView<>))]
    public unsafe struct UnsafeRingQueue<T> : IDisposable
        where T : unmanaged
    {
        /// <summary>
        /// </summary>
        [NativeDisableUnsafePtrRestriction]
        public T* Ptr;

        /// <summary>
        /// </summary>
        public Allocator Allocator;

        internal RingControl Control;

        /// <summary>
        /// Returns number of items in the container.
        /// </summary>
        public int Length => Control.Length;

        /// <summary>
        /// Returns capacity of the container.
        /// </summary>
        public int Capacity => Control.Capacity;

        /// <summary>
        /// Constructs container as view into memory.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="capacity"></param>
        public UnsafeRingQueue(T* ptr, int capacity)
        {
            Ptr = ptr;
            Allocator = Allocator.None;
            Control = new RingControl(capacity);
        }

        /// <summary>
        /// Constructs a new container with the specified capacity and type of memory allocation.
        /// </summary>
        /// <param name="capacity">Container capacity.</param>
        /// <param name="allocator">A member of the
        /// [Unity.Collections.Allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html) enumeration.</param>
        /// <param name="options">Memory should be cleared on allocation or left uninitialized.</param>
        public UnsafeRingQueue(int capacity, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            capacity += 1;

            Allocator = allocator;
            Control = new RingControl(capacity);
            var sizeInBytes = capacity * UnsafeUtility.SizeOf<T>();
            Ptr = (T*)UnsafeUtility.Malloc(sizeInBytes, 16, allocator);

            if (options == NativeArrayOptions.ClearMemory)
            {
                UnsafeUtility.MemClear(Ptr, sizeInBytes);
            }
        }

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
        /// Try enqueuing value into the container. If container is full value won't be enqueued, and return result will be false.
        /// </summary>
        /// <param name="value">The value to be appended.</param>
        /// <returns>Returns true if value was appended, otherwise returns false.</returns>
        public bool TryEnqueue(T value)
        {
            if (1 != Control.Reserve(1))
            {
                return false;
            }

            Ptr[Control.Current] = value;
            Control.Commit(1);

            return true;
        }

        /// <summary>
        /// Enqueue value into the container.
        /// </summary>
        /// <param name="value">The value to be appended.</param>
        /// <exception cref="InvalidOperationException">Thrown if capacity is reached and is not possible to enqueue value <see cref="Capacity"/>.</exception>
        public void Enqueue(T value)
        {
            if (!TryEnqueue(value))
            {
                throw new InvalidOperationException("Trying to enqueue into full queue.");
            }
        }

        /// <summary>
        /// Try dequeueing item from the container. If container is empty item won't be changed, and return result will be false.
        /// </summary>
        /// <param name="item">Item value if dequeued.</param>
        /// <returns>Returns true if item was dequeued.</returns>
        public bool TryDequeue(out T item)
        {
            item = Ptr[Control.Read];
            return 1 == Control.Consume(1);
        }

        /// <summary>
        /// Dequeue item from the container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if queue is empty <see cref="Length"/>.</exception>
        /// <returns>Returns item from the container.</returns>
        public T Dequeue()
        {
            T item;

            if (!TryDequeue(out item))
            {
                throw new InvalidOperationException("Trying to dequeue from an empty queue");
            }

            return item;
        }
    }

    internal sealed class UnsafeRingQueueDebugView<T>
        where T : unmanaged
    {
        UnsafeRingQueue<T> Data;

        public UnsafeRingQueueDebugView(UnsafeRingQueue<T> data)
        {
            Data = data;
        }

        public unsafe T[] Items
        {
            get
            {
                T[] result = new T[Data.Length];

                var read = Data.Control.Read;
                var capacity = Data.Control.Capacity;

                for (var i = 0; i < result.Length; ++i)
                {
                    result[i] = Data.Ptr[(read + i) % capacity];
                }

                return result;
            }
        }
    }
}
