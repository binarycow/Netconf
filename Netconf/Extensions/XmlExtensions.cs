using System.Diagnostics;
using System.Xml.Linq;

namespace Netconf;

internal static class XmlExtensions
{
    public static bool NameMatchesWithMaybeNamespace(this XElement element, XName expectedName)
        => element.Name.LocalName == expectedName.LocalName 
           && (element.Name.Namespace == expectedName.Namespace || element.Name.Namespace == XNamespace.None);

    public static void Deconstruct(this XName name, out XNamespace @namespace, out string localName)
    {
        @namespace = name.Namespace;
        localName = name.LocalName;
    }
    
    
    public static T ParseEnum<T>(this XElement element, bool ignoreCase = true)
        where T : struct, Enum
        => Enum.Parse<T>(element.Value, ignoreCase: ignoreCase);

    public static DateTimeOffset ParseDateTimeOffset(this XElement element)
        => DateTimeOffset.Parse(element.Value);

    public static bool ParseBool(this XElement element)
    {
        var text = element.Value;
        if (text.Equals("true", StringComparison.Ordinal))
        {
            return true;
        }
        if (text.Equals("false", StringComparison.Ordinal))
        {
            return false;
        }
        throw new NotImplementedException();
    }
    
    public static IEnumerable<XElement> Clone(this IEnumerable<XElement> elements) 
        => elements.Select(Clone);
    public static IEnumerable<XObject> Clone(this IEnumerable<XObject> nodes) 
        => nodes.Select(Clone);
    public static XElement Clone(this XElement element) 
        => new(element);
    public static XAttribute Clone(this XAttribute element) 
        => new(element);
    public static XComment Clone(this XComment element) 
        => new(element);
    public static XDocumentType Clone(this XDocumentType element) 
        => new(element);
    public static XProcessingInstruction Clone(this XProcessingInstruction element) 
        => new(element);
    public static XText Clone(this XText element) 
        => new(element);
    public static XDocument Clone(this XDocument element) 
        => new(element);

    public static XObject Clone(this XObject node) => node switch
    {
        XAttribute n => n.Clone(),
        XComment n => n.Clone(),
        XDocumentType n => n.Clone(),
        XProcessingInstruction n => n.Clone(),
        XText n => n.Clone(),
        XDocument n => n.Clone(),
        XElement n => n.Clone(),
        _ => throw new UnreachableException(),
    };
    public static XElement? ElementMaybeNamespace(this XElement element, XName name)
        => element.Element(name) ?? element.Element(name.LocalName);

    public static IEnumerable<XElement> ElementsMaybeNamespace(this XElement element, XName name)
    {
        var hasOneChild = false;
        foreach (var child in element.Elements(name))
        {
            hasOneChild = true;
            yield return child;
        }
        if (hasOneChild)
        {
            yield break;
        }
        foreach (var child in element.Elements(name.LocalName))
        {
            yield return child;
        }
    }
}