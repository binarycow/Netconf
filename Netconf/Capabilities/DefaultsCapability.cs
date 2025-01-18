namespace Netconf;

public sealed class DefaultsCapability : Capability
{
    internal DefaultsCapability(
        string value,
        DefaultsCapabilityBasicMode basicMode,
        DefaultsCapabilityAlsoSupported alsoSupported
    ) : base(value)
    {
        this.BasicMode = basicMode;
        this.AlsoSupported = alsoSupported;
    }
    public DefaultsCapabilityBasicMode BasicMode { get; }
    public DefaultsCapabilityAlsoSupported AlsoSupported { get; }
}