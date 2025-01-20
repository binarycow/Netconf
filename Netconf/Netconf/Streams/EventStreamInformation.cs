using System.Xml.Linq;

namespace Netconf.Netconf.Streams;

public sealed record EventStreamInformation(
    string Name,
    string? Description,
    bool ReplaySupport,
    DateTimeOffset? ReplayLogCreationTime
) : IXmlParsable<EventStreamInformation>
{

    private static XNamespace Namespace => XNamespaces.Notification;
    static EventStreamInformation IXmlParsable<EventStreamInformation>.FromXElement(XElement element)
        => FromXElement(element);
    internal static EventStreamInformation FromXElement(XElement element) => new(
        Name: element.ElementMaybeNamespace(Namespace + "name")?.Value ?? throw new NotImplementedException(),
        Description: element.ElementMaybeNamespace(Namespace + "description")?.Value,
        ReplaySupport: element.ElementMaybeNamespace(Namespace + "replaySupport")?.ParseBool() ?? false,
        ReplayLogCreationTime: element.ElementMaybeNamespace(Namespace + "replayLogCreationTime")?.ParseDateTimeOffset()
    );
}