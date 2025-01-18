using System.Xml.Linq;

namespace Netconf.Netconf.Models;

internal abstract record RpcRequest(
    string MessageId
) : RpcMessage
{
    public override string MessageId { get; } = MessageId;
    protected abstract IEnumerable<XObject> PayloadToXElement();
    public sealed override XElement ToXElement() => new(
        XNamespaces.Netconf + "rpc",
        new XAttribute("message-id", this.MessageId),
        this.PayloadToXElement()
    );
}

internal sealed record XElementRpcRequest(
    string MessageId,
    IReadOnlyCollection<XElement> Elements,
    IReadOnlyCollection<XAttribute>? Attributes = null
) : RpcRequest(MessageId)
{
    public XElementRpcRequest(
        string messageId,
        XElement element,
        IReadOnlyCollection<XAttribute>? attributes = null
    ) : this(
        messageId,
        [element],
        attributes
    ) { }
    public IReadOnlyCollection<XAttribute> Attributes { get; init; } = Attributes ?? [];

    protected override IEnumerable<XObject> PayloadToXElement()
        => this.Elements.Concat<XObject>(this.Attributes).Clone();
}
internal sealed record RpcRequest<T>(
    string MessageId,
    T Payload
) : RpcRequest(MessageId)
    where T : IXmlFormattable
{
    protected override IEnumerable<XElement> PayloadToXElement()
        => [this.Payload.ToXElement()];
}
