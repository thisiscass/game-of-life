using GameOfLife.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Data;

public class GameOfLifeContext : DbContext
{
    public GameOfLifeContext(DbContextOptions<GameOfLifeContext> options)
    : base(options)
    { }

    public DbSet<Game>? Games { get; set; }
    public DbSet<Board>? Boards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>()
            .HasKey(g => g.Id);

        modelBuilder.Entity<Board>()
            .HasKey(b => new { b.Id, b.Generation });
    }
}