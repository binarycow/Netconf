using Netconf.Netconf.Models;

namespace Netconf;

internal abstract class RpcListener : IDisposable
{
    private readonly CancellationTokenSource? cancellationTokenSource;
    public CancellationToken CancellationToken => this.cancellationTokenSource?.Token ?? default;

    protected RpcListener(CancellationTokenSource? cancellationTokenSource)
    {
        this.cancellationTokenSource = cancellationTokenSource;
    }
    public abstract Task Task { get; }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.cancellationTokenSource?.Dispose();
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract void SetException(Exception exception);

    public abstract void SetCancelled(CancellationToken cancellationToken);
    public void SetCancelled(OperationCanceledException exception) 
        => this.SetCancelled(exception.CancellationToken);
    public abstract void ProcessReply(XElementRpcReply message);
}
internal sealed class VoidRpcListener : RpcListener
{
    private readonly Func<XElementRpcReply, RpcResult> parseResponse;
    public VoidRpcListener(
        Func<XElementRpcReply, RpcResult> parseResponse,
        CancellationTokenSource? cancellationTokenSource
    ) : base(cancellationTokenSource)
    {
        this.parseResponse = parseResponse;
    }
    private readonly TaskCompletionSource<RpcResult> tcs = new();
    public override Task<RpcResult> Task => this.tcs.Task;
    public override void SetException(Exception exception)
        => this.tcs.SetException(exception);
    public override void SetCancelled(CancellationToken cancellationToken)
        => this.tcs.SetCanceled(cancellationToken);
    public override void ProcessReply(XElementRpcReply message)
        => this.tcs.SetResult(this.parseResponse(message));
}

internal sealed class RpcListener<TResponse> : RpcListener
    where TResponse : notnull
{
    private readonly Func<XElementRpcReply, RpcResult<TResponse>> parseResponse;

    public RpcListener(
        Func<XElementRpcReply, RpcResult<TResponse>> parseResponse,
        CancellationTokenSource? cancellationTokenSource
    ) : base(cancellationTokenSource)
    {
        this.parseResponse = parseResponse;
    }
    private readonly TaskCompletionSource<RpcResult<TResponse>> tcs = new();
    public override Task<RpcResult<TResponse>> Task => this.tcs.Task;
    
    public override void SetException(Exception exception)
        => this.tcs.SetException(exception);
    public override void SetCancelled(CancellationToken cancellationToken)
        => this.tcs.SetCanceled(cancellationToken);
    public override void ProcessReply(XElementRpcReply message)
        => this.tcs.SetResult(this.parseResponse(message));
}