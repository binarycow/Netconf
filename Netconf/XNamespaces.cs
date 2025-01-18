using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Netconf;

public static class Namespaces
{
    public const string Netconf = "urn:ietf:params:xml:ns:netconf:base:1.0";
}

[SuppressMessage("ReSharper", "ReplaceAutoPropertyWithComputedProperty")]
public static class XNamespaces
{
    public static XNamespace Netconf { get; } = Namespaces.Netconf;
}