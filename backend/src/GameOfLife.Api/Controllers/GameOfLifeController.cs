using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.APi.Controllers;

[ApiController]
[Route("api/grid")]
public class GameOfLifeController
{
    [HttpPost]
    public Results<
        Created<PostResult>, 
        BadRequest<string>
        > Post(Grid grid)
    {
        return TypedResults.Created(string.Empty, new PostResult(Guid.Empty));
    }

    [HttpGet("{id}/next")]
    public Results<
        Ok<Grid>,
        BadRequest<string>
        > Get(Guid id)
    {
        // Generate next generation grid logic
        return TypedResults.Ok(new Grid());
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