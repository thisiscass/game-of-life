namespace GameOfLife.CrossCutting.Result;

public interface IResult { }

public interface IResult<T> : IResult { }

public class Success : IResult { }

public class Success<T> : IResult<T>
{
    public Success(T data)
    {
        Data = data;
    }

    public T? Data { get; private set; }
}

public class Fail : IResult
{
    public List<string> Errors { get; private set; } = new();
    public Fail(List<string> errors) => Errors = errors;

    public Fail(string error) => Errors.Add(error);
}

public class Fail<T> : IResult<T>
{
    public List<string> Errors { get; private set; } = new();
    public Fail(List<string> errors) => Errors = errors;
    public Fail(string error) => Errors.Add(error);

}

public class InternalError<T> : IResult<T>
{
    public string Error { get; private set; }
    public InternalError(string error) => Error = error;
}