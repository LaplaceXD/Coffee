using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTrackerAPI.Interfaces;
using ExpenseTrackerAPI.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTrackerAPI.Services;

/// <summary>A service for generating JWT tokens.</summary>
/// <param name="Options">The JWT options.</param>
public class JwtService(IOptions<JwtOptions> Options) : IJwtService
{
    /// <summary>Generate a token given an id.</summary>
    /// <param name="id">The id of an entity.</param>
    /// <returns>The generated token.</returns>
    public string GenerateToken(string id)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.Value.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [new(ClaimTypes.NameIdentifier, id)],
            expires: DateTime.Now.AddMinutes(Options.Value.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
