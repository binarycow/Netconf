using System.Buffers;
using System.Diagnostics;

namespace Netconf;

internal sealed class SequenceStreamWriter : Stream
{
    private readonly int bufferSize;
    private const int DefaultBufferSize = 512; // TODO: Use a bigger buffer size.  This is purely to check how well chunking works.

    private List<ArrayMemorySegment<byte>> segments = [];
    private byte[]? currentSegmentArray;
    private ArraySegment<byte> currentSegment = ArraySegment<byte>.Empty;
    private ArrayMemorySegment<byte>? firstSegment;
    private ArrayMemorySegment<byte>? lastSegment;
    
    public ReadOnlySequence<byte> Sequence
    {
        get
        {
            this.AppendPartialSegment();
            return (this.firstSegment, this.lastSegment) switch
            {
                (null, null) => default,
                (not null, not null) => new(this.firstSegment, 0, this.lastSegment, this.lastSegment.Memory.Length),
                _ => throw new UnreachableException(),
            };
        }
    }

    public SequenceStreamWriter(int bufferSize = DefaultBufferSize)
    {
        this.bufferSize = bufferSize;
    }

    private void AppendPartialSegment()
    {
        if (this.currentSegmentArray is null)
        {
            return;
        }
        var length = this.currentSegmentArray.Length - this.currentSegment.Count;
        if (length is 0)
        {
            return;
        }
        this.AppendSegmentCore(this.currentSegmentArray, 0, length);
        this.currentSegmentArray = null;
        this.currentSegment = ArraySegment<byte>.Empty;
    }

    private void AppendSegmentCore(byte[] array, int offset, int length)
    {
        this.lastSegment = this.lastSegment is null
            ? this.firstSegment = new(array, offset, length)
            : this.lastSegment.Append(array, offset, length);
        segments.Add(this.lastSegment);
    }
    private void AppendFullSegment()
    {
        if (this.currentSegmentArray is null)
        {
            return;
        }
        this.AppendSegmentCore(this.currentSegmentArray, 0, this.currentSegmentArray.Length);
        this.currentSegmentArray = null;
        this.currentSegment = ArraySegment<byte>.Empty;
    }

    public override void Flush()
    {
    }
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        while (buffer.IsEmpty is false)
        {
            var destination = this.GetDestination(buffer.Length);
            var length = Math.Min(destination.Length, buffer.Length);
            buffer[..length].CopyTo(destination);
            buffer = buffer[length..];
        }
    }

    private Span<byte> GetDestination(int desiredLength)
    {
        if (this.currentSegment.Count is 0)
        {
            this.AppendFullSegment();
            this.currentSegmentArray = ArrayPool<byte>.Shared.Rent(this.bufferSize);
            this.currentSegment = this.currentSegmentArray;
        }
        Debug.Assert(this.currentSegment.Count > 0);
        var length = Math.Min(desiredLength, this.currentSegment.Count);
        var result = this.currentSegment.AsSpan()[..length];
        this.currentSegment = this.currentSegment[length..];
        return result;
    }



    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        this.Write(buffer.AsSpan(offset, count));
        return Task.CompletedTask;
    }
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        this.Write(buffer.Span);
        return ValueTask.CompletedTask;
    }
    public override void WriteByte(byte value) => this.Write(new Span<byte>(ref value));
    public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));
    public override bool CanWrite => true;

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }
        for (var i = this.segments.Count - 1; i >= 0; --i)
        {
            var array = this.segments[i].Array;
            this.segments.RemoveAt(i);
            ArrayPool<byte>.Shared.Return(array);
        }
        if (this.currentSegmentArray is not null)
        {
            ArrayPool<byte>.Shared.Return(this.currentSegmentArray);
        }
        this.currentSegmentArray = null;
        this.currentSegment = default;
        this.firstSegment = default;
        this.lastSegment = default;
    }

    #region Not Supported

    
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override bool CanRead => throw new NotSupportedException();
    public override bool CanSeek => false;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    #endregion Not Supported
}