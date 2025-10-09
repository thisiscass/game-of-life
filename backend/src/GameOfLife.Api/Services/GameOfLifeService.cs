using GameOfLife.Api.Models;

namespace GameOfLife.Api.Services;

public class GameOfLifeService : IGameOfLifeService
{
    public Task<Guid> Create(Board board)
    {
        throw new NotImplementedException();
    }

    public Task<Board> GetAfterNSteps(int n)
    {
        throw new NotImplementedException();
    }

    public Task<Board> GetNextGeneration(Guid boardId)
    {
        throw new NotImplementedException();
    }
}