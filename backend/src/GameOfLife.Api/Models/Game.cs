namespace GameOfLife.Api.Models;

public class Game
{
    public Guid Id { get; set; }
    public virtual ICollection<Board>? Board { get; set; }

}