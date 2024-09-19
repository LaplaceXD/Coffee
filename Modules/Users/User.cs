using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BC = BCrypt.Net.BCrypt; // Class and namespace have the same name, so we use an alias, or else it will conflict.

namespace ExpenseTrackerAPI.Models;

/// <summary>A user model.</summary>
public class User
{
    /// <summary>The unique identifier of the user.</summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>The name of the user.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(1024, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public required string Name { get; set; }

    /// <summary>The email of the user.</summary>
    /// <remarks>Must be a valid email address.</remarks>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public required string Email { get; set; }

    private string _password = string.Empty;
    /// <summary>The password of the user.</summary>
    [JsonIgnore]
    public string Password
    {
        get => throw new NotSupportedException("Passwords should never be read.");
        set => _password = BC.EnhancedHashPassword(value);
    }

    /// <summary>The transactions of the user.</summary>
    [JsonIgnore]
    public ICollection<Transaction> Transactions { get; set; } = [];

    /// <summary>Verify a password.</summary>
    /// <param name="password">The password to verify.</param>
    /// <returns>True if the password is correct, otherwise false.</returns>
    public bool VerifyPassword(string password)
    {
        return BC.EnhancedVerify(password, _password);
    }
}
