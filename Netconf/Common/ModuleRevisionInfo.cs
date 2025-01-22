namespace Netconf;

public readonly record struct ModuleRevisionInfo(
    string ModuleName,
    DateOnly? RevisionDate
)
{
    public override string ToString()
        => this.RevisionDate is { } rev 
            ? $"{this.ModuleName}@{rev.ToString(Constants.RevisionDateFormatString)}" 
            : this.ModuleName;
}