using GameOfLife.Models;
using NSubstitute;
using GameOfLife.Api.Validations;
using GameOfLife.Api.Dtos;
using GameOfLife.Services;
using GameOfLife.CrossCutting.Cache;
using GameOfLife.Repositories;
using GameOfLife.CrossCutting.Result;

namespace GameOfLife.Tests.Services;

public class GameOfLifeServiceTests
{
    private readonly ICreateBoardValidation<CreateBoardDto> _validation;
    private readonly IClockService _clockService;
    private readonly IBoardCache _boardCache;
    private readonly IAdvanceNStepsQueue _advanceNStepsQueue;
    private readonly IBoardRepository _boardRepository;
    private readonly GameOfLifeService _service;

    public GameOfLifeServiceTests()
    {
        _validation = new CreateBoardValidation();
        _clockService = Substitute.For<IClockService>();
        _boardCache = Substitute.For<IBoardCache>();
        _advanceNStepsQueue = Substitute.For<IAdvanceNStepsQueue>();
        _boardRepository = Substitute.For<IBoardRepository>();

        _service = new GameOfLifeService(
            _validation,
            _clockService,
            _boardCache,
            _advanceNStepsQueue,
            _boardRepository
        );
    }

    [Fact]
    public async Task GivenInvalidDto_WhenCreateIsCalled_ThenReturnsFailAndDoesNotCallRepository()
    {
        // Arrange
        var dto = new CreateBoardDto(new int[][] { new[] { 0, 0 }, new[] { 2, 1 } });

        // Act
        var result = await _service.Create(dto, CancellationToken.None);

        // Assert
        Assert.IsType<Fail<CreateBoardResultDto>>(result);
        await _boardRepository.DidNotReceive().Add(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenValidDto_WhenCreateIsCalled_ThenReturnsSuccessAndCallsRepository()
    {
        // Arrange
        var dto = new CreateBoardDto(new int[][]
            {
                    new[] { 1, 0 },
                    new[] { 0, 1 }
            });

        var now = DateTime.UtcNow;
        _clockService.CurrentUtc.Returns(now);

        Board? capturedBoard = null;
        _boardRepository
            .Add(Arg.Do<Board>(b => capturedBoard = b), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Create(dto, CancellationToken.None);

        // Assert result
        var success = Assert.IsType<Success<CreateBoardResultDto>>(result);
        Assert.NotNull(success.Data);
        Assert.NotEqual(Guid.Empty, success.Data.boardId);

        // Assert repository interaction
        await _boardRepository.Received(1).Add(Arg.Any<Board>(), Arg.Any<CancellationToken>());

        // Assert the created Board values
        Assert.NotNull(capturedBoard);
        Assert.Equal(success.Data.boardId, capturedBoard!.Id);
        Assert.Equal(Board.SerializeGrid(dto.Grid), capturedBoard.Grid);
        Assert.Equal(now, capturedBoard.LatestUpdateAt);
        Assert.Equal(0, capturedBoard.Generation);
        Assert.False(capturedBoard.IsRunning);
    }

    [Fact]
    public async Task GivenValidDto_WhenCreateIsCalled_ThenBoardIsCreatedWithCurrentUtcFromClock()
    {
        // Arrange
        var dto = new CreateBoardDto(new int[][] { new[] { 1 } });

        var expectedTime = new DateTime(2025, 10, 13, 12, 0, 0, DateTimeKind.Utc);
        _clockService.CurrentUtc.Returns(expectedTime);

        Board? capturedBoard = null;
        _boardRepository
            .Add(Arg.Do<Board>(b => capturedBoard = b), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Create(dto, CancellationToken.None);

        // Assert
        Assert.IsType<Success<CreateBoardResultDto>>(result);
        Assert.Equal(expectedTime, capturedBoard!.LatestUpdateAt);
    }

    [Fact]
    public async Task GivenInvalidBoardId_WhenGetNextGenerationIsCalled_ThenReturnsFailResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        _boardRepository.GetById(boardId).Returns((Board?)null);

        // Act
        var result = await _service.GetNextGeneration(boardId, CancellationToken.None);

        // Assert
        Assert.IsType<Fail<NextBoardResultDto>>(result);
        var fail = result as Fail<NextBoardResultDto>;
        Assert.Contains("Invalid board", fail!.Errors);

        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenValidBoard_WhenGetNextGenerationIsCalled_ThenCallsNextGenerationAndUpdatesRepository()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var initialGrid = Board.SerializeGrid(new int[][] { new[] { 0, 1, 0 } });
        var board = new Board(boardId, initialGrid, DateTime.UtcNow);

        _boardRepository.GetById(boardId).Returns(board);

        var expectedTime = new DateTime(2025, 10, 13, 12, 0, 0, DateTimeKind.Utc);
        _clockService.CurrentUtc.Returns(expectedTime);

        Board? updatedBoard = null;
        _boardRepository
            .Update(Arg.Do<Board>(b => updatedBoard = b), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetNextGeneration(boardId, CancellationToken.None);

        // Assert
        Assert.IsType<Success<NextBoardResultDto>>(result);
        var success = result as Success<NextBoardResultDto>;
        Assert.Equal(success!.Data!.boardId, boardId);
        Assert.Equal(success!.Data!.generation, board.Generation);
        Assert.NotEmpty(success!.Data!.grid);

        await _boardRepository.Received(1).Update(board, Arg.Any<CancellationToken>());

        // the board should have updated its latest update time
        Assert.Equal(updatedBoard!.LatestUpdateAt, expectedTime);
    }

    [Fact]
    public async Task GivenBoard_WhenGetNextGenerationIsCalled_ThenReturnsDeserializedGridInResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var grid = new int[][] { new[] { 1, 0, 1 }, new[] { 0, 0, 0 }, new[] { 1, 1, 1 } };
        var expectedNext = new int[][]
        {
            new[] { 0, 0, 0 },
            new[] { 1, 0, 1 },
            new[] { 0, 1, 0 }
        };
        var serialized = Board.SerializeGrid(grid);
        var board = new Board(boardId, serialized, DateTime.UtcNow);

        _boardRepository.GetById(boardId).Returns(board);
        _clockService.CurrentUtc.Returns(DateTime.UtcNow);

        // Act
        var result = await _service.GetNextGeneration(boardId, CancellationToken.None);

        // Assert
        var success = Assert.IsType<Success<NextBoardResultDto>>(result);
        Assert.Equal(expectedNext, success.Data!.grid);
    }

    [Fact]
    public async Task GivenStepsLessThanOne_WhenAdvanceIsCalled_ThenReturnsFailResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        // Act
        var result = await _service.Advance(boardId, 0, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Equal("Invalid steps.", fail.Errors.First());
        await _advanceNStepsQueue.DidNotReceive().EnqueueAsync(Arg.Any<AdvanceRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenStepsGreaterThanHundred_WhenAdvanceIsCalled_ThenReturnsFailResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();

        // Act
        var result = await _service.Advance(boardId, 101, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Equal("Invalid steps.", fail.Errors.First());
        await _advanceNStepsQueue.DidNotReceive().EnqueueAsync(Arg.Any<AdvanceRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenBoardIsNull_WhenAdvanceIsCalled_ThenReturnsFailResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _boardRepository.GetById(boardId).Returns((Board?)null);

        // Act
        var result = await _service.Advance(boardId, 5, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Contains("Invalid board", fail.Errors);
        await _advanceNStepsQueue.DidNotReceive().EnqueueAsync(Arg.Any<AdvanceRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenBoardIsRunning_WhenAdvanceIsCalled_ThenReturnsFailResult()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(boardId, "1,1;1,1", DateTime.UtcNow) { IsRunning = true };
        _boardRepository.GetById(boardId).Returns(board);

        // Act
        var result = await _service.Advance(boardId, 3, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Contains("Invalid board", fail.Errors);
        await _advanceNStepsQueue.DidNotReceive().EnqueueAsync(Arg.Any<AdvanceRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenValidBoardAndSteps_WhenAdvanceIsCalled_ThenEnqueuesAdvanceRequestAndReturnsSuccess()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(boardId, "1,0;0,1", DateTime.UtcNow);
        _boardRepository.GetById(boardId).Returns(board);

        // Act
        var result = await _service.Advance(boardId, 5, CancellationToken.None);

        // Assert
        var success = Assert.IsType<Success>(result);
        await _advanceNStepsQueue.Received(1).EnqueueAsync(
            Arg.Is<AdvanceRequest>(req => req.BoardId == boardId && req.Steps == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenValidBoard_WhenStartIsCalled_ThenBoardStartsAndIsUpdated()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(boardId, "serializedGrid", DateTime.UtcNow);

        _boardRepository.GetById(boardId).Returns(board);
        _boardCache.AddOrUpdate(Arg.Any<Board>());

        // Act
        var result = await _service.Start(boardId, CancellationToken.None);

        // Assert
        Assert.IsType<Success>(result);

        await _boardRepository.Received(1).Update(board, Arg.Any<CancellationToken>());
        _boardCache.Received(1).AddOrUpdate(board);
        Assert.True(board.IsRunning);
    }

    [Fact]
    public async Task GivenNullBoard_WhenStartIsCalled_ThenReturnsFail()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _boardRepository.GetById(boardId).Returns((Board?)null);

        _boardCache.AddOrUpdate(Arg.Any<Board>());

        // Act
        var result = await _service.Start(boardId, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Contains("Invalid board", fail.Errors);
        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
        _boardCache.DidNotReceive().AddOrUpdate(Arg.Any<Board>());
    }

    [Fact]
    public async Task GivenRunningBoard_WhenStartIsCalled_ThenReturnsFail()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(boardId, "serializedGrid", DateTime.UtcNow) { IsRunning = true };

        _boardRepository.GetById(boardId).Returns(board);

        _boardCache.AddOrUpdate(Arg.Any<Board>());

        // Act
        var result = await _service.Start(boardId, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Contains("Invalid board", fail.Errors);
        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>(), Arg.Any<CancellationToken>());
        _boardCache.DidNotReceive().AddOrUpdate(Arg.Any<Board>());
    }

    [Fact]
    public async Task GivenNoRunningBoards_WhenCleanRunningBoardsIsCalled_ThenUpdateIsCalledWithEmptyCollectionAndCacheCleared()
    {
        // Arrange
        var cancellation = CancellationToken.None;
        _boardRepository.GetRunning(cancellation).Returns(Task.FromResult<IEnumerable<Board>>(Array.Empty<Board>()));

        IEnumerable<Board>? updateArg = null;
        _boardRepository
            .Update(Arg.Do<IEnumerable<Board>>(b => updateArg = b), cancellation)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CleanRunningBoards(cancellation);

        // Assert
        Assert.IsType<Success>(result);

        await _boardRepository.Received(1).Update(Arg.Any<IEnumerable<Board>>(), cancellation);
        Assert.NotNull(updateArg);
        Assert.Empty(updateArg);

        _boardCache.Received(1).Clear();
    }

    [Fact]
    public async Task GivenSomeRunningBoards_WhenCleanRunningBoardsIsCalled_ThenBoardsAreStoppedUpdatedAndCacheCleared()
    {
        // Arrange
        var cancellation = CancellationToken.None;
        var b1 = new Board(Guid.NewGuid(), "1,0;0,1", DateTime.UtcNow) { IsRunning = true };
        var b2 = new Board(Guid.NewGuid(), "0,1;1,0", DateTime.UtcNow) { IsRunning = true };

        var running = new List<Board> { b1, b2 };
        _boardRepository.GetRunning(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IEnumerable<Board>>(running));

        IEnumerable<Board>? updatedBoards = null;
        _boardRepository
            .Update(Arg.Do<IEnumerable<Board>>(x => updatedBoards = x), cancellation)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CleanRunningBoards(cancellation);

        // Assert
        Assert.IsType<Success>(result);

        await _boardRepository.Received(1).Update(Arg.Any<IEnumerable<Board>>(), cancellation);

        Assert.NotNull(updatedBoards);
        var updatedList = updatedBoards!.ToList();
        Assert.Equal(2, updatedList.Count);

        Assert.Contains(updatedList, x => x.Id == b1.Id);
        Assert.Contains(updatedList, x => x.Id == b2.Id);

        Assert.All(updatedList, b => Assert.False(b.IsRunning));

        Assert.False(b1.IsRunning);
        Assert.False(b2.IsRunning);

        _boardCache.Received(1).Clear();
    }

    [Fact]
    public async Task GivenRepositoryUpdateThrows_WhenCleanRunningBoardsIsCalled_ThenExceptionPropagatesAndCacheIsNotCleared()
    {
        // Arrange
        var cancellation = CancellationToken.None;
        var b = new Board(Guid.NewGuid(), "1", DateTime.UtcNow) { IsRunning = true };
        var running = new List<Board> { b };
        _boardRepository.GetRunning(Arg.Any<CancellationToken>()).Returns(Task.FromResult<IEnumerable<Board>>(running));

        _boardRepository
            .Update(Arg.Any<IEnumerable<Board>>(), cancellation)
            .Returns<Task>(_ => throw new InvalidOperationException("DB problem"));

        // Act - Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CleanRunningBoards(cancellation));

        _boardCache.DidNotReceive().Clear();

        Assert.False(b.IsRunning);
    }

    [Fact]
    public async Task GivenNullBoard_WhenStopIsCalled_ThenReturnsFailAndDoesNotCallUpdateOrModifyCache()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _boardRepository.GetById(boardId).Returns((Board?)null);

        // Act
        var result = await _service.Stop(boardId, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Contains("Invalid board", fail.Errors);

        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>());
        Assert.False(_boardCache.TryGetBoard(boardId, out var _));
    }

    [Fact]
    public async Task GivenBoardNotRunning_WhenStopIsCalled_ThenReturnsFailAndDoesNotCallUpdateOrModifyCache()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(boardId, "0,0;0,0", DateTime.UtcNow) { IsRunning = false };
        _boardRepository.GetById(boardId).Returns(board);

        var otherBoardId = Guid.NewGuid();
        var otherBoard = new Board(otherBoardId, "0", DateTime.UtcNow);
        _boardCache.AddOrUpdate(otherBoard);

        // Act
        var result = await _service.Stop(boardId, CancellationToken.None);

        // Assert
        var fail = Assert.IsType<Fail>(result);
        Assert.Contains("Invalid board", fail.Errors);

        await _boardRepository.DidNotReceive().Update(Arg.Any<Board>());
    }

    [Fact]
    public async Task GivenRunningBoard_WhenStopIsCalled_ThenRemovesFromCacheStopsBoardUpdatesRepositoryAndReturnsSuccess()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(boardId, "1,0;0,1", DateTime.UtcNow) { IsRunning = true };

        _boardCache.AddOrUpdate(board);

        _boardRepository.GetById(boardId).Returns(board);

        _boardRepository.Update(board).Returns(Task.CompletedTask);

        // Act
        var result = await _service.Stop(boardId, CancellationToken.None);

        // Assert
        Assert.IsType<Success>(result);

        Assert.False(board.IsRunning);
        await _boardRepository.Received(1).Update(board);
        _boardCache.Received(1).TryRemoveBoard(Arg.Any<Guid>(), out _);
    }

    [Fact]
    public async Task GivenRunningBoard_WhenRepositoryUpdateThrows_ThenExceptionPropagates_BoardIsStoppedAndCacheClearedBeforeException()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        var board = new Board(boardId, "1", DateTime.UtcNow) { IsRunning = true };

        _boardCache.AddOrUpdate(board);

        _boardRepository.GetById(boardId).Returns(board);

        _boardRepository
            .When(x => x.Update(board))
            .Do(_ => throw new InvalidOperationException("DB failure"));

        // Act - Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.Stop(boardId, CancellationToken.None));

        _boardCache.DidNotReceive().TryGetBoard(boardId, out _);
        Assert.False(board.IsRunning);
    }
}
