using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class BroadcastClient
{
    public event Action<string, byte[]> OnMessage;

    private SynchronizationContext _ctx;
    private WebSocket _ws;
    private bool _isID;
    public string ID;
    public BroadcastClient(SynchronizationContext ctx)
    {
        _ctx = ctx;
    }

    public void Connect(string url)
    {
        _ws = new WebSocket(url);
        _ws.OnOpen += (s, e) =>
        {
            _isID = true;
            _ctx.Post(_ =>
            {
                Debug.Log($"=-== BroadcastReceiver OnOpen");
            }, null);
        };
        _ws.OnMessage += (s, e) =>
        {
            if (_isID)
            {
                _isID = false;
                ID = e.Data;
            }
            else
            {
                _ctx.Post(_ =>
                {
                    OnMessage?.Invoke(ID, e.RawData);
                }, null);
            }
        };
            
        _ws.OnClose += (s, e) =>
        {
            _ctx.Post(_ =>
            {
                Debug.Log($"=-== BroadcastReceiver OnClose");
            }, null);
        };
        _ws.OnError += (s, e) =>
        {
            _ctx.Post(_ =>
            {
                Debug.Log($"=-== BroadcastReceiver Error: {e.Exception.Message}");
            }, null);
        };
        _ws.Connect();
    }

    public void Close()
    {
        _ws?.Close();
        _ws = null;
    }

    public void Send(byte[] data)
    {
        _ws?.Send(data);
    }
}