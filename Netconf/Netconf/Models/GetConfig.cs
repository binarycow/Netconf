using System.Xml.Linq;

namespace Netconf.Netconf.Models;

internal sealed record GetConfig(Datastore Source, GetFilter? Filter) : IXmlFormattable
{
    public XElement ToXElement() => new(
        XNamespaces.Netconf + "get-config",
        new XElement(
            XNamespaces.Netconf + "source", 
            this.Source.ToXElement()
        ), 
        this.Filter?.ToXElement()
    );
}

internal sealed record Get(GetFilter? Filter) : IXmlFormattable
{
    public XElement ToXElement() => new(
        XNamespaces.Netconf + "get",
        this.Filter?.ToXElement()
    );
}