namespace GameOfLife.Api.Dtos;

public class CreateBoardDto
{
    public int[][] Grid { get; set; } = Array.Empty<int[]>();
}