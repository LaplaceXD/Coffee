using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Models;

/// <summary>The user context.</summary>
public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
{
    /// <summary>The user entities.</summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>Configure the user model.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
