using System.Buffers;

namespace Netconf;

internal sealed class SequenceStreamReader : Stream
{
    private readonly ReadOnlySequence<byte> sequence;
    private ReadOnlySequence<byte> unreadSequence;

    public SequenceStreamReader(ReadOnlySequence<byte> sequence)
    {
        this.sequence = sequence;
        this.unreadSequence = sequence;
    }
    public override void Flush()
    {
    }

    public override int Read(Span<byte> buffer)
    {
        var totalRead = 0;
        while (!this.unreadSequence.IsEmpty && !buffer.IsEmpty)
        {
            var firstSpan = this.unreadSequence.FirstSpan;
            var length = Math.Min(firstSpan.Length, buffer.Length);
            firstSpan[..length].CopyTo(buffer[..length]);
            buffer = buffer[length..];
            totalRead += length;
            this.unreadSequence = this.unreadSequence.Slice(length);
        }
        return totalRead;
    }

    public override int ReadByte()
    {
        if (this.sequence.IsEmpty)
        {
            return -1;
        }
        var value = this.sequence.FirstSpan[0];
        this.unreadSequence = this.sequence.Slice(1);
        return value;
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Task.FromResult(this.Read(buffer.AsSpan(offset, count)));
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(this.Read(buffer.Span));
    public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
    public override bool CanRead => true;

    #region Writing
    public override bool CanWrite => false;
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    #endregion Writing
    
    #region Seeking
    public override bool CanSeek => false;
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    #endregion Seeking
}