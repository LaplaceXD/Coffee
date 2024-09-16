using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerAPI.Models;

public enum TransactionType
{
    Expense,
    Income
}

public record Transaction
{
    /// <summary>
    /// The unique identifier of the transaction.
    /// <summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The name of the transaction.
    /// </summary>
    [Required]
    [Length(1, 255, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// The description of the transaction.
    /// </summary>
    [MaxLength(4096, ErrorMessage = "Description must be less than or equal to 4096 characters.")]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// The cost of the transaction in cents.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public required int Amount { get; set; }

    /// <summary>
    /// The timestamp of the transaction.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The type of the transaction.
    /// </summary>
    [Required]
    [EnumDataType(typeof(TransactionType))]
    public required TransactionType Type { get; set; }

    // Will add this later on once we have authentication in place,
    // for now transactions are global
    // public int UserId { get; set; }
}

public record TransactionDto
{
    /// <summary>
    /// The name of the transaction.
    /// </summary>
    [Required]
    [Length(1, 255, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// The description of the transaction.
    /// </summary>
    [MaxLength(4096, ErrorMessage = "Description must be less than or equal to 4096 characters.")]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// The cost of the transaction in cents.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public required int Amount { get; set; }

    /// <summary>
    /// The type of the transaction.
    /// </summary>
    [Required]
    [EnumDataType(typeof(TransactionType), ErrorMessage = "Type must be either 'Expense' or 'Income'.")]
    public required TransactionType Type { get; set; }
}


