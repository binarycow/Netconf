using System.Xml.Linq;
using Netconf.Netconf.Models;

namespace Netconf.Netconf;

public sealed partial class NetconfClient
{

    public Task KillSession(uint sessionId)
        => KillSession(sessionId.ToString()).GetValueOrThrow();
    public Task KillSession(int sessionId)
        => KillSession(sessionId.ToString()).GetValueOrThrow();
    public async Task<RpcResult> KillSession(string sessionId) => await this.NotifyRpcRequestOkResponse(
        new KillSession(sessionId),
        CancellationToken.None
    );

    public Task<RpcResult<XElement>> GetConfigAsync(
        Datastore source,
        CancellationToken cancellationToken
    ) => this.GetConfigAsync<GetFilter>(
        source,
        null,
        cancellationToken
    );
    
    public Task<RpcResult<XElement>> GetConfigAsync<TFilter>(
        Datastore source,
        TFilter? filter,
        CancellationToken cancellationToken
    ) where TFilter : GetFilter
    {
        this.ThrowExceptionIfNotSupported((filter as IRequiresCapabilities)?.RequiredCapabilities);
        return this.InvokeRpcRequest<GetConfig, XElement>(
            new(source, filter),
            static response =>
            {
                if (response.IsError(out var errors))
                {
                    return errors;
                }

                if (!response.Elements.TryGetSingle(out var element))
                {
                    throw new NotImplementedException();
                }

                if (element.Name is not { LocalName: "data", NamespaceName: Namespaces.Netconf or "" })
                {
                    throw new NotImplementedException();
                }

                return element;
            },
            cancellationToken
        );
    }
}