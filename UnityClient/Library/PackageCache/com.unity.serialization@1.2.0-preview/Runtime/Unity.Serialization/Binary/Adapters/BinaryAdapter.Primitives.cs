using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Binary.Adapters
{
    unsafe partial class BinaryAdapter :
        IBinaryAdapter<sbyte>, 
        IBinaryAdapter<short>, 
        IBinaryAdapter<int>,   
        IBinaryAdapter<long>,  
        IBinaryAdapter<byte>,  
        IBinaryAdapter<ushort>,
        IBinaryAdapter<uint>,  
        IBinaryAdapter<ulong>, 
        IBinaryAdapter<float>, 
        IBinaryAdapter<double>,
        IBinaryAdapter<bool>,  
        IBinaryAdapter<char>,  
        IBinaryAdapter<string>
    {
        void IBinaryAdapter<sbyte>.Serialize(UnsafeAppendBuffer* writer, sbyte value)
            => writer->Add(value);

        void IBinaryAdapter<short>.Serialize(UnsafeAppendBuffer* writer, short value)
            => writer->Add(value);

        void IBinaryAdapter<int>.Serialize(UnsafeAppendBuffer* writer, int value)
            => writer->Add(value);

        void IBinaryAdapter<long>.Serialize(UnsafeAppendBuffer* writer, long value)
            => writer->Add(value);

        void IBinaryAdapter<byte>.Serialize(UnsafeAppendBuffer* writer, byte value)
            => writer->Add(value);

        void IBinaryAdapter<ushort>.Serialize(UnsafeAppendBuffer* writer, ushort value)
            => writer->Add(value);

        void IBinaryAdapter<uint>.Serialize(UnsafeAppendBuffer* writer, uint value)
            => writer->Add(value);

        void IBinaryAdapter<ulong>.Serialize(UnsafeAppendBuffer* writer, ulong value)
            => writer->Add(value);

        void IBinaryAdapter<float>.Serialize(UnsafeAppendBuffer* writer, float value)
            => writer->Add(value);

        void IBinaryAdapter<double>.Serialize(UnsafeAppendBuffer* writer, double value)
            => writer->Add(value);

        void IBinaryAdapter<bool>.Serialize(UnsafeAppendBuffer* writer, bool value)
            => writer->Add((byte)(value ? 1 : 0));

        void IBinaryAdapter<char>.Serialize(UnsafeAppendBuffer* writer, char value)
            => writer->Add(value);
        
        void IBinaryAdapter<string>.Serialize(UnsafeAppendBuffer* writer, string value)
            => writer->Add(value);

        sbyte IBinaryAdapter<sbyte>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<sbyte>();

        short IBinaryAdapter<short>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<short>();

        int IBinaryAdapter<int>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<int>();

        long IBinaryAdapter<long>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<long>();

        byte IBinaryAdapter<byte>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<byte>();

        ushort IBinaryAdapter<ushort>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<ushort>();

        uint IBinaryAdapter<uint>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<uint>();

        ulong IBinaryAdapter<ulong>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<ulong>();

        float IBinaryAdapter<float>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<float>();

        double IBinaryAdapter<double>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<double>();

        bool IBinaryAdapter<bool>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<byte>() == 1;

        char IBinaryAdapter<char>.Deserialize(UnsafeAppendBuffer.Reader* reader)
            => reader->ReadNext<char>();

        string IBinaryAdapter<string>.Deserialize(UnsafeAppendBuffer.Reader* reader)
        {
            reader->ReadNext(out string value);
            return value;
        }
    }
}