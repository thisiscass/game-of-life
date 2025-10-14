using GameOfLife.CrossCutting.Result;
using GameOfLife.Models;
using GameOfLife.Services;
using GameOfLife.Repositories;

namespace GameOfLife.Tests.Services;

public class AdvanceNStepsServiceTests
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardLockService _boardLockService;
    private readonly IClockService _clockService;

    private readonly AdvanceNStepsService _service;

    public AdvanceNStepsServiceTests()
    {
        _boardRepository = Substitute.For<IBoardRepository>();
        _boardLockService = Substitute.For<IBoardLockService>();
        _clockService = Substitute.For<IClockService>();

        _service = new AdvanceNStepsService(
            _clockService,
            _boardLockService,
            _boardRepository
        );

        // Default: AcquireAsync returns a disposable lease that does nothing
        _boardLockService.AcquireAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult<IDisposable>(new DummyLease()));
    }

    private class DummyLease : IDisposable
    {
        public void Dispose() { /* no-op */ }
    }

    [Fact]
    public async Task GivenStepsLessThanOne_WhenGetFinalResultOrFailIsCalled_ThenReturnsFail()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        // Act
        var result = await _service.GetFinalResultOrFail(boardId, 0, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail<Board>>(result);
        Assert.Contains("Invalid step", string.Join(",", fail.Errors));
        await _boardRepository.DidNotReceive().GetById(Arg.Any<Guid>());
        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenBoardDoesNotExist_WhenGetFinalResultOrFailIsCalled_ThenReturnsFail()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _boardRepository.GetById(boardId).Returns(Task.FromResult<Board?>(null));

        // Act
        var result = await _service.GetFinalResultOrFail(boardId, 5, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail<Board>>(result);
        Assert.Contains("Invalid board", string.Join(",", fail.Errors));
        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenSingleLiveCell_WhenGetFinalResultOrFailIsCalled_ThenConcludesAsEmptyGridAndSavesBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var initialGrid = new int[][] { new[] { 1 } };
        var serialized = Board.SerializeGrid(initialGrid); // "1"
        var board = new Board(boardId, serialized, DateTime.UtcNow) { Generation = 0 };

        _boardRepository.GetById(boardId).Returns(board);

        var now = new DateTime(2025, 10, 13, 12, 0, 0, DateTimeKind.Utc);
        _clockService.CurrentUtc.Returns(now);

        _boardRepository.Update(Arg.Any<Board>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetFinalResultOrFail(boardId, 1, CancellationToken.None);

        // Assert
        var success = Assert.IsType<Success<Board>>(result);
        var returnedBoard = success.Data;
        Assert.NotNull(returnedBoard);

        Assert.DoesNotContain("1", returnedBoard.Grid);
        Assert.Equal(1, returnedBoard.Generation);
        Assert.Equal(now, returnedBoard.LatestUpdateAt);

        await _boardRepository.Received(1).Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenBlinkerWithTwoSteps_WhenGetFinalResultOrFailIsCalled_ThenDetectsRepeatAndReturnsConcludedBoard()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var grid = new int[][]
        {
                new[] { 0,0,0 },
                new[] { 1,1,1 },
                new[] { 0,0,0 }
        };
        var serialized = Board.SerializeGrid(grid);
        var board = new Board(boardId, serialized, DateTime.UtcNow) { Generation = 0 };

        _boardRepository.GetById(boardId).Returns(board);

        var now = DateTime.UtcNow;
        _clockService.CurrentUtc.Returns(now);
        _boardRepository.Update(Arg.Any<Board>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(1));

        // Act
        var result = await _service.GetFinalResultOrFail(boardId, 2, CancellationToken.None);

        // Assert
        var success = Assert.IsType<Success<Board>>(result);
        var returnedBoard = success.Data;
        Assert.NotNull(returnedBoard);

        Assert.Equal(2, returnedBoard.Generation);

        var expectedAfterTwo = Board.SerializeGrid(Board.BuildNextGeneration(Board.BuildNextGeneration(Board.DeserializeGrid(serialized))));
        Assert.Equal(expectedAfterTwo, returnedBoard.Grid);

        await _boardRepository.Received(1).Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenNonConcludingSteps_WhenGetFinalResultOrFailIsCalled_ThenReturnsFailNoSave()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var grid = new int[][]
        {
                new[] { 0,1,0 },
                new[] { 1,1,1 },
                new[] { 0,0,0 }
        };
        var serialized = Board.SerializeGrid(grid);
        var board = new Board(boardId, serialized, DateTime.UtcNow) { Generation = 0 };

        _boardRepository.GetById(boardId).Returns(board);

        // Act
        var result = await _service.GetFinalResultOrFail(boardId, 1, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail<Board>>(result);
        Assert.Contains("No conclusion", string.Join(",", fail.Errors));

        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenCancellationRequestedDuringLoop_WhenGetFinalResultOrFailIsCalled_ThenCancellationIsObserved()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var grid = new int[][] { new[] { 0, 1 }, new[] { 1, 0 } };
        var serialized = Board.SerializeGrid(grid);
        var board = new Board(boardId, serialized, DateTime.UtcNow) { Generation = 0 };
        _boardRepository.GetById(boardId).Returns(board);

        var cts = new CancellationTokenSource();

        _boardLockService.AcquireAsync(boardId, Arg.Any<CancellationToken>()).Returns(ci =>
        {
            cts.Cancel();
            return Task.FromResult<IDisposable>(new DummyLease());
        });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _service.GetFinalResultOrFail(boardId, 10, cts.Token));
    }
}

