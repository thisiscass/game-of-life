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
              // Normalize null grid and inner rows
              dto.Grid ??= Array.Empty<int[]>();
              dto.Grid = dto.Grid
                  .Select(row => row ?? Array.Empty<int>())
                  .ToArray();

              // Validate only 0 or 1 values
              return dto.Grid.All(row => row.All(cell => cell == 0 || cell == 1));
          }, "Invalid board - it accepts only 0 or 1.");

        AddRule(dto => dto.Grid.Length > 0 && dto.Grid.Length <= 20, "Invalid board size.");
        AddRule(dto => dto.Grid.FirstOrDefault()?.Length <= 20, "Invalid board size.");
    }

    public CreateBoardValidation PerformValidation(CreateBoardDto obj)
    {
        Validate(obj);
        return this;
    }
}