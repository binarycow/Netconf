using System.Buffers;

namespace Netconf;

internal sealed class ArrayMemorySegment<T> : ReadOnlySequenceSegment<T>
{
    public ArrayMemorySegment(T[] array, int offset, int length)
    {
        this.Array = array;
        this.Memory = array.AsMemory().Slice(offset, length);
    }

    public T[] Array { get; }

    public ArrayMemorySegment<T> Append(T[] array)
        => Append(array, 0, array.Length);
    public ArrayMemorySegment<T> Append(T[] array, int offset, int length)
    {
        var segment = new ArrayMemorySegment<T>(array, offset, length)
        {
            RunningIndex = this.RunningIndex + this.Memory.Length
        };
        this.Next = segment;
        return segment;
    }
}