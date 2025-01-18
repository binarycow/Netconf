using System.Xml.Linq;

namespace Netconf.Netconf.Models;

public sealed class Datastore : IXmlFormattable
{
    public XName Name { get; }
    private Datastore(XName name) => this.Name = name;
    public XElement ToXElement() => new (this.Name);
    public static Datastore Running { get; } = new(XNamespaces.Netconf + "running");
    
    // Commented until I figure out what needs to be in place for this to work.
    // public static Datastore Startup { get; } = new(XNamespaces.Netconf + "startup");
    // public static Datastore Candidate { get; } = new(XNamespaces.Netconf + "candidate");
    // public static Datastore Intended { get; } = new(XNamespaces.Netconf + "intended");
    // public static Datastore Operational { get; } = new(XNamespaces.Netconf + "operational");
}