using System.Xml.Linq;
using Netconf.Netconf.Models;

namespace Netconf.Netconf;

public sealed partial class NetconfClient
{

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
    ) => await this.NotifyRpcRequestOkResponse(
        new KillSession(sessionId),
        cancellationToken
    ).GetValueOrThrow();

    public Task<XElement> GetAsync(
        XName subtree,
        CancellationToken cancellationToken = default
    ) => this.InvokeRpcRequestXElement<Get>(
        new(new SubtreeGetFilter(new(subtree))),
        cancellationToken
    ).GetValueOrThrow();
    
    public Task<XElement> GetAsync(
        GetFilter? filter,
        CancellationToken cancellationToken = default
    ) => this.InvokeRpcRequestXElement<Get>(
        new(filter),
        cancellationToken
    ).GetValueOrThrow();
    
    public Task<XElement> GetConfigAsync(
        Datastore source,
        CancellationToken cancellationToken = default
    ) => this.GetConfigAsync<GetFilter>(
        source,
        null,
        cancellationToken
    );
    
    public Task<XElement> GetConfigAsync<TFilter>(
        Datastore source,
        TFilter? filter,
        CancellationToken cancellationToken = default
    ) where TFilter : GetFilter
    {
        this.ThrowExceptionIfNotSupported((filter as IRequiresCapabilities)?.RequiredCapabilities);
        return this.InvokeRpcRequestXElement<GetConfig>(
            new(source, filter),
            cancellationToken
        ).GetValueOrThrow();
    }
}