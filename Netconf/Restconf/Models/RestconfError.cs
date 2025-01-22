using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Netconf.Restconf.Models;

internal sealed record RestconfErrorListWrapper(
    [property: JsonPropertyName("ietf-restconf:errors")]
    RestconfErrorListObject Errors
);

internal sealed record RestconfErrorListObject(
    [property: JsonPropertyName("error")]
    IReadOnlyList<RestconfError> Errors
);

internal sealed record RestconfError(
    [property: JsonPropertyName("error-type")] ErrorType Type,
    [property: JsonPropertyName("error-tag")] string? Tag,
    [property: JsonPropertyName("error-app-tag")] string? AppTag,
    [property: JsonPropertyName("error-path")] string? Path,
    [property: JsonPropertyName("error-message")] string? Message,
    [property: JsonPropertyName("error-info")] object? Info
) : IRpcError
{
    public HttpStatusCode StatusCode { get; internal set; }
    string? IRpcError.MessageLanguage => null;
    ErrorSeverity IRpcError.Severity => ErrorSeverity.Unknown;
}

internal static class RestconfErrorExtensions
{
    public static T AddStatusCode<T>(this T errors, HttpStatusCode statusCode)
        where T : IReadOnlyCollection<RestconfError>
    {
        foreach (var error in errors)
        {
            error.StatusCode = statusCode;
        }
        return errors;
    }
}

