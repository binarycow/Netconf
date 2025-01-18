using Netconf.Netconf.Transport;
using Rebex;
using Rebex.Net;

namespace Netconf;

public sealed class RebexSshFactory : ISshFactory
{
    private readonly CreateLogWriter? createLogWriter;

    public delegate ILogWriter CreateLogWriter(string host, int port, string username);
    public RebexSshFactory(CreateLogWriter? createLogWriter)
    {
        this.createLogWriter = createLogWriter;
    }
    public static RebexSshFactory Default { get; } = new(null);
    public static void SetLicenseKey(string licenseKey) => Licensing.Key = licenseKey;
    private const string DebugLogPath = @"S:\DotNetProjects\Netconf\Demo\logs.txt";

    public ISshSession Connect(
        string host,
        int port,
        string username,
        string password
    )
    {
        File.WriteAllText(DebugLogPath, string.Empty);
        SshSession? ssh = null;
        SshChannel? channel = null;
        RebexStream? stream = null;
        DuplexPipe? pipe = null;
        try
        {
            ssh = new();
            ssh.LogWriter = this.createLogWriter?.Invoke(host, port, username);
            ssh.Connect(host, port);
            ssh.Authenticate(username, password);
            channel = ssh.OpenSession();
            channel.RequestSubsystem("netconf");
            stream = new (ssh, channel);
            pipe = new (stream);
            return new RebexSshSession(pipe);
        }
        catch (Exception e)
        {
            _ = e;
            pipe?.Dispose();
            stream?.Dispose();
            channel?.Dispose();
            ssh?.Dispose();
            throw;
        }
    }
}