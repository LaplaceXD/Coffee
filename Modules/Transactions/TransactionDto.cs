using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerAPI.Models;

/// <summary>A data transfer object for a transaction.</summary>
public record TransactionDto
{
    /// <summary>The name of the transaction.</summary>
    /// <example>Lunch</example>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public required string Name { get; set; }

    /// <summary>The description of the transaction.</summary>
    /// <example>I had lechon for lunch.</example>
    [StringLength(4096, ErrorMessage = "Description must be less than or equal to 4096 characters.")]
    public string Description { get; set; } = string.Empty;

    /// <summary>The cost of the transaction in cents.</summary>
    /// <example>100</example>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public required int Amount { get; set; }

    /// <summary>The type of the transaction.</summary>
    /// <example>Expense</example>
    [Required(ErrorMessage = "Type is required.")]
    [EnumDataType(typeof(TransactionType), ErrorMessage = "Type must be either 'Expense' or 'Income'.")]
    public required string Type { get; set; }
}
