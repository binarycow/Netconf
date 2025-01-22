namespace Netconf;

internal static class SpanExtensions
{
    public static bool TryCopyTo<T>(this ReadOnlySpan<T> source, Span<T> destination, ref int itemsWritten)
    {
        if (!source.TryCopyTo(destination))
        {
            return false;
        }
        itemsWritten += source.Length;
        return true;
    }
    public static bool TryCopyTo<T>(this ReadOnlySpan<T> source, ref Span<T> destination)
    {
        if (!source.TryCopyTo(destination))
        {
            return false;
        }
        destination = destination[source.Length..];
        return true;
    }
    public static void CopyTo<T>(this ReadOnlySpan<T> source, ref Span<T> destination)
    {
        source.CopyTo(destination);
        destination = destination[source.Length..];
    }
    public static void CopyTo<T>(this T source, ref Span<T> destination)
    {
        destination[0] = source;
        destination = destination[1..];
    }
}