using System.Security.Cryptography;
using System.Xml.Linq;

namespace Netconf.Netconf.Models;

public sealed class CloseSession : IXmlFormattable
{
    private CloseSession() { }
    public static CloseSession Instance { get; } = new();
    public XElement ToXElement() => new (
        XNamespaces.Netconf + "close-session"    
    );
}

public sealed record KillSession(string SessionId) : IXmlFormattable
{
    public XElement ToXElement() => new (
        XNamespaces.Netconf + "kill-session",
        new XElement(XNamespaces.Netconf + "session-id", this.SessionId)
    );
}