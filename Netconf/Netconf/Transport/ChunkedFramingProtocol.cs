using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;

namespace Netconf.Netconf.Transport;

internal sealed class ChunkedFramingProtocol : IFramingProtocol
{
    private ChunkedFramingProtocol() { }
    public static ChunkedFramingProtocol Instance { get; } = new();
    public bool TryExtractPayload(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
    {
        var sequenceReader = new SequenceReader<byte>(buffer);
        if (!TryParseOneChunk(ref sequenceReader, out var chunkPayload))
        {
            payload = default;
            return false;
        }
        if (sequenceReader.IsNext(EndOfChunks))
        {
            payload = chunkPayload;
            sequenceReader.Advance(EndOfChunks.Length);
            buffer = sequenceReader.UnreadSequence;
            return true;
        }
        if (sequenceReader.UnreadSequence.IsEmpty)
        {
            payload = default;
            return false;
        }
        var (first, last) = MemorySegment.Create(chunkPayload);

        while (!sequenceReader.IsNext(EndOfChunks) && TryParseOneChunk(ref sequenceReader, out chunkPayload))
        {
            last = last.Append(chunkPayload);
        }
        if (!sequenceReader.IsNext(EndOfChunks))
        {
            payload = default;
            return false;
        }
        sequenceReader.Advance(EndOfChunks.Length);
        buffer = sequenceReader.UnreadSequence;
        payload = new(first, 0, last, last.Memory.Length);
        return true;
    }
    
    
    /*
       Chunked-Message = 1*chunk
                          end-of-chunks

        chunk           = LF HASH chunk-size LF
                          chunk-data
        chunk-size      = 1*DIGIT1 0*DIGIT
        chunk-data      = 1*OCTET

        end-of-chunks   = LF HASH HASH LF

        DIGIT1          = %x31-39
        DIGIT           = %x30-39
        HASH            = %x23
        LF              = %x0A
        OCTET           = %x00-FF
     */


    private static bool TryParseOneChunk(ref SequenceReader<byte> sequenceReader, out ReadOnlySequence<byte> payload)
    {
        payload = default;
        if (sequenceReader.IsNext(EndOfChunks))
        {
            return false;
        }
        if (!sequenceReader.IsNext(StartOfChunk))
        {
            return false;
        }
        sequenceReader.Advance(StartOfChunk.Length);
        if (!sequenceReader.TryReadTo(out ReadOnlySequence<byte> lengthBytes, ChunkSizeDataDelimiter, advancePastDelimiter: true))
        {
            return false;
        }
        var payloadLength = ParseInt(lengthBytes);
        return sequenceReader.TryReadExact(payloadLength, out payload);
    }
    
    private static int ParseInt(ReadOnlySequence<byte> sequence)
    {
        if (sequence.IsSingleSegment)
        {
            return int.Parse(sequence.FirstSpan);
        }
        if (sequence.Length > 10)
        {
            // Too big for int.MaxValue
            throw new NotImplementedException();
        }
        Span<byte> bytes = stackalloc byte[10];
        Fill(sequence, bytes);
        return int.Parse(bytes);

        static void Fill(ReadOnlySequence<byte> source, scoped Span<byte> destination)
        {
            while (!source.IsEmpty)
            {
                var sourceSpan = source.FirstSpan;
                var len = Math.Min(sourceSpan.Length, destination.Length);
                sourceSpan = sourceSpan[..len];
                sourceSpan.CopyTo(destination[..len]);
                destination = destination[len..];
                source = source.Slice(len);
            }
        }
    }

    
    private static ReadOnlySpan<byte> EndOfChunks => "\n##\n"u8;
    private static ReadOnlySpan<byte> StartOfChunk => "\n#"u8;
    private static ReadOnlySpan<byte> ChunkSizeDataDelimiter => "\n"u8;
    private static int LargestOverheadLength => 10 /* Length of int.MaxValue */ + EndOfChunks.Length + StartOfChunk.Length + ChunkSizeDataDelimiter.Length;
    
    public Task WriteAsync(PipeWriter writer, ReadOnlyMemory<byte> message, CancellationToken cancellationToken) 
        => WriteChunk(writer, message, isLastChunk: true, cancellationToken);

    private static async Task WriteChunk(
        PipeWriter writer,
        ReadOnlyMemory<byte> chunk,
        bool isLastChunk,
        CancellationToken cancellationToken
    )
    {
        var destination = writer.GetMemory(LargestOverheadLength + chunk.Length);
        var written = FillChunk(destination.Span, chunk.Span, isLastChunk: isLastChunk);
        writer.Advance(written);
        await writer.FlushAsync(cancellationToken);
    }

    private static int FillChunk(
        Span<byte> destination,
        ReadOnlySpan<byte> chunk,
        bool isLastChunk
    )
    {
        var originalLength = destination.Length;
        
        StartOfChunk.CopyTo(destination);
        destination = destination[StartOfChunk.Length..];
        
        chunk.Length.TryFormat(destination, out var lengthOfChunkLength);
        destination = destination[lengthOfChunkLength..];
        
        ChunkSizeDataDelimiter.CopyTo(destination);
        destination = destination[ChunkSizeDataDelimiter.Length..];

        chunk.CopyTo(destination);
        destination = destination[chunk.Length..];
        
        if (isLastChunk)
        {
            EndOfChunks.CopyTo(destination);
            destination = destination[EndOfChunks.Length..];
        }
        
        return originalLength - destination.Length;
    }


    public async Task WriteAsync(PipeWriter writer, ReadOnlySequence<byte> message, CancellationToken cancellationToken)
    {
        while (!message.IsEmpty)
        {
            var chunk = message.First;
            var destination = writer.GetMemory(LargestOverheadLength + chunk.Length);
            var written = FillChunk(destination.Span, chunk.Span, isLastChunk: message.IsSingleSegment);
            writer.Advance(written);
            message = message.Slice(chunk.Length);
            await writer.FlushAsync(cancellationToken);
        }
    }
    
    

}