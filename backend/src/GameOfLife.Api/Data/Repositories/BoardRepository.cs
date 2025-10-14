using GameOfLife.Api.Data;
using GameOfLife.Models;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly GameOfLifeContext _context;
    public BoardRepository(GameOfLifeContext context)
    {
        _context = context;
    }

    public async Task Add(Board board, CancellationToken cancellationToken = default)
    {
        _context.Boards!.Add(board);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Board?> GetById(Guid id)
    {
        return await _context.Boards.FindAsync(id);
    }

    public async Task<IEnumerable<Board>> GetRunning(CancellationToken cancellationToken = default)
    {
        return await _context.Boards!.Where(b => b.IsRunning).ToListAsync(cancellationToken);
    }

    public async Task Update(Board board, CancellationToken cancellationToken = default)
    {
        _context.Boards.Update(board);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(IEnumerable<Board> boards, CancellationToken cancellationToken = default)
    {
        _context.Boards.UpdateRange(boards);
        await _context.SaveChangesAsync(cancellationToken);
    }
}