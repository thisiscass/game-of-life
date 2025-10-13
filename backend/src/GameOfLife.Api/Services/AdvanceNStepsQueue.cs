using System.Threading.Channels;

namespace GameOfLife.Services;

public record AdvanceRequest(Guid BoardId, int Steps);

public class AdvanceNStepsQueue : IAdvanceNStepsQueue
{
    private readonly Channel<AdvanceRequest> _channel;

    public AdvanceNStepsQueue(int capacity = 100)
    {
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
        await _channel.Writer.WriteAsync(data, cancellationToken);
    }

    public async ValueTask<AdvanceRequest> DequeueAsync(CancellationToken ct)
    {
        var data = await _channel.Reader.ReadAsync(ct);
        return data;
    }
}
