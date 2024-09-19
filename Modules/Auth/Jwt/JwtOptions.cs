namespace ExpenseTrackerAPI.Options;

/// <summary>The options for the JWT.</summary>
public record JwtOptions
{
    /// <summary>The section name for the options.</summary>
    public static readonly string Section = "Jwt";

    /// <summary>The secret key for the JWT.</summary>
    public required string Secret { get; set; }

    /// <summary>The expiry time in minutes for the JWT.</summary>
    public required int ExpiryMinutes { get; set; }
}
