using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSocketApp.Services;

/// <summary>
/// Tracks open sockets so messages can be broadcast to every client.
/// ConcurrentDictionary because connections open and close on different threads.
/// </summary>
public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ILogger<WebSocketConnectionManager> _logger;

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger) => _logger = logger;

    public int Count => _sockets.Count;

    public string Add(WebSocket socket)
    {
        var id = Guid.NewGuid().ToString("N");
        _sockets[id] = socket;
        _logger.LogInformation("Client {Id} connected. Total: {Count}", id, _sockets.Count);
        return id;
    }

    public void Remove(string id)
    {
        _sockets.TryRemove(id, out _);
        _logger.LogInformation("Client {Id} disconnected. Total: {Count}", id, _sockets.Count);
    }

    public IReadOnlyDictionary<string, WebSocket> All => _sockets;
}
