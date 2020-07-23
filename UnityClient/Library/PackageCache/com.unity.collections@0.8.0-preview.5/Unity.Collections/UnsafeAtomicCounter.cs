using System.Threading;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe
{
    /// <summary>
    /// 32-bit atomic counter.
    /// </summary>
    public unsafe struct UnsafeAtomicCounter32
    {
        /// <summary>
        /// Counter value.
        /// </summary>
        public int* Counter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Initial value.</param>
        public UnsafeAtomicCounter32(void* ptr)
        {
            Counter = (int*)ptr;
        }

        /// <summary>
        /// Reset counter to value.
        /// </summary>
        /// <param name="value">Initial value.</param>
        public void Reset(int value = 0)
        {
            *Counter = value;
        }

        /// <summary>
        /// Adds value to counter.
        /// </summary>
        /// <param name="value">Value to add to counter.</param>
        /// <returns></returns>
        public int Add(int value)
        {
            return Interlocked.Add(ref UnsafeUtilityEx.AsRef<int>(Counter), value) - value;
        }

        /// <summary>
        /// Subtract value from counter.
        /// </summary>
        /// <param name="value">Value to subtract from counter.</param>
        /// <returns></returns>
        public int Sub(int value) => Add(-value);

        /// <summary>
        /// Add value to counter and saturate to maximum specified.
        /// </summary>
        /// <param name="value">Value to add to counter.</param>
        /// <param name="max">Maximum value of counter.</param>
        /// <returns></returns>
        public int AddSat(int value, int max = int.MaxValue)
        {
            int oldVal;
            int newVal = *Counter;
            do
            {
                oldVal = newVal;
                newVal = newVal >= max ? max : math.min(max, newVal + value);
                newVal = Interlocked.CompareExchange(ref UnsafeUtilityEx.AsRef<int>(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != max);

            return oldVal;
        }

        /// <summary>
        /// Subtract value from counter and staturate to minimum specified.
        /// </summary>
        /// <param name="value">Value to subtract from counter.</param>
        /// <param name="min">Minumum value of counter.</param>
        /// <returns></returns>
        public int SubSat(int value, int min = int.MinValue)
        {
            int oldVal;
            int newVal = *Counter;
            do
            {
                oldVal = newVal;
                newVal = newVal <= min ? min : math.max(min, newVal - value);
                newVal = Interlocked.CompareExchange(ref UnsafeUtilityEx.AsRef<int>(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != min);

            return oldVal;
        }
    }

    /// <summary>
    /// 64-bit atomic counter.
    /// </summary>
    public unsafe struct UnsafeAtomicCounter64
    {
        /// <summary>
        /// Counter value.
        /// </summary>
        public long* Counter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Initial value.</param>
        public UnsafeAtomicCounter64(void* ptr)
        {
            Counter = (long*)ptr;
        }

        /// <summary>
        /// Reset counter to value.
        /// </summary>
        /// <param name="value">Initial value.</param>
        public void Reset(long value = 0)
        {
            *Counter = value;
        }

        /// <summary>
        /// Adds value to counter.
        /// </summary>
        /// <param name="value">Value to add to counter.</param>
        /// <returns></returns>
        public long Add(long value)
        {
            return Interlocked.Add(ref UnsafeUtilityEx.AsRef<long>(Counter), value) - value;
        }

        /// <summary>
        /// Subtract value from counter.
        /// </summary>
        /// <param name="value">Value to subtract from counter.</param>
        /// <returns></returns>
        public long Sub(long value) => Add(-value);

        /// <summary>
        /// Add value to counter and saturate to maximum specified.
        /// </summary>
        /// <param name="value">Value to add to counter.</param>
        /// <param name="max">Maximum value of counter.</param>
        /// <returns></returns>
        public long AddSat(long value, long max = long.MaxValue)
        {
            long oldVal;
            long newVal = *Counter;
            do
            {
                oldVal = newVal;
                newVal = newVal >= max ? max : math.min(max, newVal + value);
                newVal = Interlocked.CompareExchange(ref UnsafeUtilityEx.AsRef<long>(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != max);

            return oldVal;
        }

        /// <summary>
        /// Subtract value from counter and staturate to minimum specified.
        /// </summary>
        /// <param name="value">Value to subtract from counter.</param>
        /// <param name="min">Minumum value of counter.</param>
        /// <returns></returns>
        public long SubSat(long value, long min = long.MinValue)
        {
            long oldVal;
            long newVal = *Counter;
            do
            {
                oldVal = newVal;
                newVal = newVal <= min ? min : math.max(min, newVal - value);
                newVal = Interlocked.CompareExchange(ref UnsafeUtilityEx.AsRef<long>(Counter), newVal, oldVal);
            }
            while (oldVal != newVal && oldVal != min);

            return oldVal;
        }
    }
}
