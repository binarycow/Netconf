using System.Xml.Linq;

namespace Netconf.Restconf.Models;

public sealed record Xrd(
    string? RestconfLink 
) : IXmlParsable<Xrd>
{
    private const string XrdNamespace = "http://docs.oasis-open.org/ns/xri/xrd-1.0";
    private static readonly XNamespace XrdXNamespace = XrdNamespace;
    public static Xrd FromXElement(XElement element)
    {
        if (element.Name is not { LocalName: "XRD", NamespaceName: XrdNamespace or "" })
        {
            throw new NotImplementedException();
        }
        return new Xrd(element.ElementsMaybeNamespace(XrdXNamespace + "Link")
            .FirstOrDefault(static x => x.Attribute("rel")?.Value is "restconf")
            ?.Attribute("href")
            ?.Value
        );
    }
}