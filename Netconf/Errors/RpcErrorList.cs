using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;

namespace Netconf;

public readonly partial struct RpcErrorList : IReadOnlyList<RpcError>, IList<RpcError>, IList, IEquatable<RpcErrorList>
{
    private readonly object? value;
    public RpcErrorList(RpcError error) => this.value = error;
    public RpcErrorList(IReadOnlyList<RpcError> errors) => this.value = errors;
    public static implicit operator RpcErrorList(RpcError error) => new(error);
    public static implicit operator RpcErrorList(List<RpcError> error) => new(error);
    public static implicit operator RpcErrorList(Collection<RpcError> error) => new(error);
    public static implicit operator RpcErrorList(RpcError[] error) => new(error);
    public static implicit operator RpcErrorList(ImmutableList<RpcError> error) => new(error);
    public static implicit operator RpcErrorList(ReadOnlyCollection<RpcError> error) => new(error);
    public Enumerator GetEnumerator() => new Enumerator(this);
    IEnumerator<RpcError> IEnumerable<RpcError>.GetEnumerator() => this.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public bool Equals(RpcErrorList other) => other.value switch
    {
        RpcError error => this.Equals(error),
        IReadOnlyList<RpcError> error => this.Equals(error),
        _ => this.Count is 0
    };

    public bool Equals(IEnumerable<RpcError>? other)
    {
        switch (this.value)
        {
            case RpcError singleError:
            {
                using var otherEnumerator = other?.GetEnumerator();
                return otherEnumerator is not null
                       && otherEnumerator.MoveNext() 
                       && singleError.Equals(otherEnumerator.Current)
                       && !otherEnumerator.MoveNext();
            }
            case IReadOnlyList<RpcError> list:
                return list.SequenceEqual(other ?? []);
            default:
                return other is null;
        }
    }

    public bool Equals(RpcError? other) => (this.value, other) switch
    {
        (null, null) => true,
        (IReadOnlyList<RpcError> and [], null) => true,
        (RpcError error, not null) => error.Equals(other),
        _ => false,
    };

    public override bool Equals(object? obj) => obj switch
    {
        RpcErrorList other => this.Equals(other),
        RpcError other => this.Equals(other),
        IEnumerable<RpcError> other => this.Equals(other),
        _ => false,
    };

    public override int GetHashCode()
    {
        var hc = new HashCode();
        foreach (var item in this)
        {
            hc.Add(item);
        }
        return hc.ToHashCode();
    }

    public static bool operator ==(RpcErrorList left, RpcErrorList right) => left.Equals(right);

    public static bool operator !=(RpcErrorList left, RpcErrorList right) => !left.Equals(right);

    public int Count => this.value switch
    {
        RpcError => 1,
        IReadOnlyList<RpcError> list => list.Count,
        _ => 0,
    };

    public RpcError this[int index] => (this.value, index) switch
    {
        (RpcError error, 0) => error,
        (IReadOnlyList<RpcError> list, >= 0) when index < list.Count => list[index],
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, default),
    };

    public bool Contains(RpcError item) => this.value switch
    {
        RpcError err => err.Equals(item),
        IReadOnlyList<RpcError> list => list.Contains(item),
        _ => false,
    };
    
    public void CopyTo(RpcError[] array, int arrayIndex)
    {
        // TODO: Make a sensible implementation
        throw new NotImplementedException();
    }
    void ICollection.CopyTo(Array array, int index)
    {
        // TODO: Make a sensible implementation
        throw new NotImplementedException();
    }
    
    bool IList.IsFixedSize => true;
    public bool IsReadOnly => true;

    public int IndexOf(RpcError item) => this.value switch
    {
        RpcError stored when stored.Equals(item) => 0,
        List<RpcError> list => list.IndexOf(item),
        IReadOnlyList<RpcError> list => IndexOfSlow(list, item),
        _ => -1,
    };

    private static int IndexOfSlow(IReadOnlyList<RpcError> list, RpcError item)
    {
        for (var i = 0; i < list.Count; ++i)
        {
            if (list[i].Equals(item))
            {
                return i;
            }
        }
        return -1;
    }

    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this.value is ICollection coll ? coll.SyncRoot : this;

    bool IList.Contains(object? item) => item is RpcError error && this.Contains(error);

    int IList.IndexOf(object? item) => item is RpcError error ? this.IndexOf(error) : -1;

    public override string ToString()
    {
        if (this.value is RpcError error)
        {
            return error.ToString();
        }
        if (this.value is not IReadOnlyList<RpcError> list)
        {
            return string.Empty;
        }
        Span<char> buffer = stackalloc char[256];
        using var sb = new ValueStringBuilder(buffer);
        sb.Append("Count: ");
        sb.Append(list.Count.ToString());
        for (var i = 0; i < list.Count; ++i)
        {
            sb.Append(Environment.NewLine);
            sb.Append('[');
            sb.Append(i.ToString());
            sb.Append("]: ");
            sb.Append(list[i].ToString());
        }
        return sb.ToString();
    }


    #region Not Supported
    void ICollection<RpcError>.Add(RpcError item) => throw new NotSupportedException();
    void ICollection<RpcError>.Clear() => throw new NotSupportedException();
    bool ICollection<RpcError>.Remove(RpcError item) => throw new NotSupportedException();
    void IList<RpcError>.Insert(int index, RpcError item) => throw new NotSupportedException();
    void IList<RpcError>.RemoveAt(int index) => throw new NotSupportedException();
    RpcError IList<RpcError>.this[int index]
    {
        get => this[index];
        set => throw new NotSupportedException();
    }
    object? IList.this[int index]
    {
        get => this[index];
        set => throw new NotSupportedException();
    }
    int IList.Add(object? item) => throw new NotSupportedException();
    void IList.Clear() => throw new NotSupportedException();
    void IList.Insert(int index, object? item) => throw new NotSupportedException();
    void IList.Remove(object? item) => throw new NotSupportedException();
    void IList.RemoveAt(int index) => throw new NotSupportedException();
    #endregion Not Supported
    
    

    public struct Enumerator : IEnumerator<RpcError>
    {
        private const int StateNotStarted = 0;
        private const int StateInProgress = 1;
        private const int StateFinished = 2;
        private const uint IndexMask = 0x_3FFF_FFFF;
        private const uint StateMask = 0x_C000_0000;
        private const int StateShift = 30;
        private readonly RpcErrorList list;
        private uint state;
        private int Index
        {
            get => (int)(this.state & IndexMask);
            set => this.state = (this.state & StateMask) | ((uint)value & IndexMask);
        }
        private int State
        {
            get => (int)(this.state >> StateShift);
            set => this.state = (this.state & IndexMask) | ((uint)value << StateShift);
        }

        public Enumerator(RpcErrorList list)
        {
            this.list = list;
            this.state = 0;
        }

        public bool MoveNext()
        {
            var currentState = this.State;
            var nextIndex = this.Index + 1;
            switch (currentState)
            {
                case StateFinished:
                    return false;
                case StateNotStarted or StateInProgress when nextIndex < this.list.Count:
                    this.State = StateInProgress;
                    this.Index = nextIndex;
                    return true;
                default:
                    this.State = StateFinished;
                    return false;
            }
        }

        public void Dispose()
        {
            
        }


        public void Reset()
        {
            this.state = 0;
        }

        public RpcError Current => this.State switch
        {
            StateInProgress => this.list[this.Index],
            StateFinished => throw new InvalidOperationException("Enumeration finished"),
            _ => throw new InvalidOperationException("Enumeration not started"),
        };

        object? IEnumerator.Current => this.Current;
    }
}