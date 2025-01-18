using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Netconf.Netconf.Transport;

internal sealed class LegacyFramingProtocol : IFramingProtocol
{
    private LegacyFramingProtocol()
    {
    }

    public static LegacyFramingProtocol Instance { get; } = new();

    private static readonly ReadOnlyMemory<byte> Delimiter = "]]>]]>"u8.ToArray();

    public bool TryExtractPayload(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
    {
        var reader = new SequenceReader<byte>(buffer);
        if (!reader.TryReadTo(out payload, Delimiter.Span, advancePastDelimiter: true))
        {
            return false;
        }

        buffer = reader.UnreadSequence;
        return true;
    }

    public async Task WriteAsync(PipeWriter writer, ReadOnlyMemory<byte> message, CancellationToken cancellationToken)
    {
        var length = message.Length + Delimiter.Length;
        var memory = writer.GetMemory(length);
        message.CopyTo(memory[..message.Length]);
        Delimiter.CopyTo(memory[message.Length..][..Delimiter.Length]);
        writer.Advance(length);
        await writer.FlushAsync(cancellationToken);
    }

    public async Task WriteAsync(PipeWriter writer, ReadOnlySequence<byte> message, CancellationToken cancellationToken)
    {
        if (message.Length > Array.MaxLength)
        {
            throw new NotImplementedException();
        }
        var messageLength = (int)message.Length;
        
        var length = messageLength + Delimiter.Length;
        var memory = writer.GetMemory(length);
        CopyMessage(message, memory.Span);
        Delimiter.CopyTo(memory[messageLength..][..Delimiter.Length]);
        writer.Advance(length);
        await writer.FlushAsync(cancellationToken);

        static void CopyMessage(ReadOnlySequence<byte> source, Span<byte> destination)
        {
            while (!source.IsEmpty)
            {
                var sourceSegment = source.FirstSpan;
                var len = Math.Min(sourceSegment.Length, destination.Length);
                sourceSegment[..len].CopyTo(destination[..len]);
                destination = destination[len..];
                source = source.Slice(len);
            }
        }
    }
}