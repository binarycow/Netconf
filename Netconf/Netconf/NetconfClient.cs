using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using System.Xml.Linq;
using Netconf.Netconf.Models;
using Netconf.Netconf.Transport;

namespace Netconf.Netconf;

public sealed partial class NetconfClient : IClient
{
    public ClientHello ClientHello { get; }
    public ServerHello ServerHello { get; }
    private readonly ISshSession session;
    private readonly IFramingProtocol framingProtocol;
    private ulong nextMessageId;
    private readonly ConcurrentDictionary<string, RpcListener> listeners = new();
    private string GetNextMessageId() => Interlocked.Increment(ref this.nextMessageId).ToString();
    private readonly Channel<RpcMessage> outgoingMessages = Channel.CreateUnbounded<RpcMessage>();
    private readonly Channel<RpcMessage> incomingMessages = Channel.CreateUnbounded<RpcMessage>();
    private bool loggedOut;

    private NetconfClient(
        ISshSession session,
        ClientHello clientHello,
        ServerHello serverHello,
        IFramingProtocol framingProtocol
    )
    {
        this.ClientHello = clientHello;
        this.ServerHello = serverHello;
        this.session = session;
        this.framingProtocol = framingProtocol;
        ;
    }

    private void Start()
    {
        var task = Task.Run(() => Task.WhenAll(
            this.CheckOutgoingMessages(),
            this.ProcessIncomingMessages(),
            this.MonitorPipeForMessages()
        ));
        this.Completion = task;
    }

    public async Task CloseSession()
    {
        if (this.loggedOut)
        {
            return;
        }
        this.loggedOut = true;
        await this.NotifyRpcRequestOkResponse(
            Models.CloseSession.Instance,
            CancellationToken.None
        );
        this.outgoingMessages.Writer.Complete();
    }

    public Task Completion { get; private set; } = Task.CompletedTask;

    private static readonly ClientHello defaultClientHello = new(ImmutableHashSet.Create(
        Capability.Base,
        Capability.Base__1_1
    ));


    public void Dispose()
    {
        this.CloseSession().GetAwaiter().GetResult();
        this.session.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        await this.CloseSession();
        this.session.Dispose();
    }

    public bool SupportsXPathFilter() => CheckServerCapabilities(Capability.XPath);

    private bool CheckServerCapabilities(Capability capabilities)
        => this.ServerHello.Capabilities.Contains(capabilities);

    private void ThrowExceptionIfNotSupported(IReadOnlyList<Capability>? capabilities)
    {
        if(!this.CheckServerCapabilities(capabilities))
        {
            throw new InvalidOperationException($"Server must support all of the capabilities: {string.Join(", ", capabilities)}");
        }
    }
    private bool CheckServerCapabilities([NotNullWhen(false)] IEnumerable<Capability>? capabilities)
    {
        if (capabilities is null)
        {
            return true;
        }
        var serverCapabilities = this.ServerHello.Capabilities;
        foreach (var capability in capabilities)
        {
            if (!serverCapabilities.Contains(capability))
                return false;
        }
        return true;
    }
}