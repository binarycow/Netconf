namespace Netconf;

public sealed class ModuleCapability : Capability
{
    internal ModuleCapability(
        string value,
        string moduleName,
        DateOnly? revision,
        IReadOnlyList<string>? deviations,
        IReadOnlyList<string>? features
    ) : base(value)
    {
        this.ModuleName = moduleName;
        this.Revision = revision;
        this.Deviations = deviations ?? [];
        this.Features = features ?? [];
    }

    public string ModuleName { get; }
    public DateOnly? Revision { get; }
    public IReadOnlyList<string> Deviations { get; }
    public IReadOnlyList<string> Features { get; }
}