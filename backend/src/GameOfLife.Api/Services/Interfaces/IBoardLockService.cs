namespace GameOfLife.Services;

public interface IBoardLockService
{
    public Task<IDisposable> AcquireAsync(Guid boardId, CancellationToken ct);
}