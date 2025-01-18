using System.Diagnostics;
using System.Xml.Linq;
using Netconf.Netconf.Models;

namespace Netconf.Netconf;

public sealed partial class NetconfClient
{
    internal Task<RpcResult<TResponse>> InvokeRpcRequest<TRequest, TResponse>(
        TRequest requestPayload,
        Func<XElementRpcReply, RpcResult<TResponse>> parseResponse,
        CancellationToken cancellationToken
    ) 
        where TRequest : IXmlFormattable
        where TResponse : notnull
    {
        var cts = cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;
        var messageId = this.GetNextMessageId();
        var listener = new RpcListener<TResponse>(parseResponse, cts);
        var success = this.listeners.TryAdd(messageId, listener);
        Debug.Assert(success);
        success = this.outgoingMessages.Writer.TryWrite(new RpcRequest<TRequest>(messageId, requestPayload));
        Debug.Assert(success);
        return listener.Task;
    }
    
    internal Task<RpcResult<TResponse>> InvokeRpcRequest<TResponse>(
        XElement requestPayload,
        Func<XElementRpcReply, RpcResult<TResponse>> parseResponse,
        CancellationToken cancellationToken
    ) 
        where TResponse : notnull
    {
        var cts = cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;
        var messageId = this.GetNextMessageId();
        var listener = new RpcListener<TResponse>(parseResponse, cts);
        var success = this.listeners.TryAdd(messageId, listener);
        Debug.Assert(success);
        success = this.outgoingMessages.Writer.TryWrite(new XElementRpcRequest(messageId, requestPayload));
        Debug.Assert(success);
        return listener.Task;
    }

    internal Task<RpcResult> NotifyRpcRequestOkResponse<TRequest>(
        TRequest requestPayload,
        CancellationToken cancellationToken
    )
        where TRequest : IXmlFormattable
    {
        return NotifyRpcRequest(
            requestPayload,
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
                if (element.Name is not { LocalName: "ok", NamespaceName: Namespaces.Netconf or "" })
                {
                    throw new NotImplementedException();
                }
                if (element.Attributes().Any() || element.Elements().Any())
                {
                    throw new NotImplementedException();
                }
                return RpcResult.Success();
            },
            cancellationToken
        );
    }
    internal Task<RpcResult> NotifyRpcRequest<TRequest>(
        TRequest requestPayload,
        Func<XElementRpcReply, RpcResult> parseResponse,
        CancellationToken cancellationToken
    )
        where TRequest : IXmlFormattable
    {
        var cts = cancellationToken.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;
        var messageId = this.GetNextMessageId();
        var listener = new VoidRpcListener(parseResponse, cts);
        var success = this.listeners.TryAdd(messageId, listener);
        Debug.Assert(success);
        success = this.outgoingMessages.Writer.TryWrite(new RpcRequest<TRequest>(messageId, requestPayload));
        Debug.Assert(success);
        return listener.Task;
    }
}