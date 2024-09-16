namespace ExpenseTrackerAPI.Models;

public enum TransactionType
{
    Expense,
    Income
}

public record Transaction
{
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the transaction.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the transaction.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The cost of the transaction in cents.
    /// </summary>
    public required ulong Amount { get; set; }

    /// <summary>
    /// The timestamp of the transaction.
    /// </summary>
    public required DateTime Timestamp { get; set; }

    /// <summary>
    /// The type of the transaction.
    /// </summary>
    public required TransactionType Type { get; set; }

    // Will add this later on once we have authentication in place,
    // for now transactions are global
    // public int UserId { get; set; }
}


public record TransactionDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required ulong Amount { get; set; }
    public required DateTime Timestamp { get; set; }
    public required TransactionType Type { get; set; }
}


