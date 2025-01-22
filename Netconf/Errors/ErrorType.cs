using System.Text.Json.Serialization;

namespace Netconf;

[JsonConverter(typeof(ErrorTypeJsonConverter))]
public enum ErrorType
{
    Unknown,
    Transport,
    Rpc,
    Protocol,
    Application,
}

internal sealed class ErrorTypeJsonConverter 
    : EnumJsonConverter<ErrorTypeJsonConverter, ErrorType>,
        IEnumJsonConverter<ErrorTypeJsonConverter, ErrorType>
{
    private const string ApplicationString = "application";
    private const string TransportString = "transport";
    private const string ProtocolString = "protocol";
    private const string RpcString = "rpc";
    private static ReadOnlySpan<byte> ApplicationBytes => "application"u8;
    private static ReadOnlySpan<byte> TransportBytes => "transport"u8;
    private static ReadOnlySpan<byte> ProtocolBytes => "protocol"u8;
    private static ReadOnlySpan<byte> RpcBytes => "rpc"u8;
    
    public static int MaximumLength => 11; // application
    public static ErrorType FromSpan(scoped ReadOnlySpan<char> span) => span switch
    {
        ApplicationString => ErrorType.Application,
        TransportString => ErrorType.Transport,
        ProtocolString => ErrorType.Protocol,
        RpcString => ErrorType.Rpc,
        _ => ErrorType.Unknown,
    };

    public static ReadOnlySpan<byte> ToBytes(ErrorType value) => value switch
    {
        ErrorType.Application => ApplicationBytes,
        ErrorType.Transport => TransportBytes,
        ErrorType.Protocol => ProtocolBytes,
        ErrorType.Rpc => RpcBytes,
        _ => [],
    };
}