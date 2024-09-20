using System.ComponentModel.DataAnnotations;

namespace ExpenseTrackerAPI.Dtos;

/// <summary>A data transfer object for registering a user.</summary>
public record UserRegisterDto
{
    /// <summary>The name of the user.</summary>
    /// <example>John Doe</example>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(
        1024,
        MinimumLength = 1,
        ErrorMessage = "Name must be between 1 and 255 characters."
    )]
    public required string Name { get; set; }

    /// <summary>The email of the user.</summary>
    /// <remarks>Must be a valid email address.</remarks>
    /// <example>test@example.com</example>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public required string Email { get; set; }

    /// <summary>The password of the user.</summary>
    /// <example>MyP@ssw0rd</example>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(
        32,
        MinimumLength = 8,
        ErrorMessage = "Password must be between 8 and 32 characters."
    )]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character (@$!%*?&)."
    )]
    public required string Password { get; set; }
}

/// <summary>A data transfer object for logging in a user.</summary>
public record UserLoginDto
{
    /// <summary>The email of the user.</summary>
    /// <example>test@example.com</example>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public required string Email { get; set; }

    /// <summary>The password of the user.</summary>
    /// <example>MyP@ssw0rd</example>
    [Required]
    public required string Password { get; set; }
}

/// <summary>A data transfer object for a user token.</summary>
public record UserTokenDto
{
    /// <summary>The token of the user.</summary>
    /// <example>eyJH....</example>
    public required string Token { get; set; }
}
