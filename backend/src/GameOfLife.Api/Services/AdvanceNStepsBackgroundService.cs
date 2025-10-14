using GameOfLife.Api.Models;
using GameOfLife.CrossCutting.Hubs;
using GameOfLife.CrossCutting.Result;
using Microsoft.AspNetCore.SignalR;

namespace GameOfLife.Services;

public class AdvanceNStepsBackgroundService : BackgroundService
{
    private readonly IHubContext<BoardHub> _hub;
    private readonly ILogger<AdvanceNStepsBackgroundService> _logger;
    private readonly IAdvanceNStepsQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    public AdvanceNStepsBackgroundService(IAdvanceNStepsQueue queue,
        IHubContext<BoardHub> hub,
        ILogger<AdvanceNStepsBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _hub = hub;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        _logger.LogInformation(">>> AdvanceNStepsBackgroundService is running...");
    }


    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            AdvanceRequest request;
            try
            {
                request = await _queue.DequeueAsync(cancellationToken);

                _logger.LogInformation("[AdvanceNStepsBackgroundService] Handling board @{request}", request);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dequeuing request");
                continue;
            }

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var advanceNStepsService = scope.ServiceProvider.GetRequiredService<IAdvanceNStepsService>();

                    var result =
                        await advanceNStepsService.GetFinalResultOrFail(
                            request!.BoardId,
                            request.Steps,
                            cancellationToken);

                    if (result is Success<Board> succ)
                    {
                        await _hub.Clients.Group($"board-{request.BoardId}")
                            .SendAsync("AdvanceCompleted",
                                new { boardId = request.BoardId, grid = succ.Data!.Grid, generation = succ.Data!.Generation });
                    }
                    else
                    {
                        await _hub.Clients.Group($"board-{request.BoardId}")
                            .SendAsync("AdvanceFailed", new { boardId = request.BoardId });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[AdvanceNStepsBackgroundService] Error on handling board: {request.BoardId}.");
                await _hub.Clients.Group($"board-{request.BoardId}")
                    .SendAsync("AdvanceFailed", new { boardId = request.BoardId, error = ex.Message });
            }
        }
    }
}