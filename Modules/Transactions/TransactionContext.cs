using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Models;

/// <summary>The transaction context.</summary>
public class TransactionContext : DbContext
{
    /// <summary>The transaction entities.</summary>
    public DbSet<Transaction> Transactions { get; set; } = null!;

    /// <summary>Initialize the transaction context.</summary>
    /// <param name="options">The options for this context.</param>
    public TransactionContext(DbContextOptions<TransactionContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    /// <summary>Configure the model.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Owner)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.OwnerId)
            .IsRequired();
    }
}

