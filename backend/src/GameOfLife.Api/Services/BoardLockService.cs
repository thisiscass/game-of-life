using System.Collections.Concurrent;

namespace GameOfLife.Services;

public class BoardLockService : IBoardLockService
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> AcquireAsync(Guid boardId, CancellationToken ct)
    {
        var sem = _locks.GetOrAdd(boardId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        return new Releaser(boardId, sem, _locks);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly Guid _id;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _parent;
        private bool _disposed;

        public Releaser(
            Guid id,
            SemaphoreSlim sem,
            ConcurrentDictionary<Guid, SemaphoreSlim> parent)
        {
            _id = id; _semaphore = sem; 
            _parent = parent;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _semaphore.Release();

            if (_semaphore.CurrentCount == 1) _parent.TryRemove(_id, out _);
            _disposed = true;
        }
    }
}
