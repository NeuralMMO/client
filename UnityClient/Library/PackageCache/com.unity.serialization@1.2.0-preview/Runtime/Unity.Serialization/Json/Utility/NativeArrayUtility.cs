using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization
{
    static class NativeArrayUtility
    {
        public static unsafe NativeArray<T> Resize<T>(NativeArray<T> array, int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
            where T : unmanaged
        {
            if (array.IsCreated && array.Length >= length)
            {
                return array;
            }

            var buffer = new NativeArray<T>(length, allocator, options);

            if (!array.IsCreated)
            {
                return buffer;
            }
            
            UnsafeUtility.MemCpy(buffer.GetUnsafePtr(), array.GetUnsafePtr(), array.Length * sizeof(T));
            array.Dispose();

            return buffer;
        }
        
        public static unsafe T* Resize<T>(T* ptr, int fromLength, int toLength, int alignment, Allocator allocator)
            where T : unmanaged
        {
            var buffer = (T*) UnsafeUtility.Malloc(sizeof(T) * toLength, alignment, allocator);
            UnsafeUtility.MemCpy(buffer, ptr, fromLength * sizeof(T));
            UnsafeUtility.Free(ptr, allocator);
            return buffer;
        }
    }
}