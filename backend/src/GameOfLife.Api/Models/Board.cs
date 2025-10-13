using System.Security.Cryptography;

namespace GameOfLife.Api.Models;

public class Board
{
    public Board(
        Guid id,
        string grid,
        DateTime latestUpdateAt)
    {
        Id = id;
        Grid = grid;
        LatestUpdateAt = latestUpdateAt;
    }

    public Guid Id { get; set; }
    public string Grid { get; set; } = string.Empty;
    public DateTime LatestUpdateAt { get; set; }
    public int Generation { get; set; } = 0;
    public bool IsRunning { get; set; } = false;

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void NextGeneration(DateTime updatedAt)
    {
        var grid = DeserializeGrid(Grid);
        var nextGrid = BuildNextGeneration(grid);

        Grid = SerializeGrid(nextGrid);
        LatestUpdateAt = updatedAt;
        Generation += 1;
    }

    public static string SerializeGrid(int[][] grid)
    {
        return string.Join(";", grid.Select(row => string.Join(",", row)));

    }

    public static int[][] DeserializeGrid(string serialized)
    {
        return serialized
            .Split(';')
            .Select(row => row.Split(',').Select(int.Parse).ToArray())
            .ToArray();
    }

    public static int[][] BuildNextGeneration(int[][] grid)
    {
        if (grid == null || grid.Length == 0 || grid[0].Length == 0)
            throw new ArgumentException("Grid cannot be null or empty.");

        int rows = grid.Length;
        int cols = grid[0].Length;

        var newGrid = new int[rows][];
        for (int i = 0; i < rows; i++)
            newGrid[i] = new int[cols];

        var navigation = new (int dx, int dy)[]
        {
            (0, 1),   // right
            (1, 1),   // bottom-right
            (1, 0),   // bottom
            (1, -1),  // bottom-left
            (0, -1),  // left
            (-1, -1), // top-left
            (-1, 0),  // top
            (-1, 1)   // top-right
        };

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int neighbours = 0;

                foreach (var (dx, dy) in navigation)
                {
                    int nx = i + dx;
                    int ny = j + dy;

                    if (nx >= 0 && nx < rows && ny >= 0 && ny < cols)
                    {
                        if (grid[nx][ny] == 1)
                            neighbours++;
                    }
                }

                int square = grid[i][j];

                if (square == 0 && neighbours == 3)
                {
                    newGrid[i][j] = 1;
                }
                else if (square == 1 && (neighbours == 2 || neighbours == 3))
                {
                    newGrid[i][j] = 1;
                }
                else
                {
                    newGrid[i][j] = 0;
                }
            }
        }

        return newGrid;
    }


}