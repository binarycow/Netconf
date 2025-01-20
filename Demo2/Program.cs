

using Netconf;
using Netconf.Netconf;


const string trialKey = "==Ak3xHAH25RI/kACf72fojgT/NqXFhWOEAjaLJZpKfA1s=="; // Expires 2025-02-17
const string host = "devnetsandboxiosxe.cisco.com";
const string username = "admin";
const string password = "C1sco12345";
const int netconfPort = 830;
RebexSshFactory.SetLicenseKey(trialKey);

Console.WriteLine("Starting second client");

await using var client = await NetconfClient.ConnectAsync(
    sshFactory: RebexSshFactory.Default,
    host: host,
    username: username,
    password: password,
    port: netconfPort,
    cancellationToken: CancellationToken.None
);
Console.WriteLine("Connected to second client");

await Task.Delay(TimeSpan.FromSeconds(5));

Console.WriteLine("Shutting down second client");
await client.CloseSession();