using GameOfLife.Models;

namespace GameOfLife.CrossCutting.Cache;

public interface IBoardCache
{
    IEnumerable<Board> GetAllRunningBoards();
    void AddOrUpdate(Board board);
    void Clear();
    bool TryGetBoard(Guid id, out Board? board);
    bool TryRemoveBoard(Guid id, out Board? board);
}