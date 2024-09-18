namespace ExpenseTrackerAPI.Common;

/// <summary>A simple error response object</summary>
public record ErrorResponse
{
    /// <summary>The error message.</summary>
    public required string Message { get; init; }
}
