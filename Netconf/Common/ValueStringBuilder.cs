// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable All

namespace Netconf;

internal ref struct ValueStringBuilder
{
    private char[]? _arrayToReturnToPool;
    private Span<char> _chars;
    private int _pos;

    public ValueStringBuilder(Span<char> initialBuffer)
    {
        this._arrayToReturnToPool = null;
        this._chars = initialBuffer;
        this._pos = 0;
    }

    public ValueStringBuilder(int initialCapacity)
    {
        this._arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity);
        this._chars = this._arrayToReturnToPool;
        this._pos = 0;
    }

    public int Length
    {
        get => this._pos;
        set
        {
            Debug.Assert(value >= 0);
            Debug.Assert(value <= this._chars.Length);
            this._pos = value;
        }
    }

    public int Capacity => this._chars.Length;

    public void EnsureCapacity(int capacity)
    {
        // This is not expected to be called this with negative capacity
        Debug.Assert(capacity >= 0);

        // If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
        if ((uint)capacity > (uint)this._chars.Length)
            this.Grow(capacity - this._pos);
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// Does not ensure there is a null char after <see cref="Length"/>
    /// This overload is pattern matched in the C# 7.3+ compiler so you can omit
    /// the explicit method call, and write eg "fixed (char* c = builder)"
    /// </summary>
    public ref char GetPinnableReference()
    {
        return ref MemoryMarshal.GetReference(this._chars);
    }

    /// <summary>
    /// Get a pinnable reference to the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ref char GetPinnableReference(bool terminate)
    {
        if (terminate)
        {
            this.EnsureCapacity(this.Length + 1);
            this._chars[this.Length] = '\0';
        }
        return ref MemoryMarshal.GetReference(this._chars);
    }

    public ref char this[int index]
    {
        get
        {
            Debug.Assert(index < this._pos);
            return ref this._chars[index];
        }
    }

    public override string ToString()
    {
        string s = this._chars.Slice(0, this._pos).ToString();
        this.Dispose();
        return s;
    }

    /// <summary>Returns the underlying storage of the builder.</summary>
    public Span<char> RawChars => this._chars;

    /// <summary>
    /// Returns a span around the contents of the builder.
    /// </summary>
    /// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
    public ReadOnlySpan<char> AsSpan(bool terminate)
    {
        if (terminate)
        {
            this.EnsureCapacity(this.Length + 1);
            this._chars[this.Length] = '\0';
        }
        return this._chars.Slice(0, this._pos);
    }

    public ReadOnlySpan<char> AsSpan() => this._chars.Slice(0, this._pos);
    public ReadOnlySpan<char> AsSpan(int start) => this._chars.Slice(start, this._pos - start);
    public ReadOnlySpan<char> AsSpan(int start, int length) => this._chars.Slice(start, length);

    public bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (this._chars.Slice(0, this._pos).TryCopyTo(destination))
        {
            charsWritten = this._pos;
            this.Dispose();
            return true;
        }
        else
        {
            charsWritten = 0;
            this.Dispose();
            return false;
        }
    }

    public void Insert(int index, char value, int count)
    {
        if (this._pos > this._chars.Length - count)
        {
            this.Grow(count);
        }

        int remaining = this._pos - index;
        this._chars.Slice(index, remaining).CopyTo(this._chars.Slice(index + count));
        this._chars.Slice(index, count).Fill(value);
        this._pos += count;
    }

    public void Insert(int index, string? s)
    {
        if (s == null)
        {
            return;
        }

        int count = s.Length;

        if (this._pos > (this._chars.Length - count))
        {
            this.Grow(count);
        }

        int remaining = this._pos - index;
        this._chars.Slice(index, remaining).CopyTo(this._chars.Slice(index + count));
        s
#if !NET
                .AsSpan()
#endif
            .CopyTo(this._chars.Slice(index));
        this._pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        int pos = this._pos;
        Span<char> chars = this._chars;
        if ((uint)pos < (uint)chars.Length)
        {
            chars[pos] = c;
            this._pos = pos + 1;
        }
        else
        {
            this.GrowAndAppend(c);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? s)
    {
        if (s == null)
        {
            return;
        }

        int pos = this._pos;
        if (s.Length == 1 && (uint)pos < (uint)this._chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
        {
            this._chars[pos] = s[0];
            this._pos = pos + 1;
        }
        else
        {
            this.AppendSlow(s);
        }
    }

    private void AppendSlow(string s)
    {
        int pos = this._pos;
        if (pos > this._chars.Length - s.Length)
        {
            this.Grow(s.Length);
        }

        s
#if !NET
                .AsSpan()
#endif
            .CopyTo(this._chars.Slice(pos));
        this._pos += s.Length;
    }

    public void Append(char c, int count)
    {
        if (this._pos > this._chars.Length - count)
        {
            this.Grow(count);
        }

        Span<char> dst = this._chars.Slice(this._pos, count);
        for (int i = 0; i < dst.Length; i++)
        {
            dst[i] = c;
        }
        this._pos += count;
    }

    public void Append(scoped ReadOnlySpan<char> value)
    {
        int pos = this._pos;
        if (pos > this._chars.Length - value.Length)
        {
            this.Grow(value.Length);
        }

        value.CopyTo(this._chars.Slice(this._pos));
        this._pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
        int origPos = this._pos;
        if (origPos > this._chars.Length - length)
        {
            this.Grow(length);
        }

        this._pos = origPos + length;
        return this._chars.Slice(origPos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        this.Grow(1);
        this.Append(c);
    }

    /// <summary>
    /// Resize the internal buffer either by doubling current buffer size or
    /// by adding <paramref name="additionalCapacityBeyondPos"/> to
    /// <see cref="_pos"/> whichever is greater.
    /// </summary>
    /// <param name="additionalCapacityBeyondPos">
    /// Number of chars requested beyond current position.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int additionalCapacityBeyondPos)
    {
        Debug.Assert(additionalCapacityBeyondPos > 0);
        Debug.Assert(this._pos > this._chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

        const uint ArrayMaxLength = 0x7FFFFFC7; // same as Array.MaxLength

        // Increase to at least the required size (_pos + additionalCapacityBeyondPos), but try
        // to double the size if possible, bounding the doubling to not go beyond the max array length.
        int newCapacity = (int)Math.Max(
            (uint)(this._pos + additionalCapacityBeyondPos),
            Math.Min((uint)this._chars.Length * 2, ArrayMaxLength));

        // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative.
        // This could also go negative if the actual required length wraps around.
        char[] poolArray = ArrayPool<char>.Shared.Rent(newCapacity);

        this._chars.Slice(0, this._pos).CopyTo(poolArray);

        char[]? toReturn = this._arrayToReturnToPool;
        this._chars = this._arrayToReturnToPool = poolArray;
        if (toReturn != null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        char[]? toReturn = this._arrayToReturnToPool;
        this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
        if (toReturn != null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }
}