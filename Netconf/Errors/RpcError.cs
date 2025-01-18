using System.Xml.Linq;

namespace Netconf;

public sealed record RpcError(
    ErrorType Type,
    string? Tag,
    ErrorSeverity Severity,
    string? AppTag,
    string? Path,
    string? Message,
    string? MessageLanguage,
    object? Info
) : IXmlParsable<RpcError>
{
    private static XNamespace NetconfNs => XNamespaces.Netconf;

    static RpcError IXmlParsable<RpcError>.FromXElement(XElement element)
        => FromXElement(element);

    internal static RpcError FromXElement(XElement element)
    {
        if (
            element.Name.LocalName is not "rpc-error"
            || element.Name.NamespaceName is not (Namespaces.Netconf or "")
        )
        {
            throw new NotImplementedException();
        }

        var errorMessageNode = element.ElementMaybeNamespace(NetconfNs + "error-message");
        return new(
            Type: element.ElementMaybeNamespace(NetconfNs + "error-type")?.ParseEnum<ErrorType>() ?? ErrorType.Unknown,
            Tag: element.ElementMaybeNamespace(NetconfNs + "error-tag")?.Value,
            Severity:  element.ElementMaybeNamespace(NetconfNs + "error-severity")?.ParseEnum<ErrorSeverity>() ?? ErrorSeverity.Unknown,
            AppTag: element.ElementMaybeNamespace(NetconfNs + "error-app-tag")?.Value,
            Path: element.ElementMaybeNamespace(NetconfNs + "error-path")?.Value,
            Message: errorMessageNode?.Value,
            MessageLanguage: errorMessageNode?.Attribute(XNamespace.Xml + "lang")?.Value,
            Info: element.ElementMaybeNamespace(NetconfNs + "error-info")?.Clone()
        );
    }
}