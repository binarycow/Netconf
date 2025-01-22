using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Netconf.Netconf.Models;

internal abstract record RpcReply : RpcMessage
{
    protected abstract IEnumerable<XObject> PayloadToXElement();
    public sealed override XElement ToXElement() => new(
        XNamespaces.Netconf + "rpc-reply",
        this.MessageId is null ? null : new XAttribute("message-id", this.MessageId),
        this.PayloadToXElement()
    );

    public abstract bool IsError(out RpcErrorList error);
}

internal sealed record XElementRpcReply(
    string? MessageId,
    IEnumerable<XElement> Elements,
    IEnumerable<XAttribute> Attributes
) : RpcReply
{
    public override string? MessageId { get; } = MessageId;
    protected override IEnumerable<XObject> PayloadToXElement()
        => this.Elements.Concat<XObject>(this.Attributes).Clone();

    public override bool IsError(out RpcErrorList error)
    {
        using var builder = new ValueListBuilder<IRpcError>();
        foreach (var element in this.Elements)
        {
            if (element.Name is not { LocalName: "rpc-error", NamespaceName: Namespaces.Netconf or "" })
            {
                continue;
            }
            builder.Append(NetconfError.FromXElement(element));
        }
        switch (builder.Length)
        {
            case 0:
                error = default;
                return false;
            case 1:
                error = new(builder[0]);
                return true;
            default:
                error = builder.ToList().AsReadOnly();
                return true;
        }
    }
}