using System.Threading.Channels;

namespace GameOfLife.Services;

public record AdvanceRequest(Guid BoardId, int Steps);

public class AdvanceNStepsQueue : IAdvanceNStepsQueue
{
    private readonly Channel<AdvanceRequest> _channel;
    private readonly ILogger<AdvanceNStepsQueue> _logger;

    public AdvanceNStepsQueue(
        ILogger<AdvanceNStepsQueue> logger,
        int capacity = 100)
    {
        _logger = logger;

        var options = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<AdvanceRequest>(options);
    }

    public async ValueTask EnqueueAsync(AdvanceRequest data, CancellationToken cancellationToken = default)
    {
        try
        {
            await _channel.Writer.WriteAsync(data, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdvanceNStepsQueue] Error to enqueue data: @{data}.", data);
        }
    }

    public async ValueTask<AdvanceRequest> DequeueAsync(CancellationToken ct)
    {
        try
        {
            var data = await _channel.Reader.ReadAsync(ct);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AdvanceNStepsQueue] Error to dequeue.");
            throw;
        }
    }
}
