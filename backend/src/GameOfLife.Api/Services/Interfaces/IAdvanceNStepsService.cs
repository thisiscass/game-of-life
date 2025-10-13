using GameOfLife.Api.Models;
using GameOfLife.CrossCutting.Result;

namespace GameOfLife.Services;

public interface IAdvanceNStepsService
{
    public Task<IResult<Board>> GetFinalResultOrFail(Guid boardId, int steps, CancellationToken cancellationToken);
}