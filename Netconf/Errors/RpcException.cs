using System.Net;
using Netconf.Restconf.Models;

namespace Netconf;

public sealed class RpcException : Exception
{
    internal RpcException(string? message) : base(message)
    {
        
    }
    public ErrorType Type { get; private init; }
    public string? Tag { get; private init; }
    public ErrorSeverity Severity { get; private init; }
    public string? AppTag { get; private init; }
    public string? Path { get; private init; }
    public string? MessageLanguage { get; private init; }
    public HttpStatusCode? StatusCode { get; private init; }
    public object? Info { get; private init; }

    private static RpcException Create(IRpcError error) => new(error.Message)
    {
        Type = error.Type,
        Tag = error.Tag,
        Severity = error.Severity,
        AppTag = error.AppTag,
        Path = error.Path,
        MessageLanguage = error.MessageLanguage,
        Info = error.Info,
        StatusCode = (error as RestconfError)?.StatusCode,
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