using GameOfLife.CrossCutting.Result;
using GameOfLife.Api.Dtos;
using GameOfLife.Api.Models;

namespace GameOfLife.Services;

public interface IGameOfLifeService
{
    Task<Result<Guid>> Create(CreateBoardDto board, CancellationToken cancellationToken);
    Task<Result<NextBoardResultDto>> GetNextGeneration(Guid boardId, CancellationToken cancellationToken);
    Task<Result> Advance(Guid boardId, int steps, CancellationToken cancellationToken);
    Task<Result> Start(Guid boardId, CancellationToken cancellationToken);
}