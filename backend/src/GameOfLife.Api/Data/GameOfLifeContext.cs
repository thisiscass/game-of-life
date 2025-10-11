using GameOfLife.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Data;

public class GameOfLifeContext : DbContext
{
    public GameOfLifeContext(DbContextOptions<GameOfLifeContext> options)
    : base(options)
    { }

    public DbSet<Board>? Boards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Board>()
            .HasIndex(g => new { g.Id, g.Generation })
            .IsUnique();
    }
}