using System;
using System.Runtime.CompilerServices;

namespace Unity.Properties
{
    static class ArrayUtility
    {
        public static T[] RemoveAt<T>(T[] source, int index)
        {
            if (index < 0)
                throw new ArgumentException(nameof(ArrayUtility) + ": index must be in [0, Length -1] range.");
            
            var dest = new T[source.Length - 1];
            if( index > 0 )
                Copy(source, 0, dest, 0, index);

            if( index < source.Length - 1 )
                Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }
        
        public static T[] InsertAt<T>(T[] source, int index, T value)
        {
            if (index < 0 || index > source.Length)
                throw new ArgumentException(nameof(ArrayUtility) + ": index must be in [0, Length] range.");
            
            var dest = new T[source.Length + 1];
            if (index == 0)
            {
                dest[0] = value;
                Copy(source, 0, dest, 1, source.Length);
                return dest;
            }

            if (index == source.Length)
            {
                dest[source.Length] = value;
                Copy(source, 0, dest, 0, source.Length);
                return dest;
            }

            dest[index] = value;
            Copy(source, 0, dest, 0, index - 1);
            Copy(source, index, dest, index, source.Length - index);
            return dest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Copy<T>(
            T[] sourceArray,
            int sourceIndex,
            T[] destinationArray,
            int destinationIndex,
            int length)
        {
            for (; destinationIndex < length; destinationIndex++, sourceIndex++)
            {
                var sourceValue = sourceArray[sourceIndex];
                destinationArray[destinationIndex] = sourceValue;
            }
        }
        
    }
}