using System.Text;
using Netconf;
using Netconf.Restconf;
using Netconf.Restconf.Models;

namespace Demo;

public static class RestconfDemo
{
    private const string ModuleOutputPath = @"S:\DotNetProjects\Netconf\Demo\modules";
    public static async Task Run(CancellationToken cancellationToken)
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
            cancellationToken: cancellationToken
        );

        Console.WriteLine("Getting modules state");
        YangLibrary yangLibrary;
        Console.WriteLine(yangLibrary = await client.GetYangLibrary(cancellationToken));

        Console.WriteLine("Downloading modules");
        await DownloadYangModules(httpClient, yangLibrary, cancellationToken);

        await client.Completion;
    }

    private static async Task DownloadYangModules(
        HttpClient client,
        YangLibrary yangLibrary,
        CancellationToken cancellationToken
    )
    {
        
        foreach (var (_, moduleSet) in yangLibrary.ModuleSets)
        {
            var directory = Path.Combine(ModuleOutputPath, moduleSet.Name);
            Directory.CreateDirectory(directory);
            foreach (var (_, module) in moduleSet.ImplementedModules)
            {
                var path = Path.Combine(directory, $"{module.RevisionInfo}.yang");
                if (File.Exists(path))
                {
                    continue;
                }
                await DownloadFile(client, module, path, cancellationToken);
            }
        }
        static async Task DownloadFile(
            HttpClient client,
            YangLibraryImplementedModule module,
            string path,
            CancellationToken cancellationToken
        )
        {
            foreach (var location in module.Locations)
            {
                Console.WriteLine($"Attempting to download {module.RevisionInfo} from {location}");
                if (await DownloadFileFromLocation(
                    client,
                    location,
                    path,
                    cancellationToken
                ))
                {
                    return;
                }
            }
            Console.WriteLine($"No location for {module.RevisionInfo}");
        }

        static async Task<bool> DownloadFileFromLocation(
            HttpClient client,
            Uri location,
            string path,
            CancellationToken cancellationToken
        )
        {
            location = new UriBuilder(location)
            {
                Host = Program.Host,
                Port = Program.RestconfPort,
            }.Uri;
            using var response = await client.GetAsync(location, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed: {response.ReasonPhrase}");
                return false;
            }
            await using var output = new FileStream(path, FileMode.Create);
            await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
            await input.CopyToAsync(output, cancellationToken);
            Console.WriteLine($"Success");
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            return true;
        }
    }

}