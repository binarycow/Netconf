using System.Collections.Immutable;

namespace Netconf;

public partial class Capability
{
    private static Capability Parse(
        string key
    ) => Parse(
        key, 
        new Uri(key, UriKind.Absolute).Query,
        null,
        null
    );
    private static Capability Parse(
        string key,
        string? shorthand,
        IReadOnlyList<string>? references
    ) => Parse(
        key, 
        new Uri(key, UriKind.Absolute).Query,
        shorthand,
        references
    );

    private static Capability Parse(
        string text,
        ReadOnlySpan<char> query,
        string? shorthand,
        IReadOnlyList<string>? references
    )
    {
      
        if (query is ['?', ..])
        {
            query = query[1..];
        }
        if (query.IsEmpty)
        {
            return new OtherCapability(text);
        }
        List<(string Key, string Value)>? allValues = null;
        List<string>? features = null;
        List<string>? deviations = null;
        string? module = null;
        DateOnly? revision = null;
        DefaultsCapabilityBasicMode basicMode = default;
        DefaultsCapabilityAlsoSupported alsoSupported = default;
        string? contentId = null;
        foreach(var segmentRange in query.Split('&'))
        {
            var segment = query[segmentRange];
            var equalsIndex = segment.IndexOf('=');
            if (equalsIndex < 0)
            {
                continue;
            }
            var key = segment[..equalsIndex].Trim().ToString();
            var valueSpan = segment[(equalsIndex + 1)..].Trim();
            (allValues ??= []).Add((key, valueSpan.ToString()));
            switch (key)
            {
                case "basic-mode" when basicMode is DefaultsCapabilityBasicMode.Unknown && TryParseBasicMode(valueSpan, out var m):
                    basicMode = m;
                    break;
                case "also-supported":
                    alsoSupported |= ParseAlsoSupported(valueSpan);
                    break;
                case "module" when module is null:
                    module = valueSpan.ToString();
                    break;
                case "revision" when revision is null && DateOnly.TryParseExact(valueSpan, Constants.RevisionDateFormatString, out var date):
                    revision = date;
                    break;
                case "deviations":
                    ParseStringCsv(ref deviations, valueSpan);
                    break;
                case "features":
                    ParseStringCsv(ref features, valueSpan);
                    break;
                case "content-id" or "module-set-id" when contentId is null:
                    contentId = valueSpan.ToString();
                    break;
            }
        }

        return (Module: module, BasicMode: basicMode, ContentId: contentId) switch
        {
            (Module: not null, BasicMode: _, ContentId: _)
                => new ModuleCapability(text, module, revision, deviations, features)
            {
                Options = CreateOthers(allValues, static key => key is not ("module" or "revision" or "deviations" or "features")),
            },
            (Module: _, BasicMode: not DefaultsCapabilityBasicMode.Unknown, ContentId: _)
                => new DefaultsCapability(text, basicMode, alsoSupported)
            {
                Options = CreateOthers(allValues, static key => key is not ("basic-mode" or "also-supported")),
            },
            (Module: _, BasicMode: _, ContentId: not null)
                => new YangLibraryCapability(text, contentId)
            {
                Options = CreateOthers(allValues, static key => key is not ("module-set-id" or "content-id")),
            },
            _ => new OtherCapability(text)
            {
                Options = CreateOthers(allValues),
            },
        };
        static IReadOnlyDictionary<string, IReadOnlyList<string>> CreateOthers(
            List<(string Key, string Value)>? list,
            Func<string, bool>? except = null
        ) => list?
            .Where(static (a, except) => except?.Invoke(a.Key) is null or false, except)
            .GroupBy(
                static x => x.Key,
                static x => x.Value
            )
            .ToImmutableDictionary(
                static x => x.Key,
                static x => (IReadOnlyList<string>)x.ToImmutableList()
            ) ?? ImmutableDictionary<string, IReadOnlyList<string>>.Empty;

        static void ParseStringCsv(ref List<string>? list, ReadOnlySpan<char> value)
        {
            list ??= [];
            foreach (var featureRange in value.Split(','))
            {
                list.Add(value[featureRange].ToString());
            }
        }
        
        static bool TryParseBasicMode(ReadOnlySpan<char> value, out DefaultsCapabilityBasicMode result)
        {
            result = value switch
            {
                "report-all" => DefaultsCapabilityBasicMode.ReportAll,
                "trim" => DefaultsCapabilityBasicMode.Trim,
                "explicit" => DefaultsCapabilityBasicMode.Explicit,
                _ => DefaultsCapabilityBasicMode.Unknown,
            };
            return result is not DefaultsCapabilityBasicMode.Unknown;
        }
        static DefaultsCapabilityAlsoSupported ParseAlsoSupported(ReadOnlySpan<char> value)
        {
            var result = DefaultsCapabilityAlsoSupported.None;
            foreach (var range in value.Split(','))
            {
                var part = value[range];
                result |= part switch
                {
                    "report-all" => DefaultsCapabilityAlsoSupported.ReportAll,
                    "report-all-tagged" => DefaultsCapabilityAlsoSupported.ReportAllTagged,
                    "trim" => DefaultsCapabilityAlsoSupported.Trim,
                    "explicit" => DefaultsCapabilityAlsoSupported.Explicit,
                    _ => DefaultsCapabilityAlsoSupported.None,
                };
            }
            return result;
        }
    }

}