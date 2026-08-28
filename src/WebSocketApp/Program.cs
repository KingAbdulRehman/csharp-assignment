using WebSocketApp.Services;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration.GetValue<int>("WebSocket:Port", 5001);
var keepAliveSeconds = builder.Configuration.GetValue<int>("WebSocket:KeepAliveSeconds", 30);

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<WebSocketHandler>();

var app = builder.Build();

// KeepAliveInterval sends periodic pings so dead connections are detected
// instead of lingering open. Mirrors WS_KEEPALIVE_MS in the Express template.
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(keepAliveSeconds)
});

app.MapGet("/api/healthcheck", (WebSocketConnectionManager manager) =>
    Results.Ok(new { status = "websocket-app/healthcheck OK", connections = manager.Count }));

app.Map("/ws", async (HttpContext context, WebSocketHandler handler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { message = "WebSocket requests only" });
        return;
    }

    using var socket = await context.WebSockets.AcceptWebSocketAsync();

    // RequestAborted is triggered on client disconnect and on graceful shutdown,
    // so the receive loop unwinds instead of hanging.
    await handler.HandleAsync(socket, context.RequestAborted);
});

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("WebSocket server starting on port {Port}, keepalive={KeepAlive}s", port, keepAliveSeconds);

app.Run();
