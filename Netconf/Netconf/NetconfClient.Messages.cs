using System.Diagnostics;
using Netconf.Netconf.Models;
using Netconf.Netconf.Transport;

namespace Netconf.Netconf;

public sealed partial class NetconfClient
{
    private async Task MonitorPipeForMessages()
    {
        try
        {
            await foreach (var message in this.framingProtocol.ReadAllMessagesAsync<RpcMessage>(this.session.NetconfSubsystem.Input, CancellationToken.None))
            {
                await this.incomingMessages.Writer.WriteAsync(message);
            }
            this.incomingMessages.Writer.Complete();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private async Task CheckOutgoingMessages()
    {
        try
        {
            await foreach (var item in this.outgoingMessages.Reader.ReadAllAsync())
            {
                await this.framingProtocol.WriteAsync(this.session.NetconfSubsystem.Output, item, CancellationToken.None);
            }
            await this.session.NetconfSubsystem.Output.CompleteAsync();
        }
        catch (Exception e)
        {
            _ = e;
            throw;
        }

    }
    private async Task ProcessIncomingMessages()
    {
        try
        {
            await foreach (var message in this.incomingMessages.Reader.ReadAllAsync())
            {
                switch (message)
                {
                    case RpcRequest request:
                        this.ProcessIncomingMessage(request);
                        break;
                    case XElementRpcReply reply:
                        this.ProcessIncomingMessage(reply);
                        break; 
                    default:
                        throw new UnreachableException();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void ProcessIncomingMessage(XElementRpcReply message)
    {
        if (message.MessageId is null)
        {
            throw new NotImplementedException();
        }

        if (!this.listeners.TryRemove(message.MessageId, out var listener))
        {
            throw new NotImplementedException();
        }

        try
        {
            if (listener.CancellationToken.IsCancellationRequested)
            {
                listener.SetCancelled(listener.CancellationToken);
            }
            else
            {
                listener.ProcessReply(message);
            }
        }
        catch (OperationCanceledException e)
        {
            listener.SetCancelled(e);
        }
        catch (Exception e)
        {
            listener.SetException(e);
        }
        finally
        {
            listener.Dispose();
        }
    }
    private void ProcessIncomingMessage(RpcRequest message)
    {
        throw new NotImplementedException();
    }
}