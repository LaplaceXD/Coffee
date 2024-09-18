using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Models;

/// <summary>The transaction context.</summary>
public class TransactionContext : DbContext
{
    /// <summary>Initializes a new instance of the <see cref="TransactionContext"/> class.</summary>
    /// <param name="options">The options for this context.</param>
    public TransactionContext(DbContextOptions<TransactionContext> options) : base(options) { }

    /// <summary>The transaction entities.</summary>
    public DbSet<Transaction> Transactions { get; set; } = null!;
}

