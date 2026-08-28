using System.Net.WebSockets;
using System.Text;

namespace WebSocketApp.Services;

/// <summary>
/// Owns the receive loop for a single connection: reads frames, echoes back,
/// broadcasts to others, and closes cleanly on shutdown or client disconnect.
/// </summary>
public class WebSocketHandler
{
    private const int BufferSize = 4 * 1024;

    private readonly WebSocketConnectionManager _manager;
    private readonly ILogger<WebSocketHandler> _logger;

    public WebSocketHandler(WebSocketConnectionManager manager, ILogger<WebSocketHandler> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    public async Task HandleAsync(WebSocket socket, CancellationToken ct)
    {
        var id = _manager.Add(socket);

        try
        {
            await SendAsync(socket, $"connected: {id}", ct);
            await BroadcastAsync($"client {id} joined", id, ct);

            var buffer = new byte[BufferSize];

            while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", ct);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogInformation("Received from {Id}: {Message}", id, message);

                await SendAsync(socket, $"echo: {message}", ct);
                await BroadcastAsync($"{id}: {message}", id, ct);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Connection {Id} cancelled during shutdown", id);
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning("Connection {Id} closed unexpectedly: {Message}", id, ex.Message);
        }
        finally
        {
            _manager.Remove(id);
        }
    }

    private static async Task SendAsync(WebSocket socket, string message, CancellationToken ct)
    {
        if (socket.State != WebSocketState.Open) return;
        var bytes = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
    }

    private async Task BroadcastAsync(string message, string exceptId, CancellationToken ct)
    {
        foreach (var (id, socket) in _manager.All)
        {
            if (id == exceptId) continue;
            await SendAsync(socket, message, ct);
        }
    }
}
