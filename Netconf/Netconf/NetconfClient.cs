using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading.Channels;
using System.Xml.Linq;
using Netconf.Netconf.Models;
using Netconf.Netconf.Streams;
using Netconf.Netconf.Transport;

namespace Netconf.Netconf;

public sealed partial class NetconfClient : IClient
{
    public ClientHello ClientHello { get; }
    public ServerHello ServerHello { get; }
    private bool loggedOut;
        
    private ulong nextMessageId;
    private readonly ConcurrentDictionary<string, RpcListener> listeners = new();
    private string GetNextMessageId() => Interlocked.Increment(ref this.nextMessageId).ToString();
    private readonly ISshSession session;
    private PipeWriter PipeWriter => this.session.Pipe.Output;
    private PipeReader PipeReader => this.session.Pipe.Input;
    private readonly Channel<RpcMessage> outgoingMessages = Channel.CreateUnbounded<RpcMessage>();
    private readonly Channel<object> incomingMessages = Channel.CreateUnbounded<object>();
    private readonly Channel<NetconfNotification> notifications = Channel.CreateUnbounded<NetconfNotification>();
    private readonly IFramingProtocol framingProtocol;

    private NetconfClient(
        ISshSession sshSession,
        ClientHello clientHello,
        ServerHello serverHello,
        IFramingProtocol framingProtocol
    )
    {
        this.ClientHello = clientHello;
        this.ServerHello = serverHello;
        this.framingProtocol = framingProtocol;
        this.session = sshSession;
        this.Completion = Task.Run(() => Task.WhenAll(
            this.CheckOutgoingMessages(),
            this.ProcessIncomingMessages(),
            this.MonitorPipeForMessages(),
            this.WaitForNotifications()
        ));
    }
    
    private async Task WaitForNotifications()
        => await this.notifications.Reader.Completion;

    public async Task CloseSession(CancellationToken cancellationToken = default)
    {
        if (this.loggedOut)
        {
            return;
        }
        this.loggedOut = true;
        await this.InvokeRpcRequest<CloseSession, OkResponse>(
            Models.CloseSession.Instance,
            cancellationToken
        );
        this.Complete();
    }

    public Task Completion { get; }

    private static readonly ClientHello defaultClientHello = new(ImmutableHashSet.Create(
        Capability.Base,
        Capability.Base__1_1
    ));


    public void Dispose()
    {
        this.notifications.Writer.TryComplete();
        this.CloseSession().GetAwaiter().GetResult();
        this.session.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        this.notifications.Writer.TryComplete();
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


    public void Complete()
    {
        this.outgoingMessages.Writer.Complete();
        this.notifications.Writer.TryComplete();
    }

    private async Task MonitorPipeForMessages()
    {
        try
        {
            var messages = this.framingProtocol.ReadAllMessagesAsync(
                this.PipeReader,
                Parse,
                CancellationToken.None
            );
            await foreach (var message in messages)
            {
                await this.incomingMessages.Writer.WriteAsync(message);
            }
            this.incomingMessages.Writer.Complete();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        static object Parse(ReadOnlySequence<byte> sequence)
        {
            try
            {
                using var stream = new SequenceStreamReader(sequence);
                var document = XDocument.Load(stream);
                if (document.Root is not { } root)
                {
                    throw new NotImplementedException();
                }
                return root.Name is { NamespaceName: Namespaces.Notification__1_0 or "", LocalName: "notification" }
                    ? NetconfNotification.FromXElement(root)
                    : RpcMessage.FromXElement(root);
            }
            catch (Exception e)
            {
                _ = e;
                throw;
            }
        }
    }
    private async Task CheckOutgoingMessages()
    {
        try
        {
            await foreach (var item in this.outgoingMessages.Reader.ReadAllAsync())
            {
                await this.framingProtocol.WriteAsync(this.PipeWriter, item, CancellationToken.None);
            }
            await this.PipeWriter.CompleteAsync();
        }
        catch (Exception e)
        {
            _ = e;
            throw;
        }

    }
    private async Task ProcessIncomingMessages()
    {
        try
        {
            await foreach (var message in this.incomingMessages.Reader.ReadAllAsync())
            {
                switch (message)
                {
                    case NetconfNotification notification:
                        await this.notifications.Writer.WriteAsync(notification);
                        break;
                    case RpcRequest request:
                        this.ProcessIncomingMessage(request);
                        break;
                    case XElementRpcReply reply:
                        this.ProcessIncomingMessage(reply);
                        break; 
                    default:
                        throw new UnreachableException();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void ProcessIncomingMessage(XElementRpcReply message)
    {
        if (message.MessageId is null)
        {
            throw new NotImplementedException();
        }

        if (!this.listeners.TryRemove(message.MessageId, out var listener))
        {
            throw new NotImplementedException();
        }

        try
        {
            if (listener.CancellationToken.IsCancellationRequested)
            {
                listener.SetCancelled(listener.CancellationToken);
            }
            else
            {
                listener.ProcessReply(message);
            }
        }
        catch (OperationCanceledException e)
        {
            listener.SetCancelled(e);
        }
        catch (Exception e)
        {
            listener.SetException(e);
        }
        finally
        {
            listener.Dispose();
        }
    }
    private void ProcessIncomingMessage(RpcRequest message)
    {
        throw new NotImplementedException();
    }
    
    internal Task<RpcResult<TResponse>> InvokeRpcRequest<TRequest, TResponse>(
        TRequest requestPayload,
        CancellationToken cancellationToken
    ) 
        where TRequest : IXmlFormattable
        where TResponse : IXElementRpcReplyParsable<TResponse>, IXmlParsable<TResponse>
    {
        CancellationTokenSource? cts = null;
        try
        {

            cts = cancellationToken.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
                : null;
            var messageId = this.GetNextMessageId();
            var request = new RpcRequest<TRequest>(messageId, requestPayload);
            var listener = new RpcListener<TResponse>(TResponse.FromXElementRpcReply, cts) { Request = request };
            var success = this.listeners.TryAdd(messageId, listener);
            Debug.Assert(success);
            success = this.outgoingMessages.Writer.TryWrite(request);
            Debug.Assert(success);
            return listener.Task;
        }
        catch
        {
            cts?.Dispose();
            throw;
        }
    }
}