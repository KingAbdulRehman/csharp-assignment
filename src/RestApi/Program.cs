using RestApi.Middleware;
using RestApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Port comes from configuration (appsettings / env), not hardcoded.
var port = builder.Configuration.GetValue<int>("Api:Port", 5000);
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Singleton because the in-memory store must survive across requests.
// Swap this line for a database-backed implementation without touching controllers.
builder.Services.AddSingleton<IItemService, InMemoryItemService>();

var app = builder.Build();

// Registered first so it wraps everything downstream.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("REST API starting on port {Port}, env={Env}", port, app.Environment.EnvironmentName);

app.Run();
