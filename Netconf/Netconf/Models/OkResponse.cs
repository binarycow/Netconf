using System.Xml.Linq;

namespace Netconf.Netconf.Models;

public sealed record OkResponse
    : IXmlParsable<OkResponse>,
        IXElementRpcReplyParsable<OkResponse>
{
    public static OkResponse Instance { get; } = new();
    public static OkResponse FromXElement(XElement element) => Instance;
    public static XName RootElementName => XNamespaces.Netconf + "ok";
}