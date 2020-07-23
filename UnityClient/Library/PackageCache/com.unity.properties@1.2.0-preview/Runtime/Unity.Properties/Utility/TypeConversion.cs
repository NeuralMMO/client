using System;
using System.Globalization;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Represents the method that will handle converting an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="value">The source value to be converted.</param>
    /// <typeparam name="TSource">The source type to convert from.</typeparam>
    /// <typeparam name="TDestination">The destination type to convert to.</typeparam>
    public delegate TDestination ConvertDelegate<in TSource, out TDestination>(TSource value);

    /// <summary>
    /// Helper class to handle type conversion during properties API calls.
    /// </summary>
    public static class TypeConversion
    {
        struct Converter<TSource, TDestination>
        {
            public static ConvertDelegate<TSource, TDestination> Convert;
        }

        static TypeConversion()
        {
            PrimitiveConverters.Register();
        }

        /// <summary>
        /// Registers a new converter from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="convert"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        public static void Register<TSource, TDestination>(ConvertDelegate<TSource, TDestination> convert)
        {
            Converter<TSource, TDestination>.Convert = convert;
        }

        /// <summary>
        /// Converts the specified value from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="value">The source value to convert.</param>
        /// <typeparam name="TSource">The source type to convert from.</typeparam>
        /// <typeparam name="TDestination">The destination type to convert to.</typeparam>
        /// <returns>The value converted to the <typeparamref name="TDestination"/> type.</returns>
        /// <exception cref="InvalidOperationException">No converter is registered for the given types.</exception>
        public static TDestination Convert<TSource, TDestination>(TSource value)
        {
            if (!TryConvert<TSource, TDestination>(value, out var destination))
            {
                throw new InvalidOperationException($"TypeConversion no converter has been registered for SrcType=[{typeof(TSource)}] to DstType=[{typeof(TDestination)}]");
            }

            return destination;
        }

        /// <summary>
        /// Converts the specified value from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="source">The source value to convert.</param>
        /// <param name="destination">When this method returns, contains the converted destination value if the conversion succeeded; otherwise, default.</param>
        /// <typeparam name="TSource">The source type to convert from.</typeparam>
        /// <typeparam name="TDestination">The destination type to convert to.</typeparam>
        ///<returns><see langword="true"/> if the conversion succeeded; otherwise, <see langword="false"/>.</returns>
        public static bool TryConvert<TSource, TDestination>(TSource source, out TDestination destination)
        {
            if (null != Converter<TSource, TDestination>.Convert)
            {
                destination = Converter<TSource, TDestination>.Convert(source);
                return true;
            }

            if (RuntimeTypeInfoCache<TDestination>.IsNullable)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(TDestination));

                if (underlyingType.IsEnum)
                {
                    underlyingType = Enum.GetUnderlyingType(underlyingType);
                }
                
                destination = (TDestination) System.Convert.ChangeType(source, underlyingType);
                return true;
            }

#if !UNITY_DOTSPLAYER
            if (TryConvertToUnityEngineObject(source, out destination))
            {
                return true;
            }
#endif
            
            if (TryConvertToEnum(source, out destination))
            {
                return true;
            }

            // Could be boxing :(
            if (source is TDestination assignable)
            {
                destination = assignable;
                return true;
            }

            if (typeof(TDestination).IsAssignableFrom(typeof(TSource)))
            {
                destination = (TDestination) (object) source;
                return true;
            }

            destination = default;
            return false;
        }


#if !UNITY_DOTSPLAYER
        static bool TryConvertToUnityEngineObject<TSource, TDestination>(TSource source, out TDestination destination)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(typeof(TDestination)))
            {
                destination = default;
                return false;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TSource)) && null == source)
            {
                destination = default;
                return true;
            }

#if UNITY_EDITOR
            var sourceType = typeof(TSource);

            if ((sourceType.IsClass && null != source) || sourceType.IsValueType)
            {
                var str = source.ToString();

                if (UnityEditor.GlobalObjectId.TryParse(str, out var id))
                {
                    var obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                    destination = (TDestination) (object) obj;
                    return true;
                }

                if (str == new UnityEditor.GlobalObjectId().ToString())
                {
                    destination = (TDestination) (object) null;
                    return true;
                }
            }

#endif
            destination = default;
            return false;
        }
