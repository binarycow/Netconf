using Netconf.Netconf.Models;
using Netconf.Netconf.Transport;

namespace Netconf.Netconf;

public sealed partial class NetconfClient
{
    public static async Task<NetconfClient> ConnectAsync(
        ISshSession session,
        CancellationToken cancellationToken
    )
    {
        NetconfClient? client = null;
        IFramingProtocol framingProtocol = LegacyFramingProtocol.Instance;
        var clientHello = defaultClientHello;
        try
        {
            var clientTask = framingProtocol.WriteAsync(
                session.Pipe.Output,
                clientHello,
                cancellationToken
            );
            var serverTask = framingProtocol.ReadSingleMessageAsync<ServerHello>(
                session.Pipe.Input,
                cancellationToken
            ).AsTask();
            await Task.WhenAll(clientTask, serverTask);
            if (serverTask.Result is not { } serverHello)
            {
                throw new NotImplementedException();
            }
            framingProtocol = serverHello.Capabilities.Contains(Capability.Base__1_1)
                              && clientHello.Capabilities.Contains(Capability.Base__1_1)
                ? IFramingProtocol.Chunked
                : IFramingProtocol.Legacy;
            client = new(session, clientHello, serverHello, framingProtocol);
            return client;
        }
        catch (Exception e)
        {
            _ = e;
            if (client is not null)
            {
                await client.DisposeAsync();
            }
            session.Dispose();
            throw;
        }
    }
    public static async Task<NetconfClient> ConnectAsync(
        ISshFactory sshFactory,
        string host,
        string username,
        string password,
        int port,
        CancellationToken cancellationToken
    )
    {
        ISshSession? session = null;
        try
        {
            session = sshFactory.Connect(host, port, username, password);
            return await ConnectAsync(session, cancellationToken);
        }
        catch (Exception e)
        {
            _ = e;
            session?.Dispose();
            throw;
        }
    }
}