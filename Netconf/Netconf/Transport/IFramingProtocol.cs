using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Netconf.Netconf.Transport;

internal interface IFramingProtocol
{
    public static IFramingProtocol Legacy => LegacyFramingProtocol.Instance;
    public static IFramingProtocol Chunked => ChunkedFramingProtocol.Instance;
    public bool TryExtractPayload(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload);

    public Task WriteAsync(
        PipeWriter writer,
        ReadOnlyMemory<byte> message,
        CancellationToken cancellationToken
    );
    public Task WriteAsync(
        PipeWriter writer,
        ReadOnlySequence<byte> message,
        CancellationToken cancellationToken
    );
}

internal static class FramingProtocolExtensions
{
    private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);
    public static Task WriteAsync<TMessage>(
        this IFramingProtocol framingProtocol,
        PipeWriter pipe,
        TMessage message,
        CancellationToken cancellationToken
    ) where TMessage : IXmlFormattable
    {
        var document = new XDocument(message.ToXElement())
        {
            Declaration = new(version: "1.0", encoding: "UTF-8", standalone: null),
        };
        using var memoryStream = new SequenceStreamWriter();
        using var textWriter = new StreamWriter(memoryStream, Utf8NoBom);
        document.Save(textWriter);
        return framingProtocol.WriteAsync(pipe, memoryStream.Sequence, cancellationToken);
    }

    
    public static ValueTask<TMessage?> ReadSingleMessageAsync<TMessage>(
        this IFramingProtocol framingProtocol,
        PipeReader reader,
        CancellationToken cancellationToken
    ) where TMessage : class, IXmlParsable<TMessage>
        => ReadSingleMessageAsync(
            framingProtocol,
            reader,
            XmlParsable.ParseDocument<TMessage>,
            cancellationToken
        );

    public static async ValueTask<TMessage?> ReadSingleMessageAsync<TMessage>(
        this IFramingProtocol framingProtocol,
        PipeReader reader,
        Func<ReadOnlySequence<byte>, TMessage> parseMessage,
        CancellationToken cancellationToken
    ) where TMessage : class
    {
        while (true)
        {
            var result = await reader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;
            var consumed = buffer.Start;
            var examined = buffer.End;
            try
            {
                if (framingProtocol.TryExtractPayload(ref buffer, out var payload))
                {
                    var message = parseMessage(payload);
                    consumed = buffer.Start;
                    examined = consumed;
                    return message;
                }
                if (result.IsCompleted)
                {
                    if (buffer.Length > 0)
                    {
                        throw new InvalidDataException("Incomplete message.");
                    }
                    break;
                }
            }
            finally
            {
                reader.AdvanceTo(consumed, examined);
            }
        }
        return null;
    }
    
    public static IAsyncEnumerable<TMessage> ReadAllMessagesAsync<TMessage>(
        this IFramingProtocol framingProtocol,
        PipeReader reader,
        CancellationToken cancellationToken
    ) where TMessage : class, IXmlParsable<TMessage>
        => ReadAllMessagesAsync(
            framingProtocol,
            reader,
            XmlParsable.ParseDocument<TMessage>,
            cancellationToken
        );
    
    public static async IAsyncEnumerable<TMessage> ReadAllMessagesAsync<TMessage>(
        this IFramingProtocol framingProtocol,
        PipeReader reader,
        Func<ReadOnlySequence<byte>, TMessage> parseMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        try
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = result.Buffer;

                try
                {
                    // Process all messages from the buffer, modifying the input buffer on each
                    // iteration.
                    while (framingProtocol.TryExtractPayload(ref buffer, out var payload))
                    {
                        yield return parseMessage(payload);
                    }

                    // There's no more data to be processed.
                    if (result.IsCompleted)
                    {
                        if (buffer.Length > 0)
                        {
                            // The message is incomplete and there's no more data to process.
                            throw new InvalidDataException("Incomplete message.");
                        }
                        break;
                    }
                }
                finally
                {
                    // Since all messages in the buffer are being processed, you can use the
                    // remaining buffer's Start and End position to determine consumed and examined.
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }
        finally
        {
            await reader.CompleteAsync();
        }
    }
}