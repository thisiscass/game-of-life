using GameOfLife.Api.CrossCutting;
using GameOfLife.Api.Data;
using GameOfLife.Api.Dtos;
using GameOfLife.Api.Models;
using GameOfLife.Api.Validations;

namespace GameOfLife.Api.Services;

public class GameOfLifeService : IGameOfLifeService
{
    private ILogger<GameOfLifeService> _logger;
    private readonly ICreateBoardValidation<CreateBoardDto> _validation;
    private readonly GameOfLifeContext _context;
    private readonly IClockService _clockService;

    public GameOfLifeService(
        ICreateBoardValidation<CreateBoardDto> validation,
        GameOfLifeContext context,
        ILogger<GameOfLifeService> logger,
        IClockService clockService)
    {
        _validation = validation;
        _context = context;
        _logger = logger;
        _clockService = clockService;

    }
    public async Task<Result<Guid>> Create(CreateBoardDto boardDto)
    {
        var validation = _validation.PerformValidation(boardDto);
        if (!validation.IsValid) return new Fail<Guid>(validation.GetErrors());

        try
        {
            var grid = Board.TransformGrid(boardDto.Grid);

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

    public Task<Result<Board>> GetAfterNSteps(int n)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Board>> GetNextGeneration(Guid boardId)
    {
        throw new NotImplementedException();
    }
}