using TSAPIService.Services;
using TSAPIService.Hubs;
using Serilog;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/tsapi-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Windows Service desteği
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Host.UseWindowsService();
}

// Servisleri ekle
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TSAPI servislerini ekle
builder.Services.AddSingleton<ITSAPIClient, TSAPIClient>();
builder.Services.AddSingleton<IAgentService, AgentService>();
builder.Services.AddSingleton<ICallService, CallService>();
builder.Services.AddSingleton<IEventService, EventService>();
builder.Services.AddHostedService<TSAPIBackgroundService>();

// CORS yapılandırması
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Development ortamında Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();

// SignalR Hub'ını ekle
app.MapHub<TSAPIHub>("/tsapihub");

// Controller'ları ekle
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => new { Status = "OK", Timestamp = DateTime.UtcNow });

Log.Information("TSAPI Service başlatılıyor...");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TSAPI Service başlatılamadı");
}
finally
{
    Log.CloseAndFlush();
}