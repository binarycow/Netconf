using System.Xml.Linq;

namespace Netconf.Netconf.Streams;

internal sealed record CreateSubscriptionRequest(
    string? StreamName,
    Range<DateTimeOffset>? ReplayRange
) : IXmlFormattable
{
    private static XNamespace Namespace => XNamespaces.Notification__1_0;

    public XElement ToXElement() => new(
        Namespace + "create-subscription",
        new XElement(Namespace + "stream", this.StreamName),
        this.ReplayRange?.Start is { } start ? new XElement(Namespace + "startTime", start.ToString("O")) : null,
        this.ReplayRange?.End is { } stop ? new XElement(Namespace + "stopTime", stop.ToString("O")) : null
    );
}