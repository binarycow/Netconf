using System.Diagnostics;

namespace Netconf;

internal static class Url
{
    private const char UrlSeparator = '/';
    
    public static string Combine(string primaryPath, string secondaryPath)
    {
        ArgumentNullException.ThrowIfNull(primaryPath);
        ArgumentNullException.ThrowIfNull(secondaryPath);

        var requiredLength = GetRequiredLength(primaryPath, secondaryPath);

        var newString = string.Create(requiredLength, (primaryPath, secondaryPath), static (destination, state) =>
        {
            var (primaryString, secondaryString) = state;
            var primary = primaryString.AsSpan().TrimEnd(UrlSeparator);
            primary.CopyTo(ref destination);
            UrlSeparator.CopyTo(ref destination);
            var secondary = secondaryString.AsSpan().Trim(UrlSeparator);
            secondary.CopyTo(ref destination);
            Debug.Assert(destination.IsEmpty);
        });
        return newString;
    }
    
    public static string Combine(string primaryPath, params string[] paths)
    {
        ArgumentNullException.ThrowIfNull(primaryPath);
        ArgumentNullException.ThrowIfNull(paths);

        var requiredLength = GetRequiredLength(primaryPath, paths);

        var newString = string.Create(requiredLength, (str: primaryPath, paths), static (destination, state) =>
        {
            var (primaryString, paths) = state;
            var primary = primaryString.AsSpan().TrimEnd(UrlSeparator);
            primary.CopyTo(ref destination);
            foreach (var secondaryString in paths)
            {
                primary.CopyTo(ref destination);
                var secondary = secondaryString.AsSpan().Trim(UrlSeparator);
                secondary.CopyTo(ref destination);
            }
            Debug.Assert(destination.IsEmpty);
        });
        return newString;
    }
    
    private static int GetRequiredLength(ReadOnlySpan<char> primaryPath, ReadOnlySpan<char> secondaryPath)
    {
        if (primaryPath.IsEmpty)
        {
            return secondaryPath.Length;
        }
        if (secondaryPath.IsEmpty)
        {
            return primaryPath.Length;
        }
        var length = primaryPath.TrimEnd(UrlSeparator).Length;
        length += 1;
        length += secondaryPath.Trim(UrlSeparator).Length;
        return length;
    }
    private static int GetRequiredLength(string primaryPath, string[] paths)
    {
        var length = primaryPath.AsSpan().TrimEnd(UrlSeparator).Length;
        foreach (var path in paths)
        {
            length += 1;
            length += path.AsSpan().Trim(UrlSeparator).Length;
        }
        return length;
    }
}