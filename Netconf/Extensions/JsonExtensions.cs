using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Netconf;

internal static class JsonExtensions
{
    public static bool TryGetPropertyValue<TNode>(
        this JsonObject obj,
        string propertyName,
        [NotNullWhen(true)] out TNode? result
    )
        where TNode : JsonNode
    {
        result = default;
        if (!obj.TryGetPropertyValue(propertyName, out var node))
            return false;
        result = node as TNode;
        return result is not null;
    }
    
    public static TNode? GetPropertyValue<TNode>(this JsonObject obj, string propertyName)
        where TNode : JsonNode
        => obj.TryGetPropertyValue(propertyName, out var node) ? null : node as TNode;
    public static JsonNode? GetPropertyValue(this JsonObject obj, string propertyName)
        => obj.TryGetPropertyValue(propertyName, out var node) ? null : node;
}