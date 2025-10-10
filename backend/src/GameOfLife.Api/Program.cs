using System.Text.Json;
using GameOfLife.Api.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Setting up log
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add appsettings.json and environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var dbHost = builder.Configuration["DB_HOST"] ?? builder.Configuration["Database:Host"];
var dbPort = builder.Configuration["DB_PORT"] ?? builder.Configuration["Database:Port"];
var dbName = builder.Configuration["DB_NAME"] ?? builder.Configuration["Database:Name"];
var dbUser = builder.Configuration["DB_USER"] ?? builder.Configuration["Database:User"];
var dbPassword = builder.Configuration["DB_PASSWORD"] ?? builder.Configuration["Database:Password"];

// Build the connection string
var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GameOfLifeContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: connectionString,
        name: "postgres",
        tags: new[] { "ready" }
    );

var app = builder.Build();

app.Logger.LogInformation("🚀 Game of Life API is starting up...");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var lifetime = app.Lifetime;

lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("✅ Game of Life API is now running in {EnvironmentName}...", app.Environment.EnvironmentName);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});


app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapGet("/", () => "Game of Life API running...");

app.Run();
