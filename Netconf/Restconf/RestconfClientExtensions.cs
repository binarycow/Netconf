using Netconf.Restconf.Models;

namespace Netconf.Restconf;

public static class RestconfClientExtensions
{
    public static async Task<YangLibrary> GetYangLibrary(
        this RestconfClient client,
        CancellationToken cancellationToken
    ) => client.YangLibraryVersion >= new DateOnly(2019, 01, 04) 
        ? YangLibrary.Create(await client.GetAsync<YangLibraryDto>(new("ietf-yang-library", "yang-library"), cancellationToken))
        : YangLibrary.Create(await client.GetAsync<ModulesState>(new("ietf-yang-library", "modules-state"), cancellationToken));
}