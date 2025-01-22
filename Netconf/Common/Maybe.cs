namespace Netconf;

internal static class Maybe
{
    public static Maybe<T> CreateIfNotNull<T>(T? value)
        where T : struct
        => value is { } v ? new Maybe<T>(v) : default;
}

internal readonly struct Maybe<T> : IEquatable<Maybe<T>>
    where T : notnull
{
    public Maybe(T? value)
    {
        this.HasValue = true;
        this.Value = value;
    }
    public bool HasValue { get; }
    public T? Value { get; }
    public bool Equals(Maybe<T> other) => this.HasValue == other.HasValue && EqualityComparer<T?>.Default.Equals(this.Value, other.Value);
    public override bool Equals(object? obj) => obj is Maybe<T> other && this.Equals(other);
    public override int GetHashCode() => HashCode.Combine(this.HasValue, this.Value);
    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !left.Equals(right);
}