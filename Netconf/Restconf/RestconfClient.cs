using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using Netconf.Netconf.Models;
using Netconf.Restconf.Models;

namespace Netconf.Restconf;

public sealed class RestconfClient : IClient
{
    private bool disposed;
    private readonly HttpClient httpClient;
    private readonly string baseUrl;
    private readonly bool disposeClient;

    private RestconfClient(HttpClient httpClient, string baseUrl, DateOnly? yangLibraryVersion, bool disposeClient)
    {
        this.httpClient = httpClient;
        this.baseUrl = baseUrl;
        this.YangLibraryVersion = yangLibraryVersion;
        this.disposeClient = disposeClient;
    }

    /// <remarks>
    /// This method assumes you've already set up default request headers for authentication.
    /// </remarks>
    public static async Task<RestconfClient> ConnectAsync(
        HttpClient httpClient,
        string host,
        int port,
        bool disposeClient,
        CancellationToken cancellationToken
    )
    {
        if (await GetBaseUrl(httpClient, $"https://{host}:{port}", cancellationToken) is not { } baseUrl)
        {
            throw new NotImplementedException();
        }
        
        var maybeYangLibraryVersion = await GetAsync(
            httpClient,
            baseUrl,
            "ietf-restconf:restconf",
            RestconfContentType.Json,
            RestconfContentTypes.All,
            static jsonNode => (jsonNode as JsonObject)
                ?.GetPropertyValue<JsonValue>("yang-library-version")
                .Deserialize<Maybe<DateOnly>>(Json.JsonOptions) ?? default,
            cancellationToken
        ).GetValueOrThrow();
        var yangLibraryVersion = maybeYangLibraryVersion.HasValue ? maybeYangLibraryVersion.Value : (DateOnly?)null;
        
        return new(httpClient, baseUrl, yangLibraryVersion, disposeClient);
        static async Task<string?> GetBaseUrl(
            HttpClient httpClient,
            string baseUrl,
            CancellationToken cancellationToken
        )
        {
            var response = await httpClient.GetAsync(Url.Combine(baseUrl, ".well-known/host-meta"), cancellationToken);
            var xml = await response.Content.ReadAsXElementAsync(cancellationToken);
            return Xrd.FromXElement(xml).RestconfLink is not { } restconfLink
                ? null
                : Url.Combine(baseUrl, restconfLink);
        }
        
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }
        this.disposed = true;
        if (this.disposeClient)
        {
            this.httpClient.Dispose();
        }
    }

    public ValueTask DisposeAsync()
    {
        this.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task Completion => Task.CompletedTask;

    internal DateOnly? YangLibraryVersion { get; }

    public Task<TResponse> GetAsync<TResponse>(
        QualifiedName node,
        CancellationToken cancellationToken
    ) where TResponse : notnull
    {
        var responsePropertyName = $"{node.ModuleName}:{node.LocalName}";
        return this.GetAsync<TResponse>(
            Url.Combine("data", responsePropertyName),
            responsePropertyName,
            RestconfContentType.Json,
            RestconfContentTypes.All,
            cancellationToken
        ).GetValueOrThrow();
    }

    private Task<RpcResult<TResponse>> GetAsync<TResponse>(
        string relativeUrl,
        string responsePropertyName,
        RestconfContentType preferredContentType,
        RestconfContentTypes allowedContentTypes,
        CancellationToken cancellationToken
    ) where TResponse : notnull
        => GetAsync<TResponse>(
            this.httpClient,
            Url.Combine(this.baseUrl, relativeUrl),
            responsePropertyName,
            preferredContentType,
            allowedContentTypes,
            static node => node.Deserialize<TResponse>(Json.JsonOptions) ?? throw new NotImplementedException(),
            cancellationToken
        );
    
    private static async Task<RpcResult<TResponse>> GetAsync<TResponse>(
        HttpClient httpClient,
        string absoluteUrl,
        string responsePropertyName,
        RestconfContentType preferredContentType,
        RestconfContentTypes allowedContentTypes,
        Func<JsonNode, TResponse> parseJson,
        CancellationToken cancellationToken
    ) where TResponse : notnull
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
        requestMessage.Headers.Accept.Add(preferredContentType.GetMediaType());
        using var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
        switch (responseMessage.Content.GetRestconfContentType())
        {
            case ReceivedRestconfContentType.Json when allowedContentTypes.HasFlag(RestconfContentTypes.Json):
                return await Parse(responseMessage, responsePropertyName, parseJson, cancellationToken);
            case ReceivedRestconfContentType.Xml when allowedContentTypes.HasFlag(RestconfContentTypes.Xml):
                throw new NotImplementedException();
            case ReceivedRestconfContentType.Json:
                throw new NotImplementedException();
            case ReceivedRestconfContentType.Xml:
                throw new NotImplementedException();
            default:
                throw new NotImplementedException();
        }

        static async Task<RpcResult<TResponse>> Parse(
            HttpResponseMessage response,
            string responsePropertyName,
            Func<JsonNode, TResponse> parseJson,
            CancellationToken cancellationToken
        )
        {
            switch (response)
            {
                case { IsSuccessStatusCode: true }:
                    return await ParseSuccess(response.Content, responsePropertyName, parseJson, cancellationToken);
                case { StatusCode: HttpStatusCode.Forbidden }:
                    throw new NotImplementedException();
                case { StatusCode: >= (HttpStatusCode)400 and < (HttpStatusCode)500 }:
                    return await ParseErrors(response.Content, response.StatusCode, cancellationToken);
                default:
                    throw new NotImplementedException();
            }
        }
        
        static async Task<RpcResult<TResponse>> ParseSuccess(
            HttpContent content,
            string responsePropertyName,
            Func<JsonNode, TResponse> parseJson,
            CancellationToken cancellationToken
        ) => content.GetRestconfContentType() switch
        {
            ReceivedRestconfContentType.Json => await ParseJsonSuccess(content, responsePropertyName, parseJson, cancellationToken),
            ReceivedRestconfContentType.Xml => throw new NotImplementedException(),
            ReceivedRestconfContentType.Unknown => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };

        static async Task<RpcResult<TResponse>> ParseJsonSuccess(
            HttpContent content,
            string responsePropertyName,
            Func<JsonNode, TResponse> parseJson,
            CancellationToken cancellationToken
        )
        {
            var jsonObject = await content.ReadFromJsonAsync<JsonObject>(cancellationToken);
            if (jsonObject?.TryGetPropertyValue(responsePropertyName, out var value) is not true || value is null)
            {
                throw new NotImplementedException();
            }
            var result = parseJson(value) ?? throw new NotImplementedException();
            return result;
        }
        
        static async Task<RpcErrorList> ParseErrors(HttpContent content, HttpStatusCode statusCode, CancellationToken cancellationToken)
        {
            switch (content.GetRestconfContentType())
            {
                case ReceivedRestconfContentType.Json:
                
                    return new(
                        (await content.ReadFromJsonAsync<RestconfErrorListWrapper>(Json.JsonOptions, cancellationToken))
                            ?.Errors.Errors.AddStatusCode(statusCode)
                            ?? []
                    );
                case ReceivedRestconfContentType.Xml:
                    throw new NotImplementedException();
                case ReceivedRestconfContentType.Unknown:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }
    }

}