using ExpenseTrackerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Common;

/// <summary>The application database context.</summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>The transaction entities.</summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>The user entities.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Initialize the application database context.</summary>
    /// <param name="options">The options for this context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    /// <summary>Configure the model for the database context.</summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Transaction>()
            .HasOne(t => t.Owner)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.OwnerId)
            .IsRequired();

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder
            .Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.Owner)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
