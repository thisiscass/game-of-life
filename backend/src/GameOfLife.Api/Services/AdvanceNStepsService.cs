using GameOfLife.Api.Data;
using GameOfLife.Api.Models;
using GameOfLife.CrossCutting.Result;

namespace GameOfLife.Services;

public class AdvanceNStepsService : IAdvanceNStepsService
{
    private readonly GameOfLifeContext _context;
    private readonly IClockService _clockService;
    private readonly IBoardLockService _boardLockService;

    public AdvanceNStepsService(
        GameOfLifeContext context,
        IClockService clockService,
        IBoardLockService boardLockService)
    {
        _context = context;
        _clockService = clockService;
        _boardLockService = boardLockService;
    }

    private bool IsEmptyGrid(string grid)
    {
        return !grid.Contains('1');
    }

    public async Task<IResult<Board>> GetFinalResultOrFail(
        Guid boardId,
        int steps,
        CancellationToken cancellationToken)
    {
        if (steps < 1)
            return new Fail<Board>("Invalid step.");

        // Ensuring not concurrent ride for same board
        using var lease = await _boardLockService.AcquireAsync(boardId, cancellationToken);

        var board = await _context.Boards.FindAsync(new object[] { boardId }, cancellationToken);
        if (board == null)
            return new Fail<Board>("Invalid board.");

        string currentSerialized = board.Grid;
        int currentGeneration = board.Generation;

        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        seen[currentSerialized] = currentGeneration;

        bool concluded = false;

        for (int i = 1; i <= steps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentGrid = Board.DeserializeGrid(currentSerialized);

            var nextGrid = Board.BuildNextGeneration(currentGrid);
            var nextSerialized = Board.SerializeGrid(nextGrid);

            currentGeneration++;

            if (IsEmptyGrid(nextSerialized) ||
                string.Equals(currentSerialized, nextSerialized, StringComparison.Ordinal) ||
                seen.ContainsKey(nextSerialized))
            {
                concluded = true;
                currentSerialized = nextSerialized;
            }

            seen[nextSerialized] = currentGeneration;
            currentSerialized = nextSerialized;
        }

        if (concluded)
        {
            board.Grid = currentSerialized;
            board.Generation = currentGeneration;
            board.LatestUpdateAt = _clockService.CurrentUtc;

            await _context.SaveChangesAsync(cancellationToken);
            return new Success<Board>(board);
        }

        return new Fail<Board>("No conclusion within steps.");
    }
}