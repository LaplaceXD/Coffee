using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Models;

/// <summary>The user context.</summary>
public class UserContext : DbContext
{
    /// <summary>The user entities.</summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>Initialize the user context.</summary>
    /// <param name="options">The options for this context.</param>
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    /// <summary>Configure the user model.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.Owner)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
