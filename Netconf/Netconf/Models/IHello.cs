using System.Collections.Immutable;
using System.Xml.Linq;

namespace Netconf.Netconf.Models;

public interface IHello
{
    public IReadOnlySet<Capability> Capabilities { get; }
}

public sealed record ServerHello(
    string SessionId,
    IReadOnlySet<Capability> Capabilities
) : IHello, IXmlParsable<ServerHello>
{
    public static XName ElementName => XNamespaces.Netconf + "hello";

    public static ServerHello FromXElement(XElement element)
    {
        if (element.Name != ElementName)
        {
            throw new NotImplementedException();
        }
        return new ServerHello(
            SessionId: element.ElementMaybeNamespace(XNamespaces.Netconf + "session-id")?.Value ?? string.Empty,
            Capabilities: element
                .ElementMaybeNamespace(XNamespaces.Netconf + "capabilities")
                ?.ElementsMaybeNamespace(XNamespaces.Netconf + "capability")
                .Select(static x => Capability.Get(x.Value.Trim()))
                .WhereNotNull()
                .ToImmutableHashSet()
                    ?? []
        );
    }
}

public sealed record ClientHello(
    IReadOnlySet<Capability> Capabilities
) : IHello, IXmlFormattable
{
    public XElement ToXElement() => new(
        ServerHello.ElementName,
        new XElement(
            XNamespaces.Netconf + "capabilities",
            this.Capabilities.Select(
                static x => new XElement(XNamespaces.Netconf + "capability", x)    
            )
        )
    );
}