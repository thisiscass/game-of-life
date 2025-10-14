using System.Collections.Concurrent;
using GameOfLife.Models;

namespace GameOfLife.CrossCutting.Cache;

public class BoardCache : IBoardCache
{
    private readonly ConcurrentDictionary<Guid, Board> _boards = new();

    public IEnumerable<Board> GetAllRunningBoards() => _boards.Values;

    public void AddOrUpdate(Board board) => 
        _boards.AddOrUpdate(board.Id, board, (_, _) => board);
    
    public void Clear() => _boards.Clear();

    public bool TryGetBoard(Guid id, out Board? board) => _boards.TryGetValue(id, out board);

    public bool TryRemoveBoard(Guid id, out Board? board) => _boards.TryRemove(id, out board);
}
