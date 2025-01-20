using System.Xml.Linq;

namespace Netconf.Netconf.Models;

internal sealed record XElementDataWrapper(XElement Response)
    : IXmlParsable<XElementDataWrapper>,
        IXElementRpcReplyParsable<XElementDataWrapper>
{

    public static XElementDataWrapper FromXElement(XElement element)
    {
        if (element.Elements().TryGetSingle(out var child))
        {
            return new(child);
        }
        throw new NotImplementedException();
    }


    public static XName RootElementName => XNamespaces.Netconf + "data";
}

internal sealed record DataWrapper<TResponse>(TResponse Response)
    : IXmlParsable<DataWrapper<TResponse> >,
        IXElementRpcReplyParsable<DataWrapper<TResponse>>
    where TResponse : IXmlParsable<TResponse>
{
    public static DataWrapper<TResponse> FromXElement(XElement element)
    {
        if (element.Elements().TryGetSingle(out var child))
        {
            return new(TResponse.FromXElement(child));
        }

        throw new NotImplementedException();
    }

    public static XName RootElementName => XNamespaces.Netconf + "data";
}