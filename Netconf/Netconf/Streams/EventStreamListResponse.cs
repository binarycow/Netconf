using System.Xml.Linq;

namespace Netconf.Netconf.Streams;

public sealed record EventStreamListResponse(
    IReadOnlyList<EventStreamInformation> Streams   
)
    : IXmlParsable<EventStreamListResponse>
{
    public static EventStreamListResponse FromXElement(XElement element)
    {
        return new(
            element.ElementMaybeNamespace(XNamespaces.Notification + "streams")
                ?.ElementsMaybeNamespace(XNamespaces.Notification + "stream")
                .Select(EventStreamInformation.FromXElement)
                .ToReadOnlyList()
                ?? []
        );
    }
}

