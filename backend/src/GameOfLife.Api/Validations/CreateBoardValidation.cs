using GameOfLife.Api.Dtos;

namespace GameOfLife.Api.Validations;

public interface ICreateBoardValidation<T>
{
    public CreateBoardValidation PerformValidation(T obj);
}

public sealed class CreateBoardValidation : Validator<CreateBoardDto>, ICreateBoardValidation<CreateBoardDto>
{
    public CreateBoardValidation()
    {
        AddRule(dto =>
        {
            if (dto.Grid == null) return false;
            return dto.Grid.All(row => row != null && row.All(cell => cell == 0 || cell == 1));
        });
    }

    public CreateBoardValidation PerformValidation(CreateBoardDto obj)
    {
        Validate(obj);
        return this;
    }
}