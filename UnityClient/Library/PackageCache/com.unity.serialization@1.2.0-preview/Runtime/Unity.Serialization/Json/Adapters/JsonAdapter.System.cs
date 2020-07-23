using System;
using System.Globalization;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        IJsonAdapter<Guid>,
        IJsonAdapter<DateTime>,
        IJsonAdapter<TimeSpan>
    {
        void IJsonAdapter<Guid>.Serialize(JsonStringBuffer writer, Guid value)
            => writer.WriteEncodedJsonString(value.ToString("N", CultureInfo.InvariantCulture));

        Guid IJsonAdapter<Guid>.Deserialize(SerializedValueView view)
            => Guid.TryParseExact(view.ToString(), "N", out var value) ? value : default;

        void IJsonAdapter<DateTime>.Serialize(JsonStringBuffer writer, DateTime value)
            => writer.WriteEncodedJsonString(value.ToString("o", CultureInfo.InvariantCulture));

        DateTime IJsonAdapter<DateTime>.Deserialize(SerializedValueView view)
            => DateTime.TryParseExact(view.ToString(), "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value) ? value : default;

        void IJsonAdapter<TimeSpan>.Serialize(JsonStringBuffer writer, TimeSpan value)
            => writer.WriteEncodedJsonString(value.ToString("c", CultureInfo.InvariantCulture));

        TimeSpan IJsonAdapter<TimeSpan>.Deserialize(SerializedValueView view)
            => TimeSpan.TryParseExact(view.ToString(), "c", CultureInfo.InvariantCulture, out var value) ? value : default;
    }
}