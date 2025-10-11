using System.Net;
using GameOfLife.Api.Dtos;
using GameOfLife.Api.Services;
using GameOfLife.CrossCutting.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.APi.Controllers;

public record NextGenerationResult(Guid boardId);

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
        >> Post(CreateBoardDto board)
    {
        var result = await _gameOfLifeService.Create(board);

        if (result is Success<Guid> suc) return TypedResults.Created(nameof(Get), new CreateBoardResultDto(suc.Data));

        if (result is Fail<Guid> fail) return TypedResults.BadRequest(fail);

        var error = (InternalError<Guid>)result;

        return TypedResults.Problem(statusCode: (int)HttpStatusCode.InternalServerError, detail: error.Error);
    }

    [HttpGet("{id}/next")]
    public async Task<Results<
        Ok<Result<NextBoardResultDto>>,
        BadRequest<string>
        >> Get(Guid id)
    {
        // Generate next generation grid logic
        var result = await _gameOfLifeService.GetNextGeneration(id);

        return TypedResults.Ok(result);
    }

    [HttpGet("{id}/advance/{steps}")]
    public Results<
        Ok<Grid>,
        BadRequest<string>
        > Advance(Guid id, int steps)
    {
        // Generate next generation grid logic
        return TypedResults.Ok(new Grid());
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