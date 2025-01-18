using System.Buffers;

namespace Netconf;

internal static class MemorySegment
{
    public static (MemorySegment<T> First, MemorySegment<T> Last) Create<T>(ReadOnlySequence<T> sequence)
    {
        var first = new MemorySegment<T>(sequence.First);
        sequence = sequence.Slice(sequence.First.Length);
        var last = first.Append(sequence);
        return (first, last);
    }
}

internal sealed class MemorySegment<T> : ReadOnlySequenceSegment<T>
{
    public MemorySegment(ReadOnlyMemory<T> memory)
    {
        this.Memory = memory;
    }



    public MemorySegment<T> Append(ReadOnlySequence<T> sequence)
    {
        var segment = this;
        foreach (var memory in sequence)
        {
            segment = segment.Append(memory);
        }
        return segment;
    }

    public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
    {
        var segment = new MemorySegment<T>(memory)
        {
            RunningIndex = this.RunningIndex + this.Memory.Length
        };
        this.Next = segment;
        return segment;
    }
}