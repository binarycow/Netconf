using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Netconf;

internal static class CollectionExtensions
{
    public delegate bool WherePredicate<TItem, TArgument>(TItem item, TArgument argument)
        where TArgument : allows ref struct;
    public static IEnumerable<TItem> Where<TItem, TArgument>(
        this IEnumerable<TItem> items,
        WherePredicate<TItem, TArgument> predicate,
        TArgument argument
    )
    {
        foreach (var item in items)
        {
            if (predicate(item, argument))
            {
                yield return item;
            }
        }
    }
    
    public static bool TryGetSingle<T>(this IEnumerable<T> items, [MaybeNullWhen(false)] out T single)
    {
        single = default;
        using var enumerator = items.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return false;
        }
        var found = enumerator.Current;
        if (enumerator.MoveNext())
        {
            return false;
        }
        single = found;
        return true;
    }
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> items)
    {
        foreach (var item in items)
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> items)
        where T : struct
    {
        foreach (var item in items)
        {
            if (item is { } nonNull)
            {
                yield return nonNull;
            }
        }
    }

    public static IReadOnlyList<T> ToReadOnlyList<T>(this IEnumerable<T> items) 
        => items.ToList().AsReadOnly();
}