using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTrackerAPI.Models;

/// <summary>The type of a transaction.</summary>
public enum TransactionType
{
    /// <summary>An expense transaction.</summary>
    Expense,
    /// <summary>An income transaction.</summary>
    Income
}

/// <summary>A transaction model.</summary>
[Table("Transactions")]
public class Transaction
{
    /// <summary>The unique identifier of the transaction.</summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Key]
    public Guid Id { get; set; }

    /// <summary>The name of the transaction.</summary>
    /// <example>Lunch</example>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public required string Name { get; set; }

    /// <summary>The description of the transaction.</summary>
    /// <example>I had lechon for lunch.</example>
    [StringLength(4096, ErrorMessage = "Description must be less than or equal to 4096 characters.")]
    [Column(TypeName = "TEXT")]
    public string Description { get; set; } = string.Empty;

    /// <summary>The cost of the transaction in cents.</summary>
    /// <example>100</example>
    [Description("The cost of the transaction in cents.")]
    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public required int Amount { get; set; }

    /// <summary>The timestamp the transaction was created.</summary>
    /// <example>2022-03-01T00:00:00Z</example>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>The type of the transaction.</summary>
    /// <example>Expense</example>
    [Required(ErrorMessage = "Type is required.")]
    [EnumDataType(typeof(TransactionType))]
    public required TransactionType Type { get; set; }

    /// <summary>The user identifier of the transaction.</summary>
    /// <remarks>Foreign key to the user model.</remarks>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Required(ErrorMessage = "OwnerId is required.")]
    public Guid OwnerId { get; set; }

    /// <summary>The user of the transaction.</summary>
    /// <remarks>Navigation property to the user model.</remarks>
    [JsonIgnore]
    public User Owner { get; set; } = null!;
}

