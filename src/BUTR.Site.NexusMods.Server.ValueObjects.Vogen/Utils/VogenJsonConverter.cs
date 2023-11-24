namespace BUTR.Site.NexusMods.Server.ValueObjects.Utils;

public class VogenJsonConverter<TVogen, TValueObject> : JsonConverter<TVogen>
    where TVogen : struct, IVogen<TVogen, TValueObject>, IHasIsInitialized<TVogen>, IEquatable<TVogen>, IEquatable<TValueObject>, IComparable<TVogen>
    where TValueObject : notnull
{
    public override TVogen Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var primitive = JsonSerializer.Deserialize<TValueObject>(ref reader, options)!;
        return TVogen.DeserializeDangerous(primitive);
    }

    public override void Write(Utf8JsonWriter writer, TVogen value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (TValueObject) value, options);
    }

    public override TVogen ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //return Read(ref reader, typeToConvert, options);

        var primitive = JsonSerializer.Deserialize<TValueObject>(ref reader, options)!;
        return TVogen.DeserializeDangerous(primitive);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TVogen value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(JsonSerializer.Serialize((TValueObject) value, options));
    }
}