#endif

        static bool TryConvertToEnum<TSource, TDestination>(TSource source, out TDestination destination)
        {
            if (!typeof(TDestination).IsEnum)
            {
                destination = default;
                return false;
            }

            if (typeof(TSource) == typeof(string))
            {
                try
                {
                    destination = (TDestination) Enum.Parse(typeof(TDestination), (string) (object) source);
                }
                catch (ArgumentException)
                {
                    destination = default;
                    return false;
                }

                return true;
            }

            if (typeof(TSource).IsAssignableFrom(typeof(TDestination)))
            {
                destination = (TDestination) Enum.ToObject(typeof(TDestination), source);
                return true;
            }

            var sourceTypeCode = Type.GetTypeCode(typeof(TSource));
            var destinationTypeCode = Type.GetTypeCode(typeof(TDestination));
            
            // Enums are tricky, and we need to handle narrowing conversion manually. Might as well do all possible valid use-cases.
            switch (sourceTypeCode)
            {
                case TypeCode.UInt64:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<ulong, int>(Convert<TSource, ulong>(source));
                            break;
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<ulong, byte>(Convert<TSource, ulong>(source));
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<ulong, short>(Convert<TSource, ulong>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<ulong, long>(Convert<TSource, ulong>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<ulong, sbyte>(Convert<TSource, ulong>(source));
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<ulong, ushort>(Convert<TSource, ulong>(source));
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<ulong, uint>(Convert<TSource, ulong>(source));
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<TSource, ulong>(source);
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Int32:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<TSource, int>(source);
                            break;
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<int, byte>(Convert<TSource, int>(source));
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<int, short>(Convert<TSource, int>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<int, long>(Convert<TSource, int>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<int, sbyte>(Convert<TSource, int>(source));
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<int, ushort>(Convert<TSource, int>(source));
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<int, uint>(Convert<TSource, int>(source));
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<int, ulong>(Convert<TSource, int>(source));
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Byte:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<TSource, byte>(source);
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<byte, short>(Convert<TSource, byte>(source));
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<byte, int>(Convert<TSource, byte>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<byte, long>(Convert<TSource, byte>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<byte, sbyte>(Convert<TSource, byte>(source));
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<byte, ushort>(Convert<TSource, byte>(source));
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<byte, uint>(Convert<TSource, byte>(source));
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<byte, ulong>(Convert<TSource, byte>(source));
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.SByte:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<sbyte, byte>(Convert<TSource, sbyte>(source));
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<sbyte, short>(Convert<TSource, sbyte>(source));
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<sbyte, int>(Convert<TSource, sbyte>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<sbyte, long>(Convert<TSource, sbyte>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<TSource, sbyte>(source);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<sbyte, ushort>(Convert<TSource, sbyte>(source));
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<sbyte, uint>(Convert<TSource, sbyte>(source));
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<sbyte, ulong>(Convert<TSource, sbyte>(source));
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Int16:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<short, byte>(Convert<TSource, short>(source));
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<TSource, short>(source);
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<short, int>(Convert<TSource, short>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<short, long>(Convert<TSource, short>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<short, sbyte>(Convert<TSource, short>(source));
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<short, ushort>(Convert<TSource, short>(source));
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<short, uint>(Convert<TSource, short>(source));
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<short, ulong>(Convert<TSource, short>(source));
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.UInt16:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<ushort, byte>(Convert<TSource, ushort>(source));
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<ushort, short>(Convert<TSource, ushort>(source));
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<ushort, int>(Convert<TSource, ushort>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<ushort, long>(Convert<TSource, ushort>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<ushort, sbyte>(Convert<TSource, ushort>(source));
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<TSource, ushort>(source);
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<ushort, uint>(Convert<TSource, ushort>(source));
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<ushort, ulong>(Convert<TSource, ushort>(source));
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.UInt32:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<uint, byte>(Convert<TSource, uint>(source));
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<uint, short>(Convert<TSource, uint>(source));
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<uint, int>(Convert<TSource, uint>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<uint, long>(Convert<TSource, uint>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<uint, sbyte>(Convert<TSource, uint>(source));
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<uint, ushort>(Convert<TSource, uint>(source));
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<TSource, uint>(source);
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<uint, ulong>(Convert<TSource, uint>(source));
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                case TypeCode.Int64:
                    switch (destinationTypeCode)
                    {
                        case TypeCode.Byte:
                            destination = (TDestination) (object) Convert<long, byte>(Convert<TSource, long>(source));
                            break;
                        case TypeCode.Int16:
                            destination = (TDestination) (object) Convert<long, short>(Convert<TSource, long>(source));
                            break;
                        case TypeCode.Int32:
                            destination = (TDestination) (object) Convert<long, int>(Convert<TSource, long>(source));
                            break;
                        case TypeCode.Int64:
                            destination = (TDestination) (object) Convert<long, long>(Convert<TSource, long>(source));
                            break;
                        case TypeCode.SByte:
                            destination = (TDestination) (object) Convert<TSource, sbyte>(source);
                            break;
                        case TypeCode.UInt16:
                            destination = (TDestination) (object) Convert<long, ushort>(Convert<TSource, long>(source));
                            break;
                        case TypeCode.UInt32:
                            destination = (TDestination) (object) Convert<long, uint>(Convert<TSource, long>(source));
                            break;
                        case TypeCode.UInt64:
                            destination = (TDestination) (object) Convert<long, ulong>(Convert<TSource, long>(source));
                            break;
                        default:
                            destination = default;
                            return false;
                    }

                    break;
                default:
                    destination = default;
                    return false;
            }

            return true;
        }
        
        static class PrimitiveConverters
        {
            public static void Register()
            {
                // signed integral types
                RegisterInt8Converters();
                RegisterInt16Converters();
                RegisterInt32Converters();
                RegisterInt64Converters();

                // unsigned integral types
                RegisterUInt8Converters();
                RegisterUInt16Converters();
                RegisterUInt32Converters();
                RegisterUInt64Converters();

                // floating point types
                RegisterFloat32Converters();
                RegisterFloat64Converters();

                // .net types
                RegisterBooleanConverters();
                RegisterCharConverters();
                RegisterStringConverters();
                RegisterObjectConverters();

                // Unity vector types
                RegisterVectorConverters();
                
                // support System.Guid by default
                TypeConversion.Register<string, Guid>(g => new Guid(g));
            }

            static void RegisterInt8Converters()
            {
                Converter<sbyte, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<sbyte, bool>.Convert = v => v != 0;
                Converter<sbyte, sbyte>.Convert = v => (sbyte) v;
                Converter<sbyte, short>.Convert = v => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<sbyte, int>.Convert = v => (int) v;
                Converter<sbyte, long>.Convert = v => (long) v;
                Converter<sbyte, byte>.Convert = v => (byte) v;
                Converter<sbyte, ushort>.Convert = v => (ushort) v;
                Converter<sbyte, uint>.Convert = v => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<sbyte, ulong>.Convert = v => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<sbyte, float>.Convert = v => (float) v;
                Converter<sbyte, double>.Convert = v => (double) v;
                Converter<sbyte, object>.Convert = v => (object) v;
            }

            static void RegisterInt16Converters()
            {
                Converter<short, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<short, bool>.Convert = v => v != 0;
                Converter<short, sbyte>.Convert = v =>  (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<short, short>.Convert = v => (short) v;
                Converter<short, int>.Convert = v => (int) v;
                Converter<short, long>.Convert = v => (long) v;
                Converter<short, byte>.Convert = v => (byte) v;
                Converter<short, ushort>.Convert = v => (ushort) v;
                Converter<short, uint>.Convert = v => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<short, ulong>.Convert = v =>  (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<short, float>.Convert = v => (float) v;
                Converter<short, double>.Convert = v => (double) v;
                Converter<short, object>.Convert = v => (object) v;
            }

            static void RegisterInt32Converters()
            {
                Converter<int, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<int, bool>.Convert = v => v != 0;
                Converter<int, sbyte>.Convert = v => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<int, short>.Convert = v => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<int, int>.Convert = v => (int) v;
                Converter<int, long>.Convert = v => (long) v;
                Converter<int, byte>.Convert = v => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<int, ushort>.Convert = v => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<int, uint>.Convert = v => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<int, ulong>.Convert = v => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<int, float>.Convert = v => (float) v;
                Converter<int, double>.Convert = v => (double) v;
                Converter<int, object>.Convert = v => (object) v;
            }

            static void RegisterInt64Converters()
            {
                Converter<long, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<long, bool>.Convert = v => v != 0;
                Converter<long, sbyte>.Convert = v => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<long, short>.Convert = v => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<long, int>.Convert = v => (int) Clamp(v, int.MinValue, int.MaxValue);
                Converter<long, long>.Convert = v => (long) v;
                Converter<long, byte>.Convert = v => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<long, ushort>.Convert = v => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<long, uint>.Convert = v => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<long, ulong>.Convert = v => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<long, float>.Convert = v => (float) v;
                Converter<long, double>.Convert = v => (double) v;
                Converter<long, object>.Convert = v => (object) v;
            }

            static void RegisterUInt8Converters()
            {
                Converter<byte, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<byte, bool>.Convert = v => v != 0;
                Converter<byte, sbyte>.Convert = v => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<byte, short>.Convert = v => (short) v;
                Converter<byte, int>.Convert = v => (int) v;
                Converter<byte, long>.Convert = v => (long) v;
                Converter<byte, byte>.Convert = v => (byte) v;
                Converter<byte, ushort>.Convert = v => (ushort) v;
                Converter<byte, uint>.Convert = v => (uint) v;
                Converter<byte, ulong>.Convert = v => (ulong) v;
                Converter<byte, float>.Convert = v => (float) v;
                Converter<byte, double>.Convert = v => (double) v;
                Converter<byte, object>.Convert = v => (object) v;
            }

            static void RegisterUInt16Converters()
            {
                Converter<ushort, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<ushort, bool>.Convert = v => v != 0;
                Converter<ushort, sbyte>.Convert = v => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<ushort, short>.Convert = v => (short) Clamp(v, 0, short.MaxValue);
                Converter<ushort, int>.Convert = v => (int) v;
                Converter<ushort, long>.Convert = v => (long) v;
                Converter<ushort, byte>.Convert = v => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<ushort, ushort>.Convert = v => (ushort) v;
                Converter<ushort, uint>.Convert = v => (uint) v;
                Converter<ushort, ulong>.Convert = v => (ulong) v;
                Converter<ushort, float>.Convert = v => (float) v;
                Converter<ushort, double>.Convert = v => (double) v;
                Converter<ushort, object>.Convert = v => (object) v;
            }

            static void RegisterUInt32Converters()
            {
                Converter<uint, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<uint, bool>.Convert = v => v != 0;
                Converter<uint, sbyte>.Convert = v => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<uint, short>.Convert = v => (short) Clamp(v, 0, short.MaxValue);
                Converter<uint, int>.Convert = v => (int) Clamp(v, 0, int.MaxValue);
                Converter<uint, long>.Convert = v => (long) Clamp(v, 0, long.MaxValue);
                Converter<uint, byte>.Convert = v => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<uint, ushort>.Convert = v => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<uint, uint>.Convert = v => (uint) v;
                Converter<uint, ulong>.Convert = v => (ulong) v;
                Converter<uint, float>.Convert = v => (float) v;
                Converter<uint, double>.Convert = v => (double) v;
                Converter<uint, object>.Convert = v => (object) v;
            }

            static void RegisterUInt64Converters()
            {
                Converter<ulong, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<ulong, bool>.Convert = v => v != 0;
                Converter<ulong, sbyte>.Convert = v => (sbyte) Clamp(v, 0, sbyte.MaxValue);
                Converter<ulong, short>.Convert = v => (short) Clamp(v, 0, short.MaxValue);
                Converter<ulong, int>.Convert = v => (int) Clamp(v, 0, int.MaxValue);
                Converter<ulong, long>.Convert = v => (long) Clamp(v, 0, long.MaxValue);
                Converter<ulong, byte>.Convert = v => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<ulong, ushort>.Convert = v => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<ulong, uint>.Convert = v => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<ulong, ulong>.Convert = v => (ulong) v;
                Converter<ulong, float>.Convert = v => (float) v;
                Converter<ulong, double>.Convert = v => (double) v;
                Converter<ulong, object>.Convert = v => (object) v;
                Converter<ulong, string>.Convert = v => v.ToString();
            }

            static void RegisterFloat32Converters()
            {
                Converter<float, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<float, bool>.Convert = v => Math.Abs(v) > float.Epsilon;
                Converter<float, sbyte>.Convert = v => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<float, short>.Convert = v => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<float, int>.Convert = v => (int) Clamp(v, int.MinValue, int.MaxValue);
                Converter<float, long>.Convert = v => (long) Clamp(v, long.MinValue, long.MaxValue);
                Converter<float, byte>.Convert = v => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<float, ushort>.Convert = v => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<float, uint>.Convert = v => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<float, ulong>.Convert = v => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<float, float>.Convert = v => (float) v;
                Converter<float, double>.Convert = v => (double) v;
                Converter<float, object>.Convert = v => (object) v;
            }

            static void RegisterFloat64Converters()
            {
                Converter<double, char>.Convert = v => (char) Clamp(v, char.MinValue, char.MaxValue);
                Converter<double, bool>.Convert = v => Math.Abs(v) > double.Epsilon;
                Converter<double, sbyte>.Convert = v => (sbyte) Clamp(v, sbyte.MinValue, sbyte.MaxValue);
                Converter<double, short>.Convert = v => (short) Clamp(v, short.MinValue, short.MaxValue);
                Converter<double, int>.Convert = v => (int) Clamp(v, int.MinValue, int.MaxValue);
                Converter<double, long>.Convert = v => (long) Clamp(v, long.MinValue, long.MaxValue);
                Converter<double, byte>.Convert = v => (byte) Clamp(v, byte.MinValue, byte.MaxValue);
                Converter<double, ushort>.Convert = v => (ushort) Clamp(v, ushort.MinValue, ushort.MaxValue);
                Converter<double, uint>.Convert = v => (uint) Clamp(v, uint.MinValue, uint.MaxValue);
                Converter<double, ulong>.Convert = v => (ulong) Clamp(v, ulong.MinValue, ulong.MaxValue);
                Converter<double, float>.Convert = v => (float) Clamp(v, float.MinValue, float.MaxValue);
                Converter<double, double>.Convert = v => (double) v;
                Converter<double, object>.Convert = v => (object) v;
            }

            static void RegisterBooleanConverters()
            {
                Converter<bool, char>.Convert = v => v ? (char) 1 : (char) 0;
                Converter<bool, bool>.Convert = v => v;
                Converter<bool, sbyte>.Convert = v => v ? (sbyte) 1 : (sbyte) 0;
                Converter<bool, short>.Convert = v => v ? (short) 1 : (short) 0;
                Converter<bool, int>.Convert = v => v ? (int) 1 : (int) 0;
                Converter<bool, long>.Convert = v => v ? (long) 1 : (long) 0;
                Converter<bool, byte>.Convert = v => v ? (byte) 1 : (byte) 0;
                Converter<bool, ushort>.Convert = v => v ? (ushort) 1 : (ushort) 0;
                Converter<bool, uint>.Convert = v => v ? (uint) 1 : (uint) 0;
                Converter<bool, ulong>.Convert = v => v ? (ulong) 1 : (ulong) 0;
                Converter<bool, float>.Convert = v => v ? (float) 1 : (float) 0;
                Converter<bool, double>.Convert = v => v ? (double) 1 : (double) 0;
                Converter<bool, object>.Convert = v => (object) v;
            }
            
            static void RegisterVectorConverters()
            {
#if !UNITY_DOTSPLAYER
                Converter<UnityEngine.Vector2, UnityEngine.Vector2Int>.Convert = v => new UnityEngine.Vector2Int((int)v.x, (int)v.y);
                Converter<UnityEngine.Vector3, UnityEngine.Vector3Int>.Convert = v => new UnityEngine.Vector3Int((int)v.x, (int)v.y, (int)v.z);
                Converter<UnityEngine.Vector2Int, UnityEngine.Vector2>.Convert = v => v;
                Converter<UnityEngine.Vector3Int, UnityEngine.Vector3>.Convert = v => v;
#endif
            }

            static void RegisterCharConverters()
            {
                Converter<string, char>.Convert = v =>
                {
                    if (v.Length != 1)
                    {
                        throw new Exception("Not a valid char");
                    }

                    return v[0];
                };
                Converter<char, char>.Convert = v => v;
                Converter<char, bool>.Convert = v => v != (char) 0;
                Converter<char, sbyte>.Convert = v => (sbyte) v;
                Converter<char, short>.Convert = v => (short) v;
                Converter<char, int>.Convert = v => (int) v;
                Converter<char, long>.Convert = v => (long) v;
                Converter<char, byte>.Convert = v => (byte) v;
                Converter<char, ushort>.Convert = v => (ushort) v;
                Converter<char, uint>.Convert = v => (uint) v;
                Converter<char, ulong>.Convert = v => (ulong) v;
                Converter<char, float>.Convert = v => (float) v;
                Converter<char, double>.Convert = v => (double) v;
                Converter<char, object>.Convert = v => (object) v;
                Converter<char, string>.Convert = v => v.ToString();
            }

            static void RegisterStringConverters()
            {
                Converter<string, string>.Convert = v => v;
                Converter<string, char>.Convert = v => !string.IsNullOrEmpty(v) ? v[0] : '\0';
                Converter<char, string>.Convert = v => v.ToString();
                Converter<string, bool>.Convert = v =>
                {
                    if (bool.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, bool>(fromDouble)
                        : default;
                };
                Converter<bool, string>.Convert = v => v.ToString();
                Converter<string, sbyte>.Convert = v =>
                {
                    if (sbyte.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, sbyte>(fromDouble)
                        : default;
                };
                Converter<sbyte, string>.Convert = v => v.ToString();
                Converter<string, short>.Convert = v =>
                {
                    if (short.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, short>(fromDouble)
                        : default;
                };
                Converter<short, string>.Convert = v => v.ToString();
                Converter<string, int>.Convert = v =>
                {
                    if (int.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, int>(fromDouble)
                        : default;
                };
                Converter<int, string>.Convert = v => v.ToString();
                Converter<string, long>.Convert = v =>
                {
                    if (long.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, long>(fromDouble)
                        : default;
                };
                Converter<long, string>.Convert = v => v.ToString();
                Converter<string, byte>.Convert = v =>
                {
                    if (byte.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, byte>(fromDouble)
                        : default;
                };
                Converter<byte, string>.Convert = v => v.ToString();
                Converter<string, ushort>.Convert = v =>
                {
                    if (ushort.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, ushort>(fromDouble)
                        : default;
                };
                Converter<ushort, string>.Convert = v => v.ToString();
                Converter<string, uint>.Convert = v =>
                {
                    if (uint.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, uint>(fromDouble)
                        : default;
                };
                Converter<uint, string>.Convert = v => v.ToString();
                Converter<string, ulong>.Convert = v =>
                {
                    if (ulong.TryParse(v, out var r))
                    {
                        return r;
                    }

                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, ulong>(fromDouble)
                        : default;
                };
                Converter<ulong, string>.Convert = v => v.ToString();
                Converter<string, float>.Convert = v =>
                {
                    if (float.TryParse(v, out var r))
                        return r;
                    
                    return double.TryParse(v, out var fromDouble)
                        ? Convert<double, float>(fromDouble)
                        : default;
                };
                Converter<float, string>.Convert = v => v.ToString(CultureInfo.InvariantCulture);
                Converter<string, double>.Convert = v =>
                {
                    double.TryParse(v, out var r);
                    return r;
                };
                Converter<double, string>.Convert = v => v.ToString(CultureInfo.InvariantCulture);
                Converter<string, object>.Convert = v => v;
            }

            static void RegisterObjectConverters()
            {
                Converter<object, char>.Convert = v => v is char value ? value : default;
                Converter<object, bool>.Convert = v => v is bool value ? value : default;
                Converter<object, sbyte>.Convert = v => v is sbyte value ? value : default;
                Converter<object, short>.Convert = v => v is short value ? value : default;
                Converter<object, int>.Convert = v => v is int value ? value : default;
                Converter<object, long>.Convert = v => v is long value ? value : default;
                Converter<object, byte>.Convert = v => v is byte value ? value : default;
                Converter<object, ushort>.Convert = v => v is ushort value ? value : default;
                Converter<object, uint>.Convert = v => v is uint value ? value : default;
                Converter<object, ulong>.Convert = v => v is ulong value ? value : default;
                Converter<object, float>.Convert = v => v is float value ? value : default;
                Converter<object, double>.Convert = v => v is double value ? value : default;
                Converter<object, object>.Convert = v => v;
            } 
            
            static double Clamp(double value, double min, double max)
            {
                if (value < min)
                    value = min;
                else if (value > max)
                    value = max;
                return value;
            }
        }
    }
}