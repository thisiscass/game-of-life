using GameOfLife.CrossCutting.Result;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GameOfLife.CrossCutting.Extensions;

public static class ResultHandlerExtensions
{
    public static Results<Ok<Success<T>>, BadRequest<Fail<T>>> ToHttpResult<T>(this IResult<T> result)
    {
        return result switch
        {
            Success<T> success => TypedResults.Ok(success),
            Fail<T> fail => TypedResults.BadRequest(new Fail<T>(fail.Errors)),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }

    public static Results<Ok, BadRequest<Fail>> ToHttpResult(this Result.IResult result)
    {
        return result switch
        {
            Success => TypedResults.Ok(),
            Fail fail => TypedResults.BadRequest(new Fail(fail.Errors)),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }

    public static Results<Accepted, BadRequest<Fail>> ToAcceptResult(this Result.IResult result, string routeName)
    {
        return result switch
        {
            Success => TypedResults.Accepted(routeName),
            Fail fail => TypedResults.BadRequest(new Fail(fail.Errors)),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }

    public static Results<Created<Success<T>>, BadRequest<Fail<T>>> ToCreatedResult<T>(this IResult<T> result, string routeName)
    {
        return result switch
        {
            Success<T> success => TypedResults.Created(routeName, success),
            Fail<T> fail => TypedResults.BadRequest(new Fail<T>(fail.Errors)),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }

    public static Results<Created, BadRequest<Fail>> ToCreatedResult(this Result.IResult result, string routeName)
    {
        return result switch
        {
            Success => TypedResults.Created(routeName),
            Fail fail => TypedResults.BadRequest(new Fail(fail.Errors)),
            _ => throw new InvalidOperationException("Unexpected result type.")
        };
    }
}