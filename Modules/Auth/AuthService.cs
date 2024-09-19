using System.Security.Claims;
using ExpenseTrackerAPI.Interfaces;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Services;

/// <summary>A service for authentication.</summary>
public class AuthService(IHttpContextAccessor httpContextAccessor, UserContext userContext) : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly UserContext _userContext = userContext;

    /// <summary>Get the currently authenticated user.</summary>
    /// <returns>The currently authenticated user.</returns>
    /// <remarks>Returns null if the user is not authenticated.</remarks>
    public async Task<User?> GetUser()
    {
        var stringUserId = _httpContextAccessor?
            .HttpContext?
            .User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier)
            .Value;

        if (stringUserId is null || !Guid.TryParse(stringUserId, out var userId)) return null;

        return await _userContext.Users.FindAsync(userId);
    }
}
