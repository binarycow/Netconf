using System.IO.Pipelines;

namespace Netconf.Netconf.Transport;

public interface ISshSession : IDisposable
{
    public IDuplexPipe NetconfSubsystem { get; }
}