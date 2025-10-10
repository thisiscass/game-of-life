using GameOfLife.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Data;

public class GameOfLifeContext : DbContext
{
    public GameOfLifeContext(DbContextOptions<GameOfLifeContext> options)
    : base(options)
    { }

    public DbSet<Board>? Boards { get; set; }
    public DbSet<Generation>? Generations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<Board>()
            .HasMany(b => b.Generations)
            .WithOne(g => g.Board)
            .HasForeignKey(g => g.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Generation>()
            .HasIndex(g => new { g.BoardId, g.GenerationNumber })
            .IsUnique();
    }
}