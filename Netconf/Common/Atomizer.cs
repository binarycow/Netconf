using System.Diagnostics.CodeAnalysis;

namespace Netconf;

internal sealed class Atomizer<TKey, TValue>
    where TKey : notnull
    where TValue : class
{
    private readonly Func<TKey, TValue> createValue;
    private readonly Func<TValue, TKey> getKeyForItem;

    public Atomizer(
        Func<TKey, TValue> createValue,
        Func<TValue, TKey> getKeyForItem,
        int initialCapacity,
        IEqualityComparer<TKey>? comparer = default
    )
    {
        this.createValue = createValue;
        this.getKeyForItem = getKeyForItem;
        this.initialCapacity = initialCapacity;
        this.comparer = comparer ?? EqualityComparer<TKey>.Default;
    }
    
    private XHashtable<TKey, WeakReference<TValue>>? hashtable;
    private readonly int initialCapacity;
    private readonly IEqualityComparer<TKey> comparer;

    internal TValue Get<TParams>(TKey key, Func<TKey, TParams, TValue> createValueWithParams, TParams parameters)
    {
        if (this.hashtable is null)
        {
            Interlocked.CompareExchange(ref this.hashtable, new(this.ExtractKey, this.initialCapacity, this.comparer), null);
        }
        TValue? value;
        do
        {
            if (!this.hashtable.TryGetValue(key, out var reference))
            {
                reference = this.hashtable.Add(new(createValueWithParams(key, parameters)));
            }
            value = reference.GetTargetOrNull();
        }
        while (value is null);
        return value;
    }

    internal TValue Get(TKey key) => this.Get(
        key,
        static (key, createValue) => createValue(key),
        this.createValue
    );

    private bool ExtractKey(WeakReference<TValue> value, [MaybeNullWhen(false)] out TKey key)
    {
        if (value.TryGetTarget(out var target))
        {
            key = this.getKeyForItem(target);
            return true;
        }
        key = default;
        return false;
    }
}