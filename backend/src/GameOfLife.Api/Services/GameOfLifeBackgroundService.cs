using GameOfLife.Api.Data;
using GameOfLife.Api.Models;
using GameOfLife.CrossCutting.Cache;
using GameOfLife.CrossCutting.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GameOfLife.Services;

public class GameOfLifeBackgroundService : BackgroundService
{
    private readonly BoardCache _cache;
    private readonly IHubContext<BoardHub> _hub;
    private readonly ILogger<GameOfLifeBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public GameOfLifeBackgroundService(BoardCache cache,
        IServiceScopeFactory scopeFactory,
        IHubContext<BoardHub> hub,
        ILogger<GameOfLifeBackgroundService> logger)
    {

        _scopeFactory = scopeFactory;
        _cache = cache;
        _hub = hub;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GameOfLifeContext>();

            var runningBoards = db.Boards!.Where(b => b.IsRunning);
            foreach (var board in runningBoards)
                board.IsRunning = false;

            await db.SaveChangesAsync();

            _cache.Clear();
        }

        await base.StartAsync(cancellationToken);

        _logger.LogInformation(">>> GameOfLifeBackgroundService is running...");
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                foreach (var board in _cache.GetAllRunningBoards())
                {
                    _logger.LogInformation(">>> GameOfLifeBackgroundService handling board: {boardId}", board.Id);

                    var grid = Board.DeserializeGrid(board.Grid);
                    var next = Board.BuildNextGeneration(grid);
                    var nextSerialized = Board.SerializeGrid(next);
                    board.Grid = nextSerialized;
                    board.Generation += 1;

                    var payload = new { boardId = board.Id, grid = next, generation = board.Generation };
                    await _hub.Clients.Group($"board-{board.Id}")
                        .SendAsync("UpdateBoard", payload, cancellationToken);

                    _logger.LogInformation(">>> UpdateBoard sent @{payload}", payload);
                }

                await Task.Delay(1000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[GameOfLifeBackgroundService] Error: {ex.GetBaseException().Message}");
                await Task.Delay(3000, cancellationToken);
            }

        }
    }
}