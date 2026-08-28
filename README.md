# C# Assignment — REST API & WebSocket on Linux

Two ASP.NET Core (.NET 8) applications, developed and run on WSL Ubuntu 24.04.

## Structure
csharp-assignment/
├── CSharpAssignment.sln
└── src/
├── RestApi/ # REST API — CRUD over an in-memory store
└── WebSocketApp/ # WebSocket server — echo + broadcast


## Prerequisites

- .NET SDK 8.0
- Linux (developed on WSL Ubuntu 24.04)

## Running

Both apps bind to `0.0.0.0` so they are reachable from outside the WSL network namespace.

### REST API — port 5000

```bash
cd src/RestApi
dotnet run
```

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/healthcheck` | Liveness probe |
| GET | `/api/items` | List all items |
| GET | `/api/items/{id}` | Get one item (404 if missing) |
| POST | `/api/items` | Create an item |
| PUT | `/api/items/{id}` | Update an item |
| DELETE | `/api/items/{id}` | Delete an item |

Swagger UI is available at `/swagger` in Development.

### WebSocket — port 5001

```bash
cd src/WebSocketApp
dotnet run
```

| Endpoint | Description |
|----------|-------------|
| `GET /api/healthcheck` | Status plus live connection count |
| `WS /ws` | WebSocket endpoint |

On connect the server sends the assigned client id. Messages are echoed back to the sender and broadcast to all other connected clients.

## Design notes

**Storage behind an interface.** `IItemService` separates the controller from the store, so the in-memory implementation can be replaced with a database without touching the controller.

**Singleton lifetime.** The item store is registered as a singleton so state survives across requests. A scoped registration would give every request an empty store.

**Thread safety.** Both the item store and the connection registry use `ConcurrentDictionary`, since ASP.NET Core handles requests and socket lifecycles on multiple threads.

**Centralised error handling.** `ExceptionMiddleware` is registered first in the pipeline, so unhandled exceptions are logged in one place and the client receives a plain JSON message rather than a stack trace.

**Configuration over hardcoding.** Ports and the WebSocket keep-alive interval come from `appsettings.json`, overridable by environment variables.

**Keep-alive.** `WebSocketOptions.KeepAliveInterval` sends periodic pings so half-open connections are detected rather than left hanging.

**Graceful shutdown.** The receive loop observes `HttpContext.RequestAborted`, which fires on both client disconnect and host shutdown, so connections unwind cleanly.

## Testing

REST:

```bash
curl http://127.0.0.1:5000/api/healthcheck
curl -X POST http://127.0.0.1:5000/api/items \
  -H "Content-Type: application/json" \
  -d '{"name":"First item","description":"test"}'
curl http://127.0.0.1:5000/api/items
```

WebSocket — using the small Node client in this repo's sibling folder, or any WebSocket client:

```bash
node client.js A   # terminal 1
node client.js B   # terminal 2 — client A receives B's message
```
