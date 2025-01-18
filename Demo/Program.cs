
using Netconf;
using Netconf.Netconf;
using Netconf.Netconf.Models;

const string capabilitiesFilename = @"S:\DotNetProjects\Netconf\Demo\capabilities.txt";
const string trialKey = "==Ak3xHAH25RI/kACf72fojgT/NqXFhWOEAjaLJZpKfA1s=="; // Expires 2025-02-17
const string host = "devnetsandboxiosxe.cisco.com";
const string username = "admin";
const string password = "C1sco12345";
RebexSshFactory.SetLicenseKey(trialKey);


await using var netconfClient = await NetconfClient.ConnectAsync(
    sshFactory: RebexSshFactory.Default,
    host: host,
    username: username,
    password: password,
    port: 830,
    cancellationToken: CancellationToken.None
);
Console.WriteLine($"Session ID: {netconfClient.ServerHello.SessionId}");
Console.WriteLine($"# of capabilities: {netconfClient.ServerHello.Capabilities.Count}");
File.WriteAllLines(capabilitiesFilename, netconfClient.ServerHello.Capabilities.Select(static x => x.ToString()));

Console.WriteLine("Executing 'get-config'");
Console.WriteLine(await netconfClient.GetConfigAsync(
    Datastore.Running,
    CancellationToken.None
));
Console.WriteLine("Killing a (hopefully) invalid session");
try
{
    await netconfClient.KillSession(Random.Shared.Next(0, int.MaxValue));
}
catch (RpcException e)
{
    WriteException(e);
}



await netconfClient.CloseSession();
await netconfClient.Completion;



void WriteException(RpcException exception)
{
    Console.WriteLine($"Error: {exception.Message}");
    Console.WriteLine($"Type: {exception.Type}");
    if (exception.Tag is { } tag)
        Console.WriteLine($"Tag: {tag}");
    Console.WriteLine($"Severity: {exception.Severity}");
    if (exception.AppTag is { } appTag)
        Console.WriteLine($"App Tag: {appTag}");
    if (exception.Path is { } path)
        Console.WriteLine($"Path: {path}");
    if (exception.MessageLanguage is { } language)
        Console.WriteLine($"Message Language: {language}");
    if (exception.Info is { } info)
        Console.WriteLine($"Info: {info}");
}





/*
using var httpClient = new HttpClient();
// using var restconfClient = await RestconfClient.ConnectAsync(
//     httpClient: httpClient,
//     host: host,
//     username: username,
//     password: password,
//     port: 443,
//     cancellationToken: CancellationToken.None
// );
*/