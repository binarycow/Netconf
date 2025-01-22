using System.Text.Json;
using System.Text.Json.Serialization;

namespace Netconf;

internal interface IEnumJsonConverter<TSelf, T>
    where TSelf : IEnumJsonConverter<TSelf, T>
    where T : struct, Enum
{
    public static abstract int MaximumLength { get; }
    public static abstract T FromSpan(scoped ReadOnlySpan<char> span);
    public static abstract ReadOnlySpan<byte> ToBytes(T value);
}

internal abstract class EnumJsonConverter<TSelf, T> : JsonConverter<T>
    where TSelf : IEnumJsonConverter<TSelf, T>
    where T : struct, Enum
{
    public override void Write(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(TSelf.ToBytes(value));

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Span<char> chars = stackalloc char[TSelf.MaximumLength];
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException();
        var actualLength = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;
        if (actualLength > TSelf.MaximumLength)
            throw new JsonException();
        actualLength = reader.CopyString(chars);
        chars = chars[..(int)actualLength];
        return TSelf.FromSpan(chars);
    }
}