using GameOfLife.Api.Data;
using GameOfLife.Api.Models;
using GameOfLife.CrossCutting.Cache;
using Microsoft.AspNetCore.SignalR;

namespace GameOfLife.CrossCutting.Hubs;

public class BoardHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BoardCache _cache;

    public BoardHub(IServiceScopeFactory scopeFactory, BoardCache cache)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Client joins a board room
    // public async Task JoinBoard(string boardId, CancellationToken cancellationToken = default)
    // {
    //     await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
    //     Console.WriteLine($"Client {Context.ConnectionId} joined board {boardId}");
    //     await Clients.Caller.SendAsync("JoinedBoard", boardId, cancellationToken);
    // }

    // Client leaves a board room
    public async Task StopBoard(string boardId, CancellationToken cancellationToken = default)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
        Console.WriteLine($"Client {Context.ConnectionId} left board {boardId}");
        await Clients.Caller.SendAsync("LeftBoard", boardId, cancellationToken);
    }

    public async Task StartBoard(string boardId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"board-{boardId}");

            await Clients.Group($"board-{boardId}")
                .SendAsync("StartBoard", new { boardId });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error StartBoard: {Message}", ex.Message);
        }

    }

    // public async Task StopBoard(Guid boardId)
    // {
    //     if (_cache.TryGetBoard(boardId, out var board))
    //     {
    //         board!.IsRunning = false;

    //         using var scope = _scopeFactory.CreateScope();
    //         var db = scope.ServiceProvider.GetRequiredService<GameOfLifeContext>();
    //         var entity = await db.Boards!.FindAsync(boardId);
    //         if (entity is not null)
    //         {
    //             entity.IsRunning = false;
    //             entity.Generation = board.Generation;
    //             entity.Grid = board.Grid;
    //             await db.SaveChangesAsync();
    //         }

    //         _cache.Remove(boardId);
    //     }

    // }

}