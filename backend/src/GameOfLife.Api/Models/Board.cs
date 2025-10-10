namespace GameOfLife.Api.Models;

public class Board
{
    public Guid Id { get; set; }
    public string Grid { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int Cols { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Generation>? Generations { get; set; }

}