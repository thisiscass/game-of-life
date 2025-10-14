using GameOfLife.Models;

namespace GameOfLife.Repositories;

public interface IBoardRepository
{
    public Task Add(Board board, CancellationToken cancellationToken = default);
    public Task<Board?> GetById(Guid id);
    public Task<IEnumerable<Board>> GetRunning(CancellationToken cancellationToken = default);
    public Task Update(Board board, CancellationToken cancellationToken = default);
    public Task Update(IEnumerable<Board> boards, CancellationToken cancellationToken = default);
}

