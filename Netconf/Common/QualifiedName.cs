namespace Netconf;

public readonly record struct QualifiedName(
    string ModuleName,
    string LocalName
);