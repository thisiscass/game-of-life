using System.Text.Json;
using GameOfLife.Api.Data;
using GameOfLife.Configuration;
using GameOfLife.CrossCutting.Extensions;
using GameOfLife.CrossCutting.Hubs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Set up SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

// Set up logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Build the connection string
var connectionString = builder.Configuration.BuildPostgresConnectionString(builder.Environment);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<SwaggerOperationFilter>();
});

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});


builder.Services.AddGameOfLifeServices();

var app = builder.Build();

app.Logger.LogInformation("🚀 Game of Life API is starting up...");

app.UseSwagger();
app.UseSwaggerUI();

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

app.MapHub<BoardHub>("/board");

app.UseCors("CorsPolicy");

var lifetime = app.Lifetime;

lifetime.ApplicationStarted.Register(() =>
{
    app.Logger.LogInformation("✅ Game of Life API is now running in {EnvironmentName}...", app.Environment.EnvironmentName);
});

app.Run();
