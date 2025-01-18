using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Netconf;

internal interface IXmlParsable<out TSelf>
    where TSelf : IXmlParsable<TSelf>
{
    public static abstract TSelf FromXElement(XElement element);
}

internal static class XmlParsable
{
    public static TMessage ParseDocument<TMessage>(ReadOnlySequence<byte> sequence)
        where TMessage : IXmlParsable<TMessage>
    {
        try
        {
            using var stream = new SequenceStreamReader(sequence);
            var document = XDocument.Load(stream);
            if (document.Root is not { } root)
            {
                throw new NotImplementedException();
            }
            return TMessage.FromXElement(root);
        }
        catch (Exception e)
        {
            _ = e;
            throw;
        }
    }
}