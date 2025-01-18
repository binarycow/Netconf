namespace Netconf.Restconf;

public sealed class RestconfClient : IClient
{
    public static Task<RestconfClient> ConnectAsync(
        HttpClient httpClient,
        string host,
        string username,
        string password,
        int port,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public Task Completion => Task.CompletedTask;
}