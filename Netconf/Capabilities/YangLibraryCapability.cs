namespace Netconf;

public sealed class YangLibraryCapability : Capability
{
    public string ContentId { get; }

    internal YangLibraryCapability(
        string value,
        string contentId
    ) : base(value)
    {
        this.ContentId = contentId;
    }
}