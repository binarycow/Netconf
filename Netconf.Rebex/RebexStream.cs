using System.Buffers;
using Rebex.Net;

namespace Netconf;

internal sealed class RebexStream : Stream
{
    private readonly SshChannel channel;
    private bool disposed;

    public RebexStream(SshChannel channel)
    {
        this.channel = channel;
    }

    public override void Flush() => this.channel.GetAvailable();

    public override int Read(byte[] buffer, int offset, int count)
    {
        _ = this.channel.GetAvailable();
        return this.channel.Receive(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        byte[]? array = null;
        try
        {
            array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            var length = this.Read(array, 0, buffer.Length);
            array.AsSpan()[..length].CopyTo(buffer[..length]);
            return length;
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        byte[]? array = null;
        try
        {
            array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            var length = this.Read(array, 0, buffer.Length);
            array.AsSpan()[..length].CopyTo(buffer[..length]);
            return Task.FromResult(length);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        byte[]? array = null;
        try
        {
            array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            var length = this.Read(array, 0, buffer.Length);
            array.AsSpan()[..length].CopyTo(buffer.Span[..length]);
            return ValueTask.FromResult(length);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public override int ReadByte()
    {
        var array = new byte[1];
        return this.Read(array, 0, 1) == 1 ? array[0] : -1;
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        byte[]? array = null;
        try
        {
            array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            buffer.CopyTo(array.AsSpan());
            this.Write(array, 0, buffer.Length);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {

        byte[]? array = null;
        try
        {
            array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            buffer.CopyTo(array.AsMemory(0, buffer.Length));
            this.Write(array, 0, buffer.Length);
            return Task.CompletedTask;
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        byte[]? array = null;
        try
        {
            array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            buffer.CopyTo(array.AsMemory(0, buffer.Length));
            this.Write(array, 0, buffer.Length);
            return ValueTask.CompletedTask;
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    public override void WriteByte(byte value) => this.Write([value], 0, 1);
    public override void Write(byte[] buffer, int offset, int count) => this.channel.Send(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing || this.disposed)
        {
            return;
        }
        this.disposed = true;
        this.channel.Close();
        this.channel.Dispose();
    }

}