namespace ExpenseTrackerAPI.Auth;

/// <summary>An interface for a JWT service.</summary>
public interface IJwtService
{
    /// <summary>Generate a token given an id.</summary>
    /// <param name="id">The id of an entity.</param>
    /// <returns>The generated token.</returns>
    public string GenerateToken(string id);
}
