using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Models;

/// <summary>The transaction context.</summary>
/// <param name="options">The options for this context.</param>
public class TransactionContext(DbContextOptions<TransactionContext> options) : DbContext(options)
{
    /// <summary>The transaction entities.</summary>
    public DbSet<Transaction> Transactions { get; set; } = null!;

    /// <summary>Configure the model.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .IsRequired();
    }
}

