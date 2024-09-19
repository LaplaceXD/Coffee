namespace ExpenseTrackerAPI.Dtos;

/// <summary>A data transfer object for a user.</summary>
public record UserDto
{
    /// <summary>The unique identifier of the user.</summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public required Guid Id { get; set; }

    /// <summary>The name of the user.</summary>
    /// <example>John Doe</example>
    public required string Name { get; set; }

    /// <summary>The email of the user.</summary>
    /// <example>test@example.com</example>
    public required string Email { get; set; }
}
