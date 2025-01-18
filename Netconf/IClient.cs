namespace Netconf;

public interface IClient : IDisposable, IAsyncDisposable
{
    public Task Completion { get; }
}