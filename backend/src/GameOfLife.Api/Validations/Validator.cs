namespace GameOfLife.Api.Validations;

public class Validator<T>
{
    private List<(Func<T, bool> func, string message)> _rules = new();
    private List<string> _errors = new();

    public bool IsValid => !_errors.Any();

    protected void AddRule(Func<T, bool> func, string message = "")
    {
        _rules.Add((func, message));
    }

    protected void Validate(T obj)
    {
        foreach (var rule in _rules)
        {
            if (!rule.func.Invoke(obj)) _errors.Add(rule.message);
        }
    }

    public List<string> GetErrors() => _errors;
}