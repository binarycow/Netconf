using System.IO.Pipelines;
using Netconf.Netconf.Transport;

namespace Netconf;

internal sealed class RebexSshSession : ISshSession
{
    public RebexSshSession(DuplexPipe pipe) => this.NetconfSubsystem = pipe;
    public void Dispose() => this.NetconfSubsystem.Dispose();
    public DuplexPipe NetconfSubsystem { get; }
    IDuplexPipe ISshSession.NetconfSubsystem => this.NetconfSubsystem;
}