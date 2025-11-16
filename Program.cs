using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ShopStewardHub.DigitalTwin.Infrastructure.Database;
using ShopStewardHub.DigitalTwin.Infrastructure.Caching;
using ShopStewardHub.DigitalTwin.Services;
using ShopStewardHub.DigitalTwin.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Database contexts
builder.Services.AddDbContext<MetadataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.AddDbContext<TimeSeriesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TimescaleDB")));

// Redis caching
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Application services
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();

// SignalR for real-time updates
builder.Services.AddSignalR();

// CORS for UE5 and development tools
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:8080" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Shop Steward Hub Digital Twin API",
        Version = "v1",
        Description = "REST API for Unreal Engine 5 Digital Twin Integration",
        Contact = new OpenApiContact
        {
            Name = "Shop Steward Hub",
            Email = "support@shopsteward.local"
        }
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PostgreSQL")!, name: "postgres")
    .AddNpgSql(builder.Configuration.GetConnectionString("TimescaleDB")!, name: "timescale")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

// Build app
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop Steward Hub Digital Twin API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RealtimeDataHub>("/ws/realtime");

// Health check endpoint
app.MapHealthChecks("/health");

// Welcome message
app.MapGet("/", () => Results.Ok(new
{
    service = "Shop Steward Hub Digital Twin API",
    version = "1.0.0",
    status = "running",
    endpoints = new
    {
        swagger = "/swagger",
        health = "/health",
        websocket = "/ws/realtime",
        api = "/api"
    }
}));

app.Run();
