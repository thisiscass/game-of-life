using GameOfLife.Models;
using GameOfLife.CrossCutting.Cache;

namespace GameOfLife.Tests.CrossCutting.Cache;

public class BoardCacheTests
{
    private readonly BoardCache _cache;

    public BoardCacheTests()
    {
        _cache = new BoardCache();
    }

    [Fact]
    public void GivenBoard_WhenAddOrUpdate_ThenBoardIsStored()
    {
        // Arrange
        var board = new Board(Guid.NewGuid(), "grid", DateTime.UtcNow);

        // Act
        _cache.AddOrUpdate(board);

        // Assert
        var result = _cache.TryGetBoard(board.Id, out var cachedBoard);
        Assert.True(result);
        Assert.Equal(board, cachedBoard);
    }

    [Fact]
    public void GivenExistingBoard_WhenAddOrUpdate_ThenBoardIsUpdated()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var original = new Board(boardId, "original", DateTime.UtcNow);
        var updated = new Board(boardId, "updated", DateTime.UtcNow);

        _cache.AddOrUpdate(original);

        // Act
        _cache.AddOrUpdate(updated);

        // Assert
        var found = _cache.TryGetBoard(boardId, out var cached);
        Assert.True(found);
        Assert.Equal("updated", cached!.Grid);
    }

    [Fact]
    public void GivenMultipleBoards_WhenGetAllRunningBoards_ThenReturnsAll()
    {
        // Arrange
        var board1 = new Board(Guid.NewGuid(), "grid1", DateTime.UtcNow);
        var board2 = new Board(Guid.NewGuid(), "grid2", DateTime.UtcNow);

        _cache.AddOrUpdate(board1);
        _cache.AddOrUpdate(board2);

        // Act
        var all = _cache.GetAllRunningBoards();

        // Assert
        Assert.Equal(2, all.Count());
        Assert.Contains(board1, all);
        Assert.Contains(board2, all);
    }

    [Fact]
    public void GivenExistingBoard_WhenTryRemoveBoard_ThenRemovesAndReturnsTrue()
    {
        // Arrange
        var board = new Board(Guid.NewGuid(), "grid", DateTime.UtcNow);
        _cache.AddOrUpdate(board);

        // Act
        var result = _cache.TryRemoveBoard(board.Id, out var removed);

        // Assert
        Assert.True(result);
        Assert.Equal(board, removed);
        Assert.False(_cache.TryGetBoard(board.Id, out _));
    }

    [Fact]
    public void GivenNonExistentBoard_WhenTryRemoveBoard_ThenReturnsFalse()
    {
        // Act
        var result = _cache.TryRemoveBoard(Guid.NewGuid(), out var removed);

        // Assert
        Assert.False(result);
        Assert.Null(removed);
    }

    [Fact]
    public void GivenBoards_WhenClearIsCalled_ThenAllRemoved()
    {
        // Arrange
        _cache.AddOrUpdate(new Board(Guid.NewGuid(), "grid1", DateTime.UtcNow));
        _cache.AddOrUpdate(new Board(Guid.NewGuid(), "grid2", DateTime.UtcNow));

        // Act
        _cache.Clear();

        // Assert
        Assert.Empty(_cache.GetAllRunningBoards());
    }
}