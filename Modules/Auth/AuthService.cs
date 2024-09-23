using System.Security.Claims;
using ExpenseTrackerAPI.Common;
using ExpenseTrackerAPI.Interfaces;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Services;

/// <summary>A service for authentication.</summary>
/// <param name="HttpContextAccessor">The HTTP context accessor.</param>
/// <param name="Context">The database context.</param>
public class AuthService(IHttpContextAccessor HttpContextAccessor, ApplicationDbContext Context)
    : IAuthService
{
    /// <summary>Get the currently authenticated user.</summary>
    /// <returns>The currently authenticated user.</returns>
    /// <remarks>Returns null if the user is not authenticated.</remarks>
    public async Task<User?> GetUser()
    {
        var stringUserId = HttpContextAccessor
            ?.HttpContext?.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier)
            .Value;

        if (stringUserId is null || !Guid.TryParse(stringUserId, out var userId))
            return null;

        return await Context.Users.FindAsync(userId);
    }
}
