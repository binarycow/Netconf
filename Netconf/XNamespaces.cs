using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Netconf;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Namespaces
{
    public const string Netconf = "urn:ietf:params:xml:ns:netconf:base:1.0";
    public const string Notification = "urn:ietf:params:xml:ns:netmod:notification";
    public const string Notification__1_0 = "urn:ietf:params:xml:ns:netconf:notification:1.0";
}

[SuppressMessage("ReSharper", "ReplaceAutoPropertyWithComputedProperty")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class XNamespaces
{
    public static XNamespace Netconf { get; } = Namespaces.Netconf;
    public static XNamespace Notification { get; } = Namespaces.Notification;
    public static XNamespace Notification__1_0 { get; } = Namespaces.Notification__1_0;
}