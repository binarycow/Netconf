namespace Netconf;

public interface IRpcError
{
    public ErrorType Type { get; }
    public string? Tag { get; }
    public ErrorSeverity Severity { get; }
    public string? AppTag { get; }
    public string? Path { get; }
    public string? Message { get; }
    public string? MessageLanguage { get; }
    public object? Info { get; }
    public string ToString();
}