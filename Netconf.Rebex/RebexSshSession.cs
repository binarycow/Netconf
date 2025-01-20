using System.IO.Pipelines;
using Netconf.Netconf.Transport;
using Rebex.Net;

namespace Netconf;

internal sealed class RebexSshSession : ISshSession
{
    private bool disposed;
    private readonly SshSession ssh;
    private readonly SemaphoreSlim locker = new(1, 1);
    public RebexSshSession(SshChannel primarySession, SshSession ssh)
    {
        this.ssh = ssh;
        this.Pipe = new (new (primarySession));
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }
        this.disposed = true;
        this.Pipe.Dispose();
        this.ssh.Disconnect();
        this.ssh.Dispose();
    }

    public DuplexPipe Pipe { get; }
    IDuplexPipe ISshSession.Pipe => this.Pipe;
}