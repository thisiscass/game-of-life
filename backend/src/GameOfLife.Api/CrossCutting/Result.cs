namespace GameOfLife.CrossCutting.Result;

public abstract class Result { }

public abstract class Result<T> : Result { }

public class Success : Result { }

public class Success<T> : Result<T>
{
    public Success(T data)
    {
        Data = data;
    }

    public T? Data { get; private set; }
}

public class Fail : Result
{
    public List<string> Errors { get; private set; } = new();
    public Fail(List<string> errors) => Errors = errors;

    public Fail(string error) => Errors.Add(error);
}

public class Fail<T> : Result<T>
{
    public List<string> Errors { get; private set; } = new();
    public Fail(List<string> errors) => Errors = errors;
    public Fail(string error) => Errors.Add(error);

}

public class InternalError<T> : Result<T>
{
    public string Error { get; private set; }
    public InternalError(string error) => Error = error;
}