using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Netconf.Restconf.Models;

internal sealed record ModulesState(
    [property: JsonPropertyName("module-set-id")]
    [property: JsonRequired]
    string ModuleSetId,

    [property: JsonPropertyName("module")]
    IReadOnlyList<ModuleSetModule> Modules
);

internal sealed record ModuleSetModule(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,

    [property: JsonPropertyName("revision")]
    [property: JsonConverter(typeof(OptionalRevisionDateJsonConverter))]
    [property: JsonRequired]
    DateOnly? Revision,

    [property: JsonPropertyName("schema")] 
    Uri? Schema,

    [property: JsonPropertyName("namespace")]
    [property: JsonRequired]
    XNamespace Namespace,

    [property: JsonPropertyName("conformance-type")]
    [property: JsonRequired]
    ConformanceType ConformanceType,

    IReadOnlyList<ModuleSetSubmodule>? Submodules,
    IReadOnlyList<string>? Features,
    IReadOnlyList<ModuleSetDeviation>? Deviations
)
{
    [JsonPropertyName("feature")]
    public IReadOnlyList<string> Features { get; } = Features ?? [];

    [JsonPropertyName("deviation")]
    public IReadOnlyList<ModuleSetDeviation> Deviations { get; } = Deviations ?? [];

    [JsonPropertyName("submodule")]
    public IReadOnlyList<ModuleSetSubmodule> Submodules { get; } = Submodules ?? [];
}

internal sealed record ModuleSetSubmodule(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    
    [property: JsonPropertyName("revision")]
    [property: JsonConverter(typeof(OptionalRevisionDateJsonConverter))]
    DateOnly? Revision,
    
    [property: JsonPropertyName("schema")]
    Uri? Schema
);

internal sealed record ModuleSetDeviation(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    
    [property: JsonPropertyName("revision")]
    [property: JsonConverter(typeof(OptionalRevisionDateJsonConverter))]
    DateOnly? Revision
);