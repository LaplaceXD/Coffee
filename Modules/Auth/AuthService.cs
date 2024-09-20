using System.Security.Claims;
using ExpenseTrackerAPI.Common;
using ExpenseTrackerAPI.Interfaces;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Services;

/// <summary>A service for authentication.</summary>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
/// <param name="dbContext">The database context.</param>
public class AuthService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
    : IAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ApplicationDbContext _dbContext = dbContext;

    /// <summary>Get the currently authenticated user.</summary>
    /// <returns>The currently authenticated user.</returns>
    /// <remarks>Returns null if the user is not authenticated.</remarks>
    public async Task<User?> GetUser()
    {
        var stringUserId = _httpContextAccessor
            ?.HttpContext?.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier)
            .Value;

        if (stringUserId is null || !Guid.TryParse(stringUserId, out var userId))
            return null;

        return await _dbContext.Users.FindAsync(userId);
    }
}
