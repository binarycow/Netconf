using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Xml.Linq;
using Netconf.Netconf.Models;
using Netconf.Netconf.Streams;
using Netconf.Netconf.Transport;

namespace Netconf.Netconf;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed partial class NetconfClient
{

    #region Kill Session

    
    public Task KillSession(
        uint sessionId,
        CancellationToken cancellationToken = default
    ) => KillSession(sessionId.ToString(), cancellationToken);
    public Task KillSession(
        int sessionId,
        CancellationToken cancellationToken = default
    ) => this.KillSession(sessionId.ToString(), cancellationToken);
    public async Task KillSession(
        string sessionId,
        CancellationToken cancellationToken = default
    ) => await this.InvokeRpcRequest<KillSession, OkResponse>(
        new KillSession(sessionId),
        cancellationToken
    ).GetValueOrThrow();

    #endregion Kill Session

    #region Get

    public Task<XElement> GetAsync(
        XName subtree,
        CancellationToken cancellationToken = default
    ) => this.GetAsync(
        new SubtreeGetFilter(new(subtree)),
        cancellationToken
    );
    
    public async Task<XElement> GetAsync(
        GetFilter? filter,
        CancellationToken cancellationToken = default
    ) => (await this.InvokeRpcRequest<Get, XElementDataWrapper>(
        new(filter),
        cancellationToken
    ).GetValueOrThrow()).Response;

    
    internal async Task<TResponse> GetAsync<TResponse>(
        GetFilter? filter,
        CancellationToken cancellationToken = default
    ) where TResponse : IXmlParsable<TResponse>
        => (await this.InvokeRpcRequest<Get, DataWrapper<TResponse>>(
            new(filter),
            cancellationToken
        ).GetValueOrThrow()).Response;
    
    #endregion Get

    #region Get Config

    
    public Task<XElement> GetConfigAsync(
        Datastore source,
        CancellationToken cancellationToken = default
    ) => this.GetConfigAsync<GetFilter>(
        source,
        null,
        cancellationToken
    );
    
    public async Task<XElement> GetConfigAsync<TFilter>(
        Datastore source,
        TFilter? filter,
        CancellationToken cancellationToken = default
    ) where TFilter : GetFilter
    {
        this.ThrowExceptionIfNotSupported((filter as IRequiresCapabilities)?.RequiredCapabilities);
        return (await this.InvokeRpcRequest<GetConfig, XElementDataWrapper>(
            new(source, filter),
            cancellationToken
        ).GetValueOrThrow()).Response;
    }

    #endregion Get Config

    #region Event Streams

    public async Task<IReadOnlyList<EventStreamInformation>> ListEventStreams(
        CancellationToken cancellationToken = default
    ) => (await this.GetAsync<EventStreamListResponse>(
        new SubtreeGetFilter(
            new(
                XNamespaces.Notification + "netconf",
                new XElement(XNamespaces.Notification + "streams")
            )
        ),
        cancellationToken
    )).Streams;
        
    private async IAsyncEnumerable<NetconfNotification> SubscribeToStream(
        CreateSubscriptionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var result = await this.InvokeRpcRequest<CreateSubscriptionRequest, OkResponse>(request, cancellationToken);
        if (!result.IsSuccess)
        {
            throw RpcException.Create(result.Errors);
        }

        await foreach (var notification in this.notifications.Reader.ReadAllAsync(cancellationToken))
        {
            yield return notification;
        }
    }

    public IAsyncEnumerable<NetconfNotification> SubscribeToStream(
        string streamName,
        DateTimeOffset startTime,
        DateTimeOffset stopTime,
        CancellationToken cancellationToken = default
    ) => this.SubscribeToStream(new CreateSubscriptionRequest(streamName, new(startTime, stopTime)), cancellationToken);

    public IAsyncEnumerable<NetconfNotification> SubscribeToStream(
        string streamName,
        CancellationToken cancellationToken = default
    ) => this.SubscribeToStream(new CreateSubscriptionRequest(streamName, null), cancellationToken);

    public IAsyncEnumerable<NetconfNotification> SubscribeToStream(
        EventStreamInformation streamInformation,
        DateTimeOffset startTime,
        DateTimeOffset stopTime,
        CancellationToken cancellationToken = default
    ) => this.SubscribeToStream(new CreateSubscriptionRequest(streamInformation.Name, new(startTime, stopTime)), cancellationToken);
    public IAsyncEnumerable<NetconfNotification> SubscribeToStream(
        EventStreamInformation streamInformation,
        CancellationToken cancellationToken = default
    ) => this.SubscribeToStream(new CreateSubscriptionRequest(streamInformation.Name, null), cancellationToken);

    #endregion Event Streams

}