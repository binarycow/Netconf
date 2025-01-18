namespace Netconf;

internal static class WeakReferenceExtensions
{
    public static T? GetTargetOrNull<T>(this WeakReference<T>? reference)
        where T : class
        => reference?.TryGetTarget(out var target) is true ? target : null;
}