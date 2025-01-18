namespace Netconf.Netconf.Transport;

public interface ISshFactory
{
    ISshSession Connect(string host, int port, string username, string password);
}