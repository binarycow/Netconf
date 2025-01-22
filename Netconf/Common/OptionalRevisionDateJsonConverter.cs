using System.Text.Json;
using System.Text.Json.Serialization;

namespace Netconf;

internal sealed class MaybeRevisionDateJsonConverter : JsonConverter<Maybe<DateOnly>>
{
    public override bool HandleNull => true;

    public override Maybe<DateOnly> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
    ) => reader.TokenType switch
    {
        JsonTokenType.Null => default,
        JsonTokenType.String => Maybe.CreateIfNotNull(OptionalRevisionDateJsonConverter.FromString(ref reader)),
        _ => throw new JsonException(),
    };

    public override void Write(Utf8JsonWriter writer, Maybe<DateOnly> value, JsonSerializerOptions options)
    {
        if (value.HasValue is false)
        {
            writer.WriteStringValue(default(ReadOnlySpan<byte>));
            return;
        }

        OptionalRevisionDateJsonConverter.Write(writer, value.Value);
    }
}
internal sealed class OptionalRevisionDateJsonConverter : JsonConverter<DateOnly?>
{
    public override bool HandleNull => true;

    public override DateOnly? Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
    ) => reader.TokenType switch
    {
        JsonTokenType.Null => null,
        JsonTokenType.String => FromString(ref reader),
        _ => throw new JsonException(),
    };


    private const string FormatString = Constants.RevisionDateFormatString;

    internal static DateOnly? FromString(ref Utf8JsonReader reader)
    {
        var length = reader.HasValueSequence
            ? reader.ValueSequence.Length 
            : reader.ValueSpan.Length;
        if (length is 0)
        {
            return null;
        }
        if (length > FormatString.Length)
        {
            throw new JsonException();
        }
        Span<char> chars = stackalloc char[FormatString.Length];
        length = reader.CopyString(chars);
        chars = chars[..(int)length];
        return DateOnly.ParseExact(chars, FormatString);
    }

    internal static void Write(Utf8JsonWriter writer, DateOnly? value)
    {
        if (value is not { } nonNull)
        {
            writer.WriteStringValue(default(ReadOnlySpan<byte>));
            return;
        }
        Span<char> chars = stackalloc char[FormatString.Length];
        nonNull.TryFormat(chars, out var written, FormatString);
        chars = chars[..written];
        writer.WriteStringValue(chars);
    }
    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options) 
        => Write(writer, value);
}