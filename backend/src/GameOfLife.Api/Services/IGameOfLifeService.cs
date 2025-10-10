using GameOfLife.Api.Models;

namespace GameOfLife.Api.Services;

public interface IGameOfLifeService
{
    Task<Guid> Create(Board board);
    Task<Board> GetNextGeneration(Guid boardId);
    Task<Board> GetAfterNSteps(int n);
}