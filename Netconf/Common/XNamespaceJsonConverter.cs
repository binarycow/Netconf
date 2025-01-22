using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Netconf;

public sealed class XNamespaceJsonConverter : JsonConverter<XNamespace>
{
    public override XNamespace Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => XNamespace.Get(reader.GetString() ?? throw new JsonException());

    public override void Write(Utf8JsonWriter writer, XNamespace value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.NamespaceName);
}