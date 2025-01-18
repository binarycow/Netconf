using Netconf.Restconf;

namespace Demo;

public static class RestconfDemo
{
    public static async Task Run()
    {
        using var httpClient = new HttpClient();
        await using var client = await RestconfClient.ConnectAsync(
            httpClient: httpClient,
            host: Program.Host,
            username: Program.Username,
            password: Program.Password,
            port: Program.RestconfPort,
            cancellationToken: CancellationToken.None
        );

        await client.Completion;
    }
}