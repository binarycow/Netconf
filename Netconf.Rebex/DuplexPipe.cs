using System.IO.Pipelines;

namespace Netconf;

internal sealed class DuplexPipe : IDuplexPipe, IDisposable
{
    private readonly RebexStream stream;
    private static readonly StreamPipeReaderOptions readerOptions = new(leaveOpen: true);
    private static readonly StreamPipeWriterOptions writerOptions = new(leaveOpen: true);
    public DuplexPipe(RebexStream stream)
    {
        this.stream = stream;
        this.Input = PipeReader.Create(stream, readerOptions);
        this.Output = PipeWriter.Create(stream, writerOptions);
    }
    public PipeReader Input { get; }
    public PipeWriter Output { get; }

    public void Dispose() => this.stream.Dispose();
}