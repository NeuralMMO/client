using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections
{
    /// <summary>
    /// Arbitrary sized array of bits.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [DebuggerDisplay("Length = {Length}, IsCreated = {IsCreated}")]
    public unsafe struct NativeBitArray : IDisposable
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#if UNITY_2020_1_OR_NEWER
        private static readonly SharedStatic<int> s_staticSafetyId = SharedStatic<int>.GetOrCreate<NativeBitArray>();
        [BurstDiscard]
        private static void CreateStaticSafetyId()
        {
            s_staticSafetyId.Data = AtomicSafetyHandle.NewStaticSafetyId<NativeBitArray>();
        }

#endif
        [NativeSetClassTypeToNullOnSchedule]
        DisposeSentinel m_DisposeSentinel;
#endif
        [NativeDisableUnsafePtrRestriction]
        internal UnsafeBitArray m_BitArray;

        /// <summary>
        /// Constructs a new container with the specified initial capacity and type of memory allocation.
        /// </summary>
        /// <param name="numBits">Number of bits.</param>
        /// <param name="allocator">A member of the
        /// [Unity.Collections.Allocator](https://docs.unity3d.com/ScriptReference/Unity.Collections.Allocator.html) enumeration.</param>
        /// <param name="options">Memory should be cleared on allocation or left uninitialized.</param>
        public NativeBitArray(int numBits, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
            : this(numBits, allocator, options, 2)
        {
        }

        NativeBitArray(int numBits, Allocator allocator, NativeArrayOptions options, int disposeSentinelStackDepth)
        {
            if (allocator <= Allocator.None)
            {
                throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", nameof(allocator));
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, disposeSentinelStackDepth, allocator);
#if UNITY_2020_1_OR_NEWER
            if (s_staticSafetyId.Data == 0)
            {
                CreateStaticSafetyId();
            }
            AtomicSafetyHandle.SetStaticSafetyId(ref m_Safety, s_staticSafetyId.Data);
#endif
#endif
            m_BitArray = new UnsafeBitArray(numBits, allocator, options);
        }

        /// <summary>
        /// Reports whether memory for the container is allocated.
        /// </summary>
        /// <value>True if this container object's internal storage has been allocated.</value>
        /// <remarks>Note that the container storage is not created if you use the default constructor. You must specify
        /// at least an allocation type to construct a usable container.</remarks>
        public bool IsCreated => m_BitArray.IsCreated;

        /// <summary>
        /// Disposes of this container and deallocates its memory immediately.
        /// </summary>
        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            m_BitArray.Dispose();
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
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Clear(ref m_DisposeSentinel);
#endif
            var jobHandle = m_BitArray.Dispose(inputDeps);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.Release(m_Safety);
#endif

            return jobHandle;
        }

        /// <summary>
        /// The current number of items in the list.
        /// </summary>
        /// <value>The item count.</value>
        public int Length
        {
            get
            {
                CheckRead();
                return CollectionHelper.AssumePositive(m_BitArray.Length);
            }
        }

        /// <summary>
        /// Clear all bits to 0.
        /// </summary>
        public void Clear()
        {
            CheckWrite();
            m_BitArray.Clear();
        }

        /// <summary>
        /// Set single bit to desired boolean value.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="value">Value of bits to set.</param>
        public void Set(int pos, bool value)
        {
            CheckWrite();
            m_BitArray.Set(pos, value);
        }

        /// <summary>
        /// Set bits to desired boolean value.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="value">Value of bits to set.</param>
        /// <param name="numBits">Number of bits to set.</param>
        public void SetBits(int pos, bool value, int numBits)
        {
            CheckWrite();
            m_BitArray.SetBits(pos, value, numBits);
        }

        /// <summary>
        /// Sets bits in range as ulong.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="value">Value of bits to set.</param>
        /// <param name="numBits">Number of bits to set (must be 1-64).</param>
        public void SetBits(int pos, ulong value, int numBits = 1)
        {
            CheckWrite();
            m_BitArray.SetBits(pos, value, numBits);
        }

        /// <summary>
        /// Returns all bits in range as ulong.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="numBits">Number of bits to get (must be 1-64).</param>
        /// <returns>Returns requested range of bits.</returns>
        public ulong GetBits(int pos, int numBits = 1)
        {
            CheckRead();
            return m_BitArray.GetBits(pos, numBits);
        }

        /// <summary>
        /// Returns true is bit at position is set.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <returns>Returns true if bit is set.</returns>
        public bool IsSet(int pos)
        {
            CheckRead();
            return m_BitArray.IsSet(pos);
        }

        /// <summary>
        /// Returns true if none of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="numBits">Number of bits to test.</param>
        /// <returns>Returns true if none of bits are set.</returns>
        public bool TestNone(int pos, int numBits = 1)
        {
            CheckRead();
            return m_BitArray.TestNone(pos, numBits);
        }

        /// <summary>
        /// Returns true if any of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="numBits">Number of bits to test.</param>
        /// <returns>Returns true if at least one bit is set.</returns>
        public bool TestAny(int pos, int numBits = 1)
        {
            CheckRead();
            return m_BitArray.TestAny(pos, numBits);
        }

        /// <summary>
        /// Returns true if all of bits in range are set.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="numBits">Number of bits to test.</param>
        /// <returns>Returns true if all bits are set.</returns>
        public bool TestAll(int pos, int numBits = 1)
        {
            CheckRead();
            return m_BitArray.TestAll(pos, numBits);
        }

        /// <summary>
        /// Calculate number of set bits.
        /// </summary>
        /// <param name="pos">Position in bit array.</param>
        /// <param name="numBits">Number of bits to perform count.</param>
        /// <returns>Number of set bits.</returns>
        public int CountBits(int pos, int numBits = 1)
        {
            CheckRead();
            return m_BitArray.CountBits(pos, numBits);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckRead()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckWrite()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
        }
    }
}
