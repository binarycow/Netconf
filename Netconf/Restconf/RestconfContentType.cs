using System.Collections;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace Netconf.Restconf;

public enum RestconfContentType
{
    Unknown = 0,
    Json = 1,
    Xml = 2,
}


internal enum ReceivedRestconfContentType
{
    Unknown = 0,
    Json = 1,
    Xml = 2,
}

[Flags]
public enum RestconfContentTypes
{
    None = 0,
    Json = 1,
    Xml = 2,
    All = Json | Xml,
}

internal static class RestconfContentTypeExtensions
{
    private static readonly MediaTypeWithQualityHeaderValue YangDataXml = MediaTypeWithQualityHeaderValue.Parse("application/yang-data+xml");
    private static readonly MediaTypeWithQualityHeaderValue YangDataJson = MediaTypeWithQualityHeaderValue.Parse("application/yang-data+json");

    public static ReceivedRestconfContentType GetRestconfContentType(this MediaTypeHeaderValue? contentType)
    {
        return TryOne(contentType, ReceivedRestconfContentType.Json)
               ?? TryOne(contentType, ReceivedRestconfContentType.Xml)
               ?? ReceivedRestconfContentType.Unknown;
        static ReceivedRestconfContentType? TryOne(MediaTypeHeaderValue? actual, ReceivedRestconfContentType contentType)
        {
            if (contentType.GetMediaType().Equals(actual))
            {
                return contentType;
            }
            return null;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReceivedRestconfContentType GetRestconfContentType(this HttpContent content)
        => content.Headers.ContentType.GetRestconfContentType();

    public static RestconfContentTypes ToFlags(this RestconfContentType contentType) => contentType switch
    {
        RestconfContentType.Json => RestconfContentTypes.Json,
        RestconfContentType.Xml => RestconfContentTypes.Xml,
        _ => RestconfContentTypes.None,
    };
    
    public static MediaTypeWithQualityHeaderValue GetMediaType(this RestconfContentType contentType) => contentType switch
    {
        RestconfContentType.Json => YangDataJson,
        RestconfContentType.Xml => YangDataXml,
        RestconfContentType.Unknown => throw new ArgumentOutOfRangeException(nameof(contentType)),
        _ => throw new ArgumentOutOfRangeException(nameof(contentType)),
    };

    public static MediaTypeWithQualityHeaderValue GetMediaType(this ReceivedRestconfContentType contentType) => contentType switch
    {
        ReceivedRestconfContentType.Json => YangDataJson,
        ReceivedRestconfContentType.Xml => YangDataXml,
        ReceivedRestconfContentType.Unknown => throw new ArgumentOutOfRangeException(nameof(contentType)),
        _ => throw new ArgumentOutOfRangeException(nameof(contentType)),
    };

    public static IEnumerable<MediaTypeWithQualityHeaderValue> GetMediaTypes(this RestconfContentTypes contentType)
    {
        if (contentType.HasFlag(RestconfContentTypes.Json))
        {
            yield return YangDataJson;
        }
        if (contentType.HasFlag(RestconfContentTypes.Xml))
        {
            yield return YangDataXml;
        }
    }
}