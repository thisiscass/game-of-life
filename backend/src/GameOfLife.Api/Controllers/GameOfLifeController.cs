using GameOfLife.Api.Dtos;
using GameOfLife.Services;
using GameOfLife.CrossCutting.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using GameOfLife.CrossCutting.Extensions;

namespace GameOfLife.APi.Controllers;

[ApiController]
[Route("api/board")]
public class GameOfLifeController
{
    private readonly IGameOfLifeService _gameOfLifeService;
    public GameOfLifeController(IGameOfLifeService gameOfLifeService)
    {
        _gameOfLifeService = gameOfLifeService;
    }

    [HttpPost]
    public async Task<Results<
        Created<Success<CreateBoardResultDto>>,
        BadRequest<Fail<CreateBoardResultDto>>
        >> Post(CreateBoardDto board, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.Create(board, cancellationToken);

        return result.ToCreatedResult(nameof(Get));
    }

    [HttpGet("{id}/next")]
    public async Task<Results<
        Ok<Success<NextBoardResultDto>>,
        BadRequest<Fail<NextBoardResultDto>>
        >> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.GetNextGeneration(id, cancellationToken);

        return result.ToHttpResult();
    }

    [HttpPost("{id}/start")]
    public async Task<Results<
        Accepted,
        BadRequest<Fail>
        >> Start(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.Start(id, cancellationToken);

        return result.ToAcceptResult();
    }

    [HttpPost("{id}/advance/{steps}")]
    public async Task<Results<
        Accepted,
        BadRequest<Fail>
        >> Advance(Guid id, int steps, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.Advance(id, steps, cancellationToken);

        return result.ToAcceptResult();
    }

    [HttpPost("{id}/stop")]
    public async Task<Results<
        Accepted,
        BadRequest<Fail>
        >> Stop(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.Stop(id, cancellationToken);

        return result.ToAcceptResult();
    }
}