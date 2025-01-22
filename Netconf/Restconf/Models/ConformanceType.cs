using System.Text.Json;
using System.Text.Json.Serialization;

namespace Netconf.Restconf.Models;

[JsonConverter(typeof(ConformanceTypeJsonConverter))]
public enum ConformanceType
{
    Unknown,
    Implement,
    Import,
}

internal sealed class ConformanceTypeJsonConverter
    : EnumJsonConverter<ConformanceTypeJsonConverter, ConformanceType>,
        IEnumJsonConverter<ConformanceTypeJsonConverter, ConformanceType>
{
    private const string ImplementString = "implement";
    private const string ImportString = "import";
    private static ReadOnlySpan<byte> ImplementBytes => "implement"u8;
    private static ReadOnlySpan<byte> ImportBytes => "import"u8;
    public static int MaximumLength => 9; // implement
    public static ConformanceType FromSpan(scoped ReadOnlySpan<char> span) => span switch
    {
        ImplementString => ConformanceType.Implement,
        ImportString => ConformanceType.Import,
        _ => ConformanceType.Unknown,
    };

    public static ReadOnlySpan<byte> ToBytes(ConformanceType value) => value switch
    {
        ConformanceType.Implement => ImplementBytes,
        ConformanceType.Import => ImportBytes,
        _ => [],
    };
}