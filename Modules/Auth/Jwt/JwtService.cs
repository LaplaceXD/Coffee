using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using ExpenseTrackerAPI.Options;
using ExpenseTrackerAPI.Interfaces;

namespace ExpenseTrackerAPI.Services;

/// <summary>A service for generating JWT tokens.</summary>
public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _options = options.Value;

    /// <summary>Generate a token given an id.</summary>
    /// <param name="id">The id of an entity.</param>
    /// <returns>The generated token.</returns>
    public string GenerateToken(string id)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: [new(ClaimTypes.NameIdentifier, id)],
            expires: DateTime.Now.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
