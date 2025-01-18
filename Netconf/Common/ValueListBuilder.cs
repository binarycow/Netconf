using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Netconf;

internal ref struct ValueListBuilder<T>
{
    private Span<T> span;
    private T[]? arrayFromPool;
    private int pos;

    public ValueListBuilder(Span<T?> scratchBuffer)
    {
        this.span = scratchBuffer!;
    }

    public ValueListBuilder(int capacity)
    {
        this.Grow(capacity);
    }

    public int Length
    {
        get => this.pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= this.span.Length);
            this.pos = value;
        }
    }

    public ref T this[int index]
    {
        get
        {
            Debug.Assert(index < this.pos);
            return ref this.span[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(T item)
    {
        var localPos = this.pos;

        // Workaround for https://github.com/dotnet/runtime/issues/72004
        var localSpan = this.span;
        if ((uint)localPos < (uint)localSpan.Length)
        {
            localSpan[localPos] = item;
            this.pos = localPos + 1;
        }
        else
        {
            this.AddWithResize(item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<T> source)
    {
        var localPos = this.pos;
        var localSpan = this.span;
        if (source.Length == 1 && (uint)localPos < (uint)localSpan.Length)
        {
            localSpan[localPos] = source[0];
            this.pos = localPos + 1;
        }
        else
        {
            this.AppendMultiChar(source);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendMultiChar(scoped ReadOnlySpan<T> source)
    {
        if ((uint)(this.pos + source.Length) > (uint)this.span.Length)
        {
            this.Grow(this.span.Length - this.pos + source.Length);
        }

        source.CopyTo(this.span.Slice(this.pos));
        this.pos += source.Length;
    }

    public void Insert(int index, scoped ReadOnlySpan<T> source)
    {
        Debug.Assert(index == 0, "Implementation currently only supports index == 0");

        if ((uint)(this.pos + source.Length) > (uint)this.span.Length)
        {
            this.Grow(source.Length);
        }

        this.span.Slice(0, this.pos).CopyTo(this.span.Slice(source.Length));
        source.CopyTo(this.span);
        this.pos += source.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AppendSpan(int length)
    {
        Debug.Assert(length >= 0);

        var localPos = this.pos;
        var localSpan = this.span;
        if ((uint)localPos + (ulong)(uint)length > (uint)localSpan.Length)
        {
            return this.AppendSpanWithGrow(length);
        }

        this.pos = localPos + length;
        return localSpan.Slice(localPos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<T> AppendSpanWithGrow(int length)
    {
        var localPos = this.pos;
        this.Grow(this.span.Length - localPos + length);
        this.pos += length;
        return this.span.Slice(localPos, length);
    }

    // Hide uncommon path
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        Debug.Assert(this.pos == this.span.Length);
        var localPos = this.pos;
        this.Grow();
        this.span[localPos] = item;
        this.pos = localPos + 1;
    }

    public ReadOnlySpan<T> AsSpan()
    {
        return this.span.Slice(0, this.pos);
    }

    public List<T> ToList()
    {
        var localSpan = this.AsSpan();
        var list = new List<T>(localSpan.Length);
        foreach (var item in localSpan)
        {
            list.Add(item);
        }
        return list;
    }

    public bool TryCopyTo(Span<T> destination, out int itemsWritten)
    {
        if (this.span.Slice(0, this.pos).TryCopyTo(destination))
        {
            itemsWritten = this.pos;
            return true;
        }

        itemsWritten = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (this.arrayFromPool is not { } toReturn)
        {
            return;
        }
        this.arrayFromPool = null;
        ArrayPool<T>.Shared.Return(toReturn);
    }

    // Note that consuming implementations depend on the list only growing if it's absolutely
    // required.  If the list is already large enough to hold the additional items be added,
    // it must not grow. The list is used in a number of places where the reference is checked
    // and it's expected to match the initial reference provided to the constructor if that
    // span was sufficiently large.
    private void Grow(int additionalCapacityRequired = 1)
    {
        const int arrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Double the size of the span.  If it's currently empty, default to size 4,
        // although it'll be increased in Rent to the pool's minimum bucket size.
        var nextCapacity = Math.Max(this.span.Length != 0 ? this.span.Length * 2 : 4, this.span.Length + additionalCapacityRequired);

        // If the computed doubled capacity exceeds the possible length of an array, then we
        // want to downgrade to either the maximum array length if that's large enough to hold
        // an additional item, or the current length + 1 if it's larger than the max length, in
        // which case it'll result in an OOM when calling Rent below.  In the exceedingly rare
        // case where _span.Length is already int.MaxValue (in which case it couldn't be a managed
        // array), just use that same value again and let it OOM in Rent as well.
        if ((uint)nextCapacity > arrayMaxLength)
        {
            nextCapacity = Math.Max(Math.Max(this.span.Length + 1, arrayMaxLength), this.span.Length);
        }

        var array = ArrayPool<T>.Shared.Rent(nextCapacity);
        this.span.CopyTo(array);

        T[]? toReturn = this.arrayFromPool;
        this.span = this.arrayFromPool = array;
        if (toReturn != null)
        {
            ArrayPool<T>.Shared.Return(toReturn);
        }
    }
}