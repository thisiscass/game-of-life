using GameOfLife.CrossCutting.Result;
using GameOfLife.Api.Dtos;
using GameOfLife.Models;
using GameOfLife.Api.Validations;
using GameOfLife.CrossCutting.Cache;
using GameOfLife.Repositories;

namespace GameOfLife.Services;

public class GameOfLifeService : IGameOfLifeService
{
    private readonly ICreateBoardValidation<CreateBoardDto> _validation;
    private readonly IClockService _clockService;
    private readonly IBoardCache _boardCache;
    private readonly IAdvanceNStepsQueue _advanceNStepsQueue;
    private readonly IBoardRepository _boardRepository;

    public GameOfLifeService(
        ICreateBoardValidation<CreateBoardDto> validation,
        IClockService clockService,
        IBoardCache boardCache,
        IAdvanceNStepsQueue advanceNStepsQueue,
        IBoardRepository boardRepository)
    {
        _validation = validation;
        _clockService = clockService;
        _boardCache = boardCache;
        _advanceNStepsQueue = advanceNStepsQueue;
        _boardRepository = boardRepository;
    }

    public async Task<IResult<CreateBoardResultDto>> Create(CreateBoardDto boardDto, CancellationToken cancellationToken)
    {
        var validation = _validation.PerformValidation(boardDto);
        if (!validation.IsValid) 
            return new Fail<CreateBoardResultDto>(validation.GetErrors());

        var grid = Board.SerializeGrid(boardDto.Grid);

        var newBoard = new Board(Guid.NewGuid(), grid, _clockService.CurrentUtc);

        await _boardRepository.Add(newBoard, cancellationToken);

        return new Success<CreateBoardResultDto>(new CreateBoardResultDto(newBoard.Id));
    }

    public async Task<IResult<NextBoardResultDto>> GetNextGeneration(Guid boardId, CancellationToken cancellationToken)
    {
        var currentBoard = await _boardRepository.GetById(boardId);

        if (currentBoard == null)
            return new Fail<NextBoardResultDto>(new List<string> { "Invalid board" });

        currentBoard.NextGeneration(_clockService.CurrentUtc);

        await _boardRepository.Update(currentBoard, cancellationToken);

        return new Success<NextBoardResultDto>(
                new NextBoardResultDto(currentBoard.Id,
                    currentBoard.Generation,
                    Board.DeserializeGrid(currentBoard.Grid)));
    }

    public async Task<CrossCutting.Result.IResult> Advance(Guid boardId, int steps, CancellationToken cancellationToken)
    {
        if (steps < 1 || steps > 100)
            return new Fail("Invalid steps.");

        var board = await _boardRepository.GetById(boardId);

        if (board == null || board.IsRunning) return new Fail(new List<string> { "Invalid board" });

        await _advanceNStepsQueue.EnqueueAsync(new AdvanceRequest(board.Id, steps), cancellationToken);

        return new Success();
    }

    public async Task<CrossCutting.Result.IResult> Start(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetById(boardId);

        if (board == null || board.IsRunning) return new Fail(new List<string> { "Invalid board" });

        board.Start();

        _boardCache.AddOrUpdate(board);
        await _boardRepository.Update(board, cancellationToken);

        return new Success();
    }

    public async Task<CrossCutting.Result.IResult> CleanRunningBoards(CancellationToken cancellationToken)
    {
        var runningBoards = await _boardRepository.GetRunning(cancellationToken);
        foreach (var board in runningBoards)
            board.Stop();

        await _boardRepository.Update(runningBoards, cancellationToken);

        _boardCache.Clear();

        return new Success();
    }

    public async Task<CrossCutting.Result.IResult> Stop(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetById(boardId);

        if (board == null || !board.IsRunning)
            return new Fail("Invalid board");

        _boardCache.TryRemoveBoard(boardId, out _);
        board.Stop();
        
        await _boardRepository.Update(board);

        return new Success();
    }
}