using System.Xml.Linq;

namespace Netconf;

internal interface IXmlFormattable
{
    public XElement ToXElement();
}