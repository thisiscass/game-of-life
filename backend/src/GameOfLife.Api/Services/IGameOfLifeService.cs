using GameOfLife.CrossCutting.Result;
using GameOfLife.Api.Dtos;
using GameOfLife.Api.Models;

namespace GameOfLife.Api.Services;

public interface IGameOfLifeService
{
    Task<Result<Guid>> Create(CreateBoardDto board);
    Task<Result<NextBoardResultDto>> GetNextGeneration(Guid boardId);
    Task<Result<Board>> GetAfterNSteps(int n);
}