using System;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        IJsonAdapter<sbyte>,
        IJsonAdapter<short>,
        IJsonAdapter<int>,
        IJsonAdapter<long>,
        IJsonAdapter<byte>,
        IJsonAdapter<ushort>,
        IJsonAdapter<uint>,
        IJsonAdapter<ulong>,
        IJsonAdapter<float>,
        IJsonAdapter<double>,
        IJsonAdapter<bool>,
        IJsonAdapter<char>,
        IJsonAdapter<string>
    {
        void IJsonAdapter<sbyte>.Serialize(JsonStringBuffer writer, sbyte value) => writer.Write(value);
        void IJsonAdapter<short>.Serialize(JsonStringBuffer writer, short value) => writer.Write(value);
        void IJsonAdapter<int>.Serialize(JsonStringBuffer writer, int value) => writer.Write(value);
        void IJsonAdapter<long>.Serialize(JsonStringBuffer writer, long value) => writer.Write(value);
        void IJsonAdapter<byte>.Serialize(JsonStringBuffer writer, byte value) => writer.Write(value);
        void IJsonAdapter<ushort>.Serialize(JsonStringBuffer writer, ushort value) => writer.Write(value);
        void IJsonAdapter<uint>.Serialize(JsonStringBuffer writer, uint value) => writer.Write(value);
        void IJsonAdapter<ulong>.Serialize(JsonStringBuffer writer, ulong value) => writer.Write(value);
        void IJsonAdapter<float>.Serialize(JsonStringBuffer writer, float value) => writer.Write(value);
        void IJsonAdapter<double>.Serialize(JsonStringBuffer writer, double value) => writer.Write(value);
        void IJsonAdapter<bool>.Serialize(JsonStringBuffer writer, bool value) => writer.Write(value ? "true" : "false");
        void IJsonAdapter<char>.Serialize(JsonStringBuffer writer, char value) => writer.WriteEncodedJsonString(value);
        void IJsonAdapter<string>.Serialize(JsonStringBuffer writer, string value) => writer.WriteEncodedJsonString(value);
        
        sbyte IJsonAdapter<sbyte>.Deserialize(SerializedValueView view) 
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        short IJsonAdapter<short>.Deserialize(SerializedValueView view) 
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        int IJsonAdapter<int>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        long IJsonAdapter<long>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        byte IJsonAdapter<byte>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        ushort IJsonAdapter<ushort>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        uint IJsonAdapter<uint>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        ulong IJsonAdapter<ulong>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        float IJsonAdapter<float>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        double IJsonAdapter<double>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        bool IJsonAdapter<bool>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");

        char IJsonAdapter<char>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
        
        string IJsonAdapter<string>.Deserialize(SerializedValueView view)
            => throw new NotImplementedException($"This code should never be executed. {nameof(JsonPropertyReader)} should handle primitives in a specialized way.");
    }
}