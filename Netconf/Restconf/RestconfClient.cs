using System.Net.Http.Headers;
using System.Text;
using Netconf.Restconf.Models;

namespace Netconf.Restconf;

public sealed class RestconfClient : IClient
{
    private bool disposed;
    private readonly HttpClient httpClient;
    private readonly string baseUrl;
    private readonly bool disposeClient;

    private RestconfClient(HttpClient httpClient, string baseUrl, bool disposeClient)
    {
        this.httpClient = httpClient;
        this.baseUrl = baseUrl;
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
        return new(httpClient, baseUrl, disposeClient: disposeClient);
        static async Task<string?> GetBaseUrl(
            HttpClient httpClient,
            string baseUrl,
            CancellationToken cancellationToken
        )
        {
            var response = await httpClient.GetAsync($"{baseUrl}/.well-known/host-meta", cancellationToken);
            var xml = await response.Content.ReadAsXElementAsync(cancellationToken);
            return Xrd.FromXElement(xml).RestconfLink is not { } restconfLink
                ? null
                : $"{baseUrl}{restconfLink}";
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
}