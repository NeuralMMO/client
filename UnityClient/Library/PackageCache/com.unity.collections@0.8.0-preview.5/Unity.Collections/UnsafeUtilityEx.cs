namespace Unity.Collections.LowLevel.Unsafe
{
    public unsafe static class UnsafeUtilityEx
    {
        public static ref T As<U, T>(ref U from)
        {
#if NET_DOTS || UNITY_2020_1_OR_NEWER
            return ref UnsafeUtility.As<U, T>(ref from);
#else
            return ref System.Runtime.CompilerServices.Unsafe.As<U, T>(ref from);
#endif
        }

        public static ref T AsRef<T>(void* ptr) where T : struct
        {
#if NET_DOTS || UNITY_2020_1_OR_NEWER
            return ref UnsafeUtility.AsRef<T>(ptr);
#else
            return ref System.Runtime.CompilerServices.Unsafe.AsRef<T>(ptr);
#endif
        }

        public static ref T ArrayElementAsRef<T>(void* ptr, int index) where T : struct
        {
#if NET_DOTS || UNITY_2020_1_OR_NEWER
            return ref UnsafeUtility.ArrayElementAsRef<T>(ptr, index);
#else
            return ref System.Runtime.CompilerServices.Unsafe.AsRef<T>((byte*)ptr + index * UnsafeUtility.SizeOf<T>());
#endif
        }
    }
}
