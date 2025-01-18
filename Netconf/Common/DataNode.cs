using System.Xml.Linq;

namespace Netconf;

public abstract class DataNode : IEquatable<DataNode>, IXmlParsable<DataNode>
{
    private protected DataNode()
    {
        
    }
    public static DataNode FromXElement(XElement element)
    {
        throw new NotImplementedException();
    }
    public abstract override string ToString();
    public abstract bool Equals(DataNode? other);
    public abstract override int GetHashCode();
    
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj.GetType() != this.GetType())
        {
            return false;
        }
        return this.Equals((DataNode)obj);
    }
    public static bool operator ==(DataNode? left, DataNode? right) => Equals(left, right);
    public static bool operator !=(DataNode? left, DataNode? right) => !Equals(left, right);
}

public abstract class DataNode<TSelf> : DataNode
    where TSelf : DataNode<TSelf>
{
    private protected DataNode()
    {
    }
    private protected abstract bool Equals(TSelf other);
    public sealed override bool Equals(DataNode? other)
        => other is TSelf t && this.Equals(t);
}

public sealed class DataValue : DataNode<DataValue>
{
    public string Value { get; }
    public DataValue(string value) => this.Value = value;
    public override string ToString() => this.Value;
    public override int GetHashCode()
        => string.GetHashCode(this.Value, StringComparison.Ordinal);
    private protected override bool Equals(DataValue other)
        => string.Equals(this.Value, other.Value, StringComparison.Ordinal);
}