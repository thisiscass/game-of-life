using GameOfLife.Api.Dtos;

namespace GameOfLife.Api.Validations;

public interface ICreateBoardValidation<T>
{
    public CreateBoardValidation PerformValidation(T obj);
}

public class CreateBoardValidation : Validator<CreateBoardDto>, ICreateBoardValidation<CreateBoardDto>
{
    public CreateBoardValidation()
    {
        AddRule((dto) => dto.Grid.Length > 0, "Invalid grid");
    }

    public CreateBoardValidation PerformValidation(CreateBoardDto obj)
    {
        Validate(obj);
        return this;
    }
}