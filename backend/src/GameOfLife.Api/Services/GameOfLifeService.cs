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

    public GameOfLifeService(
        ICreateBoardValidation<CreateBoardDto> validation,
        GameOfLifeContext context,
        ILogger<GameOfLifeService> logger,
        IClockService clockService,
        BoardCache boardCache,
        IHubContext<BoardHub> hubContext)
    {
        _validation = validation;
        _context = context;
        _logger = logger;
        _clockService = clockService;
        _boardCache = boardCache;
        _hubContext = hubContext;

    }
    public async Task<Result<Guid>> Create(CreateBoardDto boardDto, CancellationToken cancellationToken)
    {
        var validation = _validation.PerformValidation(boardDto);
        if (!validation.IsValid) return new Fail<Guid>(validation.GetErrors());

        try
        {
            var grid = Board.SerializeGrid(boardDto.Grid);

            var newBoard = new Board(Guid.NewGuid(), grid, _clockService.CurrentUtc);

            _context.Boards!.Add(newBoard);
            await _context.SaveChangesAsync();

            return new Success<Guid>(newBoard.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return new InternalError<Guid>(ex.Message);
        }
    }

    public async Task<Result<NextBoardResultDto>> GetNextGeneration(Guid boardId, CancellationToken cancellationToken)
    {
        var currentBoard = await _context.Boards.FindAsync(boardId);

        if (currentBoard == null) 
            return new Fail<NextBoardResultDto>(new List<string> { "Invalid board" });

        var grid = Board.DeserializeGrid(currentBoard.Grid);

        var nextBoard = Board.BuildNextGeneration(grid);
        currentBoard.Grid = Board.SerializeGrid(nextBoard);
        currentBoard.LatestUpdateAt = _clockService.CurrentUtc;
        currentBoard.Generation += 1;

        _context.Boards.Update(currentBoard);
        await _context.SaveChangesAsync();

        return new Success<NextBoardResultDto>(new NextBoardResultDto(currentBoard.Id, currentBoard.Generation, nextBoard));
    }

    public Task<Result> Advance(Guid boardId, int steps, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> Start(Guid boardId, CancellationToken cancellationToken)
    {
        var board = await _context.Boards.FindAsync(boardId);

        if (board == null || board.IsRunning) return new Fail(new List<string> { "Invalid board" });

        board.IsRunning = true;

        _boardCache.AddOrUpdate(board);
        await _context.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}