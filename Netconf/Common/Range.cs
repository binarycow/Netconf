namespace Netconf;

internal readonly record struct Range<T>(
    T Start,
    T End
) where T : IComparable<T>;