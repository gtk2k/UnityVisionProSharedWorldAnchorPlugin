using System;
using WebSocketSharp;
using WebSocketSharp.Server;

public class SharedWorldAnchorBehaviour : WebSocketBehavior
{
    public event Action<string> OnClientConnect;
    public event Action<string, byte[]> OnClientMessage;
    public event Action<string, ushort, string> OnClientDisconnect;
    public event Action<string, Exception> OnClientError;

    protected override void OnOpen()
    {
        Send(ID);
        OnClientConnect?.Invoke(ID);
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        OnClientMessage?.Invoke(ID, e.RawData);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        OnClientDisconnect?.Invoke(ID, e.Code, e.Reason);
    }

    protected override void OnError(ErrorEventArgs e)
    {
        OnClientError?.Invoke(ID, e.Exception);
    }

    public void _Send(string data)
    {
        Send(data);
    }

    public void _Send(byte[] data)
    {
        Send(data);
    }
}