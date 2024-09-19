using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Interfaces;

/// <summary>An interface for an authentication service.</summary>
public interface IAuthService
{
    /// <summary>Get the currently authenticated user.</summary>
    /// <returns>The currently authenticated user.</returns>
    public Task<User?> GetUser();
}
