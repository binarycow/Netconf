using System.Collections.Immutable;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Netconf.Restconf.Models;

internal sealed record YangLibraryDto(
    [property: JsonPropertyName("content-id")]
    [property: JsonRequired]
    string ContentId,
    IReadOnlyList<YangLibraryModuleSetDto>? ModuleSets,
    IReadOnlyList<YangLibrarySchemaDto>? Schemas,
    IReadOnlyList<YangLibraryDatastoreDto>? Datastores

)
{
    [property: JsonPropertyName("module-set")]
    public IReadOnlyList<YangLibraryModuleSetDto> ModuleSets { get; init; }
        = ModuleSets ?? ImmutableList<YangLibraryModuleSetDto>.Empty;
    [property: JsonPropertyName("schema")]
    public IReadOnlyList<YangLibrarySchemaDto> Schemas { get; init; }
        = Schemas ?? ImmutableList<YangLibrarySchemaDto>.Empty;
    [property: JsonPropertyName("datastore")]
    public IReadOnlyList<YangLibraryDatastoreDto> Datastores { get; init; }
        = Datastores ?? ImmutableList<YangLibraryDatastoreDto>.Empty;
}

internal sealed record YangLibrarySchemaDto(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    IReadOnlyList<string>? ModuleSetNames
)
{
    [JsonPropertyName("module-set")]
    public IReadOnlyList<string> ModuleSetNames { get; init; } = ModuleSetNames ?? ImmutableList<string>.Empty;
}
internal sealed record YangLibraryDatastoreDto(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    [property: JsonPropertyName("schema")]
    [property: JsonRequired]
    string SchemaName
);

internal sealed record YangLibraryModuleSetDto(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    IReadOnlyList<YangLibraryImplementedModuleDto>? ImplementedModules,
    IReadOnlyList<YangLibraryImportedModuleDto>? ImportedModules
)
{
    [property: JsonPropertyName("module")]
    public IReadOnlyList<YangLibraryImplementedModuleDto> ImplementedModules { get; init; }
        = ImplementedModules ?? ImmutableList<YangLibraryImplementedModuleDto>.Empty;
    [property: JsonPropertyName("import-only-module")]
    public IReadOnlyList<YangLibraryImportedModuleDto> ImportedModules { get; init; }
        = ImportedModules ?? ImmutableList<YangLibraryImportedModuleDto>.Empty;
}

internal sealed record YangLibraryImplementedModuleDto(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    [property: JsonPropertyName("revision")]
    [property: JsonConverter(typeof(OptionalRevisionDateJsonConverter))]
    DateOnly? Revision,
    [property: JsonPropertyName("namespace")]
    XNamespace Namespace,
    IReadOnlyList<Uri>? Locations,
    IReadOnlyList<YangLibrarySubmodule>? Submodules,
    IReadOnlyList<string>? FeatureNames,
    IReadOnlyList<string>? DeviationModuleNames
)
{
    [property: JsonPropertyName("submodule")]
    public IReadOnlyList<YangLibrarySubmodule> Submodules { get; init; } = Submodules ?? ImmutableList<YangLibrarySubmodule>.Empty;
    [property: JsonPropertyName("location")]
    public IReadOnlyList<Uri> Locations { get; init; } = Locations ?? ImmutableList<Uri>.Empty;
    [property: JsonPropertyName("feature")]
    public IReadOnlyList<string> FeatureNames { get; init; } = FeatureNames ?? ImmutableList<string>.Empty;
    [property: JsonPropertyName("deviation")]
    public IReadOnlyList<string> DeviationModuleNames { get; init; } = DeviationModuleNames ?? ImmutableList<string>.Empty;
}

[PublicAPI]
public sealed record YangLibrarySubmodule(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    [property: JsonPropertyName("revision")]
    [property: JsonConverter(typeof(OptionalRevisionDateJsonConverter))]
    DateOnly? RevisionDate,
    IReadOnlyList<Uri>? Locations
)
{
    [property: JsonPropertyName("location")]
    public IReadOnlyList<Uri> Locations { get; init; } = Locations ?? ImmutableList<Uri>.Empty;

    internal static YangLibrarySubmodule Create(ModuleSetSubmodule model) => new(
        model.Name,
        model.Revision,
        model.Schema is { } schema ? [schema] : []
    );
}


internal sealed record YangLibraryImportedModuleDto(
    [property: JsonPropertyName("name")]
    [property: JsonRequired]
    string Name,
    [property: JsonPropertyName("revision")]
    [property: JsonConverter(typeof(OptionalRevisionDateJsonConverter))]
    DateOnly? Revision,
    [property: JsonPropertyName("namespace")]
    XNamespace Namespace,
    IReadOnlyList<Uri>? Locations,
    IReadOnlyList<YangLibrarySubmodule>? Submodules
)
{
    [property: JsonPropertyName("submodule")]
    public IReadOnlyList<YangLibrarySubmodule> Submodules { get; init; } = Submodules ?? ImmutableList<YangLibrarySubmodule>.Empty;
    [property: JsonPropertyName("location")]
    public IReadOnlyList<Uri> Locations { get; init; } = Locations ?? ImmutableList<Uri>.Empty;
}