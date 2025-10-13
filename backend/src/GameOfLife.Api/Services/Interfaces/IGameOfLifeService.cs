using GameOfLife.CrossCutting.Result;
using GameOfLife.Api.Dtos;

namespace GameOfLife.Services;

public interface IGameOfLifeService
{
    Task<IResult<CreateBoardResultDto>> Create(CreateBoardDto board, CancellationToken cancellationToken);
    Task<IResult<NextBoardResultDto>> GetNextGeneration(Guid boardId, CancellationToken cancellationToken);
    Task<CrossCutting.Result.IResult> Advance(Guid boardId, int steps, CancellationToken cancellationToken);
    Task<CrossCutting.Result.IResult> Start(Guid boardId, CancellationToken cancellationToken);
    Task<CrossCutting.Result.IResult> CleanRunningBoards(CancellationToken cancellationToken);
}