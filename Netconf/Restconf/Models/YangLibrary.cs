using System.Xml.Linq;
using JetBrains.Annotations;

namespace Netconf.Restconf.Models;

[PublicAPI]
public sealed record YangLibrary(
    string ContentId,
    IReadOnlyDictionary<string, YangLibraryModuleSet> ModuleSets,
    IReadOnlyDictionary<string, YangLibrarySchema> Schemas,
    IReadOnlyDictionary<string, YangLibraryDatastore> Datastores    
)
{
    internal static YangLibrary Create(YangLibraryDto model)
    {
        var moduleSets = model.ModuleSets
            .Select(YangLibraryModuleSet.Create)
            .ToReadOnlyDictionary(static x => x.Name);
        var schemas = model.Schemas
            .Select(YangLibrarySchema.Create, moduleSets)
            .ToReadOnlyDictionary(static x => x.Name);
        return new(
            ContentId: model.ContentId,
            ModuleSets: moduleSets,
            Schemas: schemas,
            Datastores: model.Datastores
                .Select(YangLibraryDatastore.Create, schemas)
                .ToReadOnlyDictionary(static x => x.Name)
        );
    }

    private const string LegacyName = "common";
    internal static YangLibrary Create(ModulesState model)
    {
        var dtoModules = model.Modules
            .Where(static x => x.ConformanceType is ConformanceType.Implement)
            .ToDictionary(static x => x.Name);

        var moduleSet = new YangLibraryModuleSet(
            Name: LegacyName,
            ImplementedModules: dtoModules
                .Values
                .Select(YangLibraryImplementedModule.Create)
                .ToReadOnlyDictionary(static x => x.Name),
            ImportedModules: model.Modules
                .Where(static x => x.ConformanceType is ConformanceType.Import)
                .Select(YangLibraryImportedModule.Create)
                .ToReadOnlyDictionary(static x => new ModuleRevisionInfo(x.Name, x.Revision))
        );
        foreach (var (name, module) in moduleSet.ImplementedModules)
        {
            module.Deviations = dtoModules[name].Deviations.Select(
                static (deviation, modules) => modules[deviation.Name],
                moduleSet.ImplementedModules
            ).ToReadOnlyList();
        }
        var schema = new YangLibrarySchema(
            LegacyName,
            [moduleSet]
        );
        var datastore = new YangLibraryDatastore(
            Name: LegacyName,
            Schema: schema
        );
        return new(
            ContentId: model.ModuleSetId,
            ModuleSets: new Dictionary<string, YangLibraryModuleSet>
            {
                { moduleSet.Name, moduleSet },
            }.AsReadOnly(),
            Schemas: new Dictionary<string, YangLibrarySchema>
            {
                { schema.Name, schema },
            }.AsReadOnly(),
            Datastores: new Dictionary<string, YangLibraryDatastore>
            {
                { schema.Name, datastore },
            }.AsReadOnly()
        );
    }
}

[PublicAPI]
public sealed record YangLibraryModuleSet(
    string Name,
    IReadOnlyDictionary<string, YangLibraryImplementedModule> ImplementedModules,
    IReadOnlyDictionary<ModuleRevisionInfo, YangLibraryImportedModule> ImportedModules    
)
{
    internal static YangLibraryModuleSet Create(YangLibraryModuleSetDto model)
    {
        var dtoModules = model.ImplementedModules.ToDictionary(static x => x.Name);
        var modules = model.ImplementedModules
            .Select(YangLibraryImplementedModule.Create)
            .ToReadOnlyDictionary(static x => x.Name);
        foreach (var (name, module) in modules)
        {
            module.Deviations = dtoModules[name].DeviationModuleNames
                .Select(static (a, b) => b[a], modules)
                .ToReadOnlyList();
        }
        return new(
            Name: model.Name,
            ImplementedModules: modules,
            ImportedModules: model.ImportedModules
                .Select(YangLibraryImportedModule.Create)
                .ToReadOnlyDictionary(static x => new ModuleRevisionInfo(x.Name, x.Revision))
        );
    }
}

[PublicAPI]
public sealed record YangLibraryImplementedModule(
    string Name,
    DateOnly? Revision,
    XNamespace Namespace,
    IReadOnlyList<Uri> Locations,
    IReadOnlyList<string> Features,
    IReadOnlyDictionary<string, YangLibrarySubmodule> Submodules
)
{
    public IReadOnlyList<YangLibraryImplementedModule> Deviations { get; internal set; } = [];
    public ModuleRevisionInfo RevisionInfo => new ModuleRevisionInfo(this.Name, this.Revision);

    internal static YangLibraryImplementedModule Create(YangLibraryImplementedModuleDto model) => new(
        Name: model.Name,
        Revision: model.Revision,
        Namespace: model.Namespace,
        Locations: model.Locations,
        Features: model.FeatureNames,
        Submodules: model.Submodules.ToReadOnlyDictionary(static x => x.Name)
    );

    internal static YangLibraryImplementedModule Create(ModuleSetModule model) => new(
        Name: model.Name,
        Revision: model.Revision,
        Namespace: model.Namespace,
        Locations: model.Schema is { } schema ? [schema] : [],
        Features: model.Features,
        Submodules: model.Submodules.Select(YangLibrarySubmodule.Create).ToReadOnlyDictionary(static x => x.Name) 
    );
}


public sealed record YangLibraryImportedModule(
    string Name,
    DateOnly? Revision,
    XNamespace Namespace,
    IReadOnlyList<Uri> Locations,
    IReadOnlyDictionary<string, YangLibrarySubmodule> Submodules
)
{

    internal static YangLibraryImportedModule Create(ModuleSetModule model) => new(
        Name: model.Name,
        Revision: model.Revision,
        Namespace: model.Namespace,
        Locations: model.Schema is { } schema ? [schema] : [],
        Submodules: model.Submodules.Select(YangLibrarySubmodule.Create)
            .ToReadOnlyDictionary(static x => x.Name) 
    );

    internal static YangLibraryImportedModule Create(YangLibraryImportedModuleDto model) => new(
        Name: model.Name,
        Revision: model.Revision,
        Namespace: model.Namespace,
        Locations: model.Locations,
        Submodules: model.Submodules
            .ToReadOnlyDictionary(static x => x.Name) 
    );
}

[PublicAPI]
public sealed record YangLibrarySchema(
    string Name,
    IReadOnlyList<YangLibraryModuleSet> ModuleSets
)
{
    internal static YangLibrarySchema Create(
        YangLibrarySchemaDto model,
        IReadOnlyDictionary<string, YangLibraryModuleSet> moduleSets
    ) => new(
        Name: model.Name,
        ModuleSets: model.ModuleSetNames.Select(
            static (a, b) => b[a],
            moduleSets
        ).ToReadOnlyList()
    );
}

[PublicAPI]
public sealed record YangLibraryDatastore(
    string Name,
    YangLibrarySchema Schema
)
{
    internal static YangLibraryDatastore Create(
        YangLibraryDatastoreDto model, 
        IReadOnlyDictionary<string, YangLibrarySchema> schemas
    ) => new(
        Name: model.Name,
        Schema: schemas[model.Name]
    );
}