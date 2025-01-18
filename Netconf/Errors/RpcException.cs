namespace Netconf;

public sealed class RpcException : Exception
{
    internal RpcException(string? message) : base(message)
    {
        
    }
    public ErrorType Type { get; internal set; }
    public string? Tag { get; internal set; }
    public ErrorSeverity Severity { get; internal set; }
    public string? AppTag { get; internal set; }
    public string? Path { get; internal set; }
    public string? MessageLanguage { get; internal set; }
    public object? Info { get; internal set; }

    internal static RpcException Create(RpcError error) => new(error.Message)
    {
        Type = error.Type,
        Tag = error.Tag,
        Severity = error.Severity,
        AppTag = error.AppTag,
        Path = error.Path,
        MessageLanguage = error.MessageLanguage,
        Info = error.Info,
    };

    internal static Exception Create(RpcErrorList errorList) => errorList.Count switch
    {
        1 => Create(errorList[0]),
        _ => new AggregateException(errorList.Select(Create)),
    };

    internal static void ThrowIfFailure(RpcResult result)
    {
        if (!result.IsSuccess)
        {
            throw Create(result.Errors);
        }
    }

    internal static T GetResultOrThrow<T>(RpcResult<T> result)
        where T : notnull
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }
        throw Create(result.Errors);
    }
}

internal static class RpcExceptionExtensions
{
    public static async Task<T> GetValueOrThrow<T>(this Task<RpcResult<T>> task)
        where T : notnull
        => RpcException.GetResultOrThrow(await task);

    public static async Task GetValueOrThrow(this Task<RpcResult> task)
        => RpcException.ThrowIfFailure(await task);
}