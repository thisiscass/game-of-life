namespace GameOfLife.Services;

public interface IAdvanceNStepsQueue
{
    ValueTask EnqueueAsync(AdvanceRequest data, CancellationToken cancellationToken);
    ValueTask<AdvanceRequest> DequeueAsync(CancellationToken cancellationToken);
}
