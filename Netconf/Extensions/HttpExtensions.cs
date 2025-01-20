using System.Xml.Linq;

namespace Netconf;

internal static class HttpExtensions
{
    public static async Task<XElement> ReadAsXElementAsync(this HttpContent content, CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        return await XElement.LoadAsync(stream, LoadOptions.None, cancellationToken);
    }
}