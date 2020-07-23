#if !UNITY_DOTSPLAYER && !NET_DOTS
using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Burst
{
    /// <summary>
    /// Base interface for a function pointer.
    /// </summary>
    public interface IFunctionPointer
    {
        /// <summary>
        /// Converts a pointer to a function pointer.
        /// </summary>
        /// <param name="ptr">The native pointer.</param>
        /// <returns>An instance of this interface.</returns>
        [Obsolete("This method will be removed in a future version of Burst")]
        IFunctionPointer FromIntPtr(IntPtr ptr);
    }

    /// <summary>
    /// A function pointer that can be used from a Burst Job or from regular C#.
    /// It needs to be compiled through <see cref="BurstCompiler.CompileFunctionPointer{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of the delegate of this function pointer</typeparam>
    public readonly struct FunctionPointer<T> : IFunctionPointer
    {
        [NativeDisableUnsafePtrRestriction]
        private readonly IntPtr _ptr;

        /// <summary>
        /// Creates a new instance of this function pointer with the following native pointer.
        /// </summary>
        /// <param name="ptr"></param>
        public FunctionPointer(IntPtr ptr)
        {
            _ptr = ptr;
        }

        /// <summary>
        /// Gets the underlying pointer.
        /// </summary>
        public IntPtr Value => _ptr;

        /// <summary>
        /// Gets the delegate associated to this function pointer in order to call the function pointer.
        /// This delegate can be called from a Burst Job or from regular C#.
        /// If calling from regular C#, it is recommended to cache the returned delegate of this property
        /// instead of using this property every time you need to call the delegate.
        /// </summary>
        public T Invoke => (T)(object)Marshal.GetDelegateForFunctionPointer(_ptr, typeof(T));

        /// <summary>
        /// Whether the function pointer is valid.
        /// </summary>
        public bool IsCreated => _ptr != IntPtr.Zero;

        IFunctionPointer IFunctionPointer.FromIntPtr(IntPtr ptr) => new FunctionPointer<T>(ptr);
    }
}
#endif