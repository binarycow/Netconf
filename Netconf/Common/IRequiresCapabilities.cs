using Netconf.Netconf;

namespace Netconf;

public interface IRequiresCapabilities
{
    public IReadOnlyList<Capability> RequiredCapabilities { get; }
}
public interface IRequiresCapabilities<TSelf> : IRequiresCapabilities
    where TSelf : IRequiresCapabilities<TSelf>
{
    public new static abstract IReadOnlyList<Capability> RequiredCapabilities { get; }

    IReadOnlyList<Capability> IRequiresCapabilities.RequiredCapabilities => TSelf.RequiredCapabilities;
}