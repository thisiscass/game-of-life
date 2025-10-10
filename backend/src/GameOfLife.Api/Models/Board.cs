namespace GameOfLife.Api.Models;

public class Board
{
    public Guid Id { get; set; }
    public string Grid { get; set; } = string.Empty;
    public int Generation { get; set; }

    public Guid GameId { get; set; }
    public virtual Game? Game { get; set; }

}