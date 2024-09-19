using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

using ExpenseTrackerAPI.Dtos;
using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Interfaces;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing the currently authenticated user.</summary>
/// <param name="userContext">The user context.</param>
/// <param name="logger">The logger.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MeController(UserContext userContext, ILogger<MeController> logger) : ControllerBase
{
    private readonly UserContext _userContext = userContext;
    private readonly ILogger<MeController> _logger = logger;

    /// <summary>Get the currently authenticated user.</summary>
    /// <param name="authService">The authentication service.</param>
    /// <returns>The currently authenticated user.</returns>
    ///
    /// <response code="200">The currently authenticated user.</response>
    /// <response code="401">The user is not authenticated.</response>
    [HttpGet]
    public async Task<Results<UnauthorizedHttpResult, Ok<UserDto>>> Get([FromServices] IAuthService authService)
    {
        var user = await authService.GetUser();
        if (user is null) return TypedResults.Unauthorized();

        return TypedResults.Ok(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
    }
}
