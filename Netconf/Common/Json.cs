using System.Text.Json;

namespace Netconf;

internal static class Json
{
    public static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerOptions.Default)
    {
        Converters =
        {
            new XNamespaceJsonConverter(),
            new OptionalRevisionDateJsonConverter(),
            new MaybeRevisionDateJsonConverter(),
        },
    };

}