using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RadekClickerServer;

public class Player
{
    [Key] [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? DisplayName { get; set; }
    public float Radeks { get; set; }
}

public sealed class PlayerDb : DbContext
{
    public PlayerDb(DbContextOptions opts) : base(opts)
    {
        Database.EnsureCreated();
    }
    public DbSet<Player> Players { get; set; } = null!;
}