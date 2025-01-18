using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;

namespace Netconf.Netconf.Models;

public abstract record GetFilter : IXmlFormattable
{
    private protected GetFilter()
    {
    }
    protected abstract string FilterType { get; }
    protected abstract IEnumerable<XObject> GetPayload();

    public XElement ToXElement() => new(
        XNamespaces.Netconf + "filter",
        new XAttribute(XNamespaces.Netconf + "type", this.FilterType),
        this.GetPayload()
    );
}

[PublicAPI]
public sealed record SubtreeGetFilter(XElement Filter) : GetFilter
{
    protected override string FilterType => "subtree";
    protected override IEnumerable<XObject> GetPayload()
        => [this.Filter.Clone()];
}

[PublicAPI]
public sealed record XPathGetFilter(
    string Select,
    IReadOnlyList<(XNamespace Namespace, string Prefix)>? Namespaces
) : GetFilter, IRequiresCapabilities<XPathGetFilter>
{
    public IReadOnlyList<(XNamespace Namespace, string Prefix)> Namespaces { get; init; }
        = Namespaces ?? [];
    public static IReadOnlyList<Capability> RequiredCapabilities => [Capability.XPath];
    protected override string FilterType => "xpath";
    protected override IEnumerable<XObject> GetPayload()
    {
        foreach (var (@namespace, prefix) in this.Namespaces)
        {
            yield return new XAttribute(XNamespace.Xmlns + prefix, @namespace);
        }
        yield return new XElement("select", this.Select);
    }
}