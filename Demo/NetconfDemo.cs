using System.Xml.Linq;
using Netconf;
using Netconf.Netconf;
using Netconf.Netconf.Models;

namespace Demo;

public static class NetconfDemo
{
    private const string CapabilitiesFilename = @"S:\DotNetProjects\Netconf\Demo\capabilities.txt";
    private const string TrialKey = "==Ak3xHAH25RI/kACf72fojgT/NqXFhWOEAjaLJZpKfA1s=="; // Expires 2025-02-17
    
    public static async Task Run()
    {
        RebexSshFactory.SetLicenseKey(TrialKey);
        await using var client = await NetconfClient.ConnectAsync(
            sshFactory: RebexSshFactory.Default,
            host: Program.Host,
            username: Program.Username,
            password: Program.Password,
            port: Program.NetconfPort,
            cancellationToken: CancellationToken.None
        );
        Console.WriteLine($"Session ID: {client.ServerHello.SessionId}");
        Console.WriteLine($"# of capabilities: {client.ServerHello.Capabilities.Count}");
        await File.WriteAllLinesAsync(CapabilitiesFilename, client.ServerHello.Capabilities.Select(static x => x.ToString()));

        Console.WriteLine("Executing 'get-config'");
        Console.WriteLine(await client.GetConfigAsync(
            Datastore.Running,
            CancellationToken.None
        ));
        Console.WriteLine("Killing a (hopefully) invalid session");
        try
        {
            await client.KillSession(Random.Shared.Next(0, int.MaxValue));
        }
        catch (RpcException e)
        {
            Program.WriteException(e);
        }

        Console.WriteLine("Getting openconfig system-top");
        Console.WriteLine(await client.GetAsync(XNamespace.Get("http://openconfig.net/yang/system") + "system-top"));

        await client.CloseSession();
        await client.Completion;
    }
}