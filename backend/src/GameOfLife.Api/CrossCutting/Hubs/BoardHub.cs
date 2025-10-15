using Microsoft.AspNetCore.SignalR;

namespace GameOfLife.CrossCutting.Hubs;

public class BoardHub : Hub
{
    private readonly ILogger<BoardHub> _logger;

    public BoardHub(ILogger<BoardHub> logger)
    {
        _logger = logger;
    }
    public override Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

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
}