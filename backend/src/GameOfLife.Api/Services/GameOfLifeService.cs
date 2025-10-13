using GameOfLife.CrossCutting.Result;
using GameOfLife.Api.Data;
using GameOfLife.Api.Dtos;
using GameOfLife.Api.Models;
using GameOfLife.Api.Validations;
using GameOfLife.CrossCutting.Cache;
using Microsoft.AspNetCore.SignalR;
using GameOfLife.CrossCutting.Hubs;

namespace GameOfLife.Services;

public class GameOfLifeService : IGameOfLifeService
{
    private ILogger<GameOfLifeService> _logger;
    private readonly ICreateBoardValidation<CreateBoardDto> _validation;
    private readonly GameOfLifeContext _context;
    private readonly IClockService _clockService;
    private readonly BoardCache _boardCache;
    private readonly IHubContext<BoardHub> _hubContext;
    private readonly IAdvanceNStepsQueue _advanceNStepsQueue;

    public GameOfLifeService(
        ICreateBoardValidation<CreateBoardDto> validation,
        GameOfLifeContext context,
        ILogger<GameOfLifeService> logger,
        IClockService clockService,
        BoardCache boardCache,
        IHubContext<BoardHub> hubContext,
        IAdvanceNStepsQueue advanceNStepsQueue)
    {
        _validation = validation;
        _context = context;
        _logger = logger;
        _clockService = clockService;
        _boardCache = boardCache;
        _hubContext = hubContext;
        _advanceNStepsQueue = advanceNStepsQueue;
    }

    public async Task<IResult<CreateBoardResultDto>> Create(CreateBoardDto boardDto, CancellationToken cancellationToken)
    {
        var validation = _validation.PerformValidation(boardDto);
        if (!validation.IsValid) return new Fail<CreateBoardResultDto>(validation.GetErrors());

        var grid = Board.SerializeGrid(boardDto.Grid);

        var newBoard = new Board(Guid.NewGuid(), grid, _clockService.CurrentUtc);

        _context.Boards!.Add(newBoard);
        await _context.SaveChangesAsync();

        return new Success<CreateBoardResultDto>(new CreateBoardResultDto(newBoard.Id));

    }

    public async Task<IResult<NextBoardResultDto>> GetNextGeneration(Guid boardId, CancellationToken cancellationToken)
    {
        var currentBoard = await _context.Boards.FindAsync(boardId);

        if (currentBoard == null)
            return new Fail<NextBoardResultDto>(new List<string> { "Invalid board" });

        currentBoard.NextGeneration(_clockService.CurrentUtc);

        _context.Boards.Update(currentBoard);
        await _context.SaveChangesAsync();

        return new Success<NextBoardResultDto>(
                new NextBoardResultDto(currentBoard.Id,
                    currentBoard.Generation,
                    Board.DeserializeGrid(currentBoard.Grid)));
    }

    public async Task<CrossCutting.Result.IResult> Advance(Guid boardId, int steps, CancellationToken cancellationToken)
    {
        if (steps < 1)
            return new Fail(new List<string> { "Invalid steps." });

        var board = await _context.Boards.FindAsync(boardId);

        if (board == null)
            return new Fail(new List<string> { "Invalid board." });

        await _advanceNStepsQueue.EnqueueAsync(new AdvanceRequest(board.Id, steps), cancellationToken);

        return new Success();
    }

    public async Task<CrossCutting.Result.IResult> Start(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await _context.Boards.FindAsync(boardId);

        if (board == null || board.IsRunning) return new Fail(new List<string> { "Invalid board" });

        board.Start();

        _boardCache.AddOrUpdate(board);
        await _context.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}