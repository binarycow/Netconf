using System.Xml.Linq;

namespace Netconf.Netconf.Streams;

public abstract record NetconfNotification : IXmlParsable<NetconfNotification>
{
    public DateTimeOffset EventTime { get; }

    private protected NetconfNotification(DateTimeOffset eventTime)
    {
        this.EventTime = eventTime;
    }
    static NetconfNotification IXmlParsable<NetconfNotification>.FromXElement(XElement element)
        => FromXElement(element);
    internal static NetconfNotification FromXElement(XElement element) => new XElementNetconfNotification(
        element.ElementMaybeNamespace("eventTime")?.ParseDateTimeOffset() ?? default,
        new(element)
    );
}

public sealed record XElementNetconfNotification(
    DateTimeOffset EventTime,
    XElement Event
) : NetconfNotification(
    EventTime    
);