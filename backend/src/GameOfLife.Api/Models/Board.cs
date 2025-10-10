namespace GameOfLife.Api.Models;

public class Board
{
    public Board(
        Guid id,
        string grid,
        DateTime createdAt)
    {
        Id = id;
        Grid = grid;
        CreatedAt = createdAt;
    }

    public Guid Id { get; set; }
    public string Grid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<Generation>? Generations { get; set; }

    public static string TransformGrid(int[][] grid)
    {
        if (!(grid.Length > 0)) throw new ArgumentException("Grid cannot be empty.");

        var rows = grid.Select(row => string.Join(",", row));
        return string.Join(";", rows);
    }
}