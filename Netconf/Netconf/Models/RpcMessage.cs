using System.Xml.Linq;

namespace Netconf.Netconf.Models;

internal abstract record RpcMessage
    : IXmlFormattable, 
        IXmlParsable<RpcMessage>
{
    internal XElement? OriginalElement { get; init; }
    public abstract string? MessageId { get; }
    public abstract XElement ToXElement();
    public static RpcMessage FromXElement(XElement element) => element.Name switch
    {
        { NamespaceName: Namespaces.Netconf or "", LocalName: "rpc" } => ParseRequest(element: element),
        { NamespaceName: Namespaces.Netconf or "", LocalName: "rpc-reply" } => ParseReply(element: element),
        _ => throw new NotImplementedException(),
    };

    private static XElementRpcReply ParseReply(XElement element)
    {
        return new(
            MessageId: element.Attribute(name: "message-id")?.Value,
            Elements: element.Elements().ToReadOnlyList(),
            Attributes: element.Attributes().Where(predicate: static x => x.Name != "message-id").ToReadOnlyList()
        )
        {
            OriginalElement = element,
        };
    }

    private static XElementRpcRequest ParseRequest(XElement element)
    {
        if (element.Attribute(name: "message-id")?.Value is not { } messageId)
            throw new NotImplementedException();
        return new(
            MessageId: messageId,
            Elements: element.Elements().ToReadOnlyList(),
            Attributes: element.Attributes().Where(predicate: static x => x.Name != "message-id").ToReadOnlyList()
        );
    }
}