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
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the transaction.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// The description of the transaction.
    /// </summary>
    [StringLength(4096, ErrorMessage = "Description must be less than or equal to 4096 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The cost of the transaction in cents.
    /// </summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public required int Amount { get; set; }

    /// <summary>
    /// The timestamp of the transaction.
    /// </summary>
    [Timestamp]
    [Required(ErrorMessage = "Timestamp is required.")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The type of the transaction.
    /// </summary>
    [Required(ErrorMessage = "Type is required.")]
    [EnumDataType(typeof(TransactionType))]
    public required TransactionType Type { get; set; }
}

public record TransactionDto
{
    /// <summary>
    /// The name of the transaction.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// The description of the transaction.
    /// </summary>
    [StringLength(4096, ErrorMessage = "Description must be less than or equal to 4096 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The cost of the transaction in cents.
    /// </summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public required int Amount { get; set; }

    /// <summary>
    /// The type of the transaction.
    /// </summary>
    [Required(ErrorMessage = "Type is required.")]
    [EnumDataType(typeof(TransactionType), ErrorMessage = "Type must be either 'Expense' or 'Income'.")]
    public required string Type { get; set; }
}


