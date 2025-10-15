namespace GameOfLife.Api.Dtos;

public class CreateBoardDto
{
    public CreateBoardDto(int[][] grid)
    {
        if (grid == null)
            grid = Array.Empty<int[]>();

        Grid = grid;
    }
    
    public int[][] Grid { get; set; }
}