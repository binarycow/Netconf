using System.Text;
using Netconf.Restconf;

namespace Demo;

public static class RestconfDemo
{
    public static async Task Run()
    {
        using var httpClient = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = static (_,_,_,_) => true,
        });
        
        httpClient.DefaultRequestHeaders.Authorization = new(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Program.Username}:{Program.Password}"))
        );
        await using var client = await RestconfClient.ConnectAsync(
            httpClient: httpClient,
            host: Program.Host,
            port: Program.RestconfPort,
            disposeClient: false,
            cancellationToken: CancellationToken.None
        );

        await client.Completion;
    }
}