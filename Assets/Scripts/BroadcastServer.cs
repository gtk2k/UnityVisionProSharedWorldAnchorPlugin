using System.Threading;
using TMPro;
using UnityEngine;
using WebSocketSharp.Server;

public class BroadcastServer : MonoBehaviour
{
    [SerializeField] private int _port;
    [SerializeField] private TMP_Text _logList;

    private SynchronizationContext _ctx;
    private WebSocketServer _wss;

    private DebugLogger _debugLogger;

    private void Start()
    {
        _ctx = SynchronizationContext.Current;

        _wss = new WebSocketServer(_port);
        _wss.AddWebSocketService<SharedWorldAnchorBehaviour>("/", behaviour =>
        {
            behaviour.OnClientConnect += (id) =>
            {
                _ctx.Post(_ =>
                {
                    _debugLogger.Log($"=-== BroadcastServer OnClientConnect: {id}");
                }, null);
            };
            behaviour.OnClientMessage += (id, data) =>
            {
                _ctx.Post(_ =>
                {
                    _debugLogger.Log($"=-== BroadcastServer OnClientMessage");
                    _wss.WebSocketServices["/"].Sessions.Broadcast(data);
                }, null);
            };
            behaviour.OnClientDisconnect += (id, code, reason) =>
            {
                _ctx.Post(_ =>
                {
                    _debugLogger.Log($"=-== BroadcastServer OnClientDisconnect > code: {code}, reason: '{reason}'");
                }, null);
            };
            behaviour.OnClientError += (id, err) =>
            {
                _ctx.Post(_ =>
                {
                    _debugLogger.Log($"=-== BroadcastServer OnClientError > {err.Message}");
                }, null);
            };
        });
        _wss.Start();
        _debugLogger.Log($"=-== BroadcastServer Started");
    }

    public void Stop()
    {
        _wss?.Stop();
        _wss = null;
    }

    public void Broadcast(byte[] data)
    {
        _wss.WebSocketServices["/"].Sessions.Broadcast(data);
    }
}