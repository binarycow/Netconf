using System.Text.Json.Nodes;

namespace Netconf;

internal interface IJsonParsable<out TSelf>
{
    public static abstract TSelf FromJsonNode(JsonNode node);
}