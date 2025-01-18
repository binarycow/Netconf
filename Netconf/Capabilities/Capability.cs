using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Netconf;

public partial class Capability : IEquatable<Capability>
{
    private const int InitialCapacity = 32;
    private static Atomizer<string, Capability>? atomizer;
    private static Atomizer<string, Capability> Atomizer => atomizer ??= new(
        createValue: Parse,
        getKeyForItem: static capability => capability.Value,
        initialCapacity: InitialCapacity,
        comparer: StringComparer.Ordinal
    );

    private protected Capability(string value)
    {
        this.Value = value;
    }
    [PublicAPI]
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Options { get; private protected init; }
        = ImmutableDictionary<string, IReadOnlyList<string>>.Empty;
    public override string ToString() => this.Value;
    [PublicAPI]
    public string Value { get; }
    [PublicAPI]
    public string? Shorthand { get; private protected init; }
    [PublicAPI]
    public IReadOnlyList<string>? References { get; private protected init; }

    public static Capability Get(string uri) => Atomizer.Get(uri);

    internal static Capability Get(
        string uri,
        string shorthand,
        IReadOnlyList<string>? references
    ) => Atomizer.Get<(string Shorthand, IReadOnlyList<string>? References)>(
        uri,
        static (key, parameters) => Parse(key, parameters.Shorthand, parameters.References),
        (shorthand, references)
    );
    
    #region Equality

    public bool Equals(Capability? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return this.Value == other.Value;
    }
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Capability other && this.Equals(other);
    public override int GetHashCode() => this.Value.GetHashCode();
    public static bool operator ==(Capability? left, Capability? right) => Equals(left, right);
    public static bool operator !=(Capability? left, Capability? right) => !Equals(left, right);

    #endregion Equality

}