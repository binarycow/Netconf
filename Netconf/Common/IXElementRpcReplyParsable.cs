using System.Xml.Linq;
using Netconf.Netconf.Models;

namespace Netconf;

internal interface IXElementRpcReplyParsable<TSelf>
    where TSelf : IXElementRpcReplyParsable<TSelf>, IXmlParsable<TSelf>
{
    public static abstract XName RootElementName { get; }

    public static virtual RpcResult<TSelf> FromXElementRpcReply(XElementRpcReply reply)
    {
        if (reply.IsError(out var errors))
        {
            return errors;
        }
        if (!reply.Elements.TryGetSingle(out var element))
        {
            throw new NotImplementedException();
        }
        if (!element.NameMatchesWithMaybeNamespace(TSelf.RootElementName))
        {
            throw new NotImplementedException();
        }
        return RpcResult.Success(TSelf.FromXElement(element));
    }
}