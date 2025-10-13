using System.Net;
using GameOfLife.Api.Dtos;
using GameOfLife.Services;
using GameOfLife.CrossCutting.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(typeof(CreateBoardResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Fail<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemHttpResult), StatusCodes.Status500InternalServerError)]
    public async Task<Results<
        Created<CreateBoardResultDto>,
        BadRequest<Fail<Guid>>,
        ProblemHttpResult
        >> Post(CreateBoardDto board, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.Create(board, cancellationToken);

        if (result is Success<Guid> suc) return TypedResults.Created(nameof(Get), new CreateBoardResultDto(suc.Data));

        if (result is Fail<Guid> fail) return TypedResults.BadRequest(fail);

        var error = (InternalError<Guid>)result;

        return TypedResults.Problem(statusCode: (int)HttpStatusCode.InternalServerError, detail: error.Error);
    }

    [HttpGet("{id}/next")]
    public async Task<Results<
        Ok<Result<NextBoardResultDto>>,
        BadRequest<Fail<NextBoardResultDto>>
        >> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.GetNextGeneration(id, cancellationToken);

        return TypedResults.Ok(result);
    }

    [HttpGet("{id}/start")]
    public async Task<Results<
        Accepted,
        BadRequest
        >> Start(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _gameOfLifeService.Start(id, cancellationToken);

        return TypedResults.Accepted(string.Empty);
    }

    [HttpGet("{id}/advance/{steps}")]
    public async Task<Results<
        Accepted,
        BadRequest<Fail>
        >> Advance(Guid id, int steps, CancellationToken cancellationToken = default)
    {
        await _gameOfLifeService.Advance(id, steps, cancellationToken);

        return TypedResults.Accepted(string.Empty);
    }

    [HttpGet("{id}/final")]
    public Results<
       Ok<Grid>,
       BadRequest<string>
       > Final(Guid id, int steps)
    {
        // Generate next generation grid logic
        return TypedResults.Ok(new Grid());
    }
}

public record Grid();

public record PostResult(Guid id);