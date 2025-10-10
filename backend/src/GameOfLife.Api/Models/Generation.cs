namespace GameOfLife.Api.Models;

public class Generation
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Grid { get; set; } = string.Empty;
    public int GenerationNumber { get; set; }
    public virtual Board? Board { get; set; }

}