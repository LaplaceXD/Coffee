namespace ExpenseTrackerAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Common;

/// <summary>Controller for managing authentication.</summary>
/// <param name="userContext">The user context.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/[controller]")]
public class AuthController(UserContext userContext, ILogger<AuthController> logger) : ControllerBase
{
    private readonly UserContext _userContext = userContext;
    private readonly ILogger<AuthController> _logger = logger;

    /// <summary>Login a user.</summary>
    /// <param name="userLoginDto">The user login data.</param>
    /// <returns>The logged in user.</returns>
    ///
    /// <response code="200">The user was logged in successfully.</response>
    /// <response code="400">Invalid user credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Results<BadRequest<ErrorResponse>, Ok<UserTokenDto>>> Login(UserLoginDto userLoginDto)
    {
        _logger.LogInformation("Logging in user {}.", userLoginDto.Email);

        var user = await _userContext.Users
            .Where(u => u.Email == userLoginDto.Email)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            _logger.LogInformation("User {} not found.", userLoginDto.Email);
            return TypedResults.BadRequest(new ErrorResponse { Message = "Invalid user credentials." });
        }

        if (!user.VerifyPassword(userLoginDto.Password))
        {
            _logger.LogInformation("User {} provided an incorrect password.", userLoginDto.Email);
            return TypedResults.BadRequest(new ErrorResponse { Message = "Invalid user credentials." });
        }

        _logger.LogInformation("User {} logged in.", user.Id);
        return TypedResults.Ok(new UserTokenDto { Token = "TODO" });
    }

    /// <summary>Register a user.</summary>
    /// <param name="userRegisterDto">The user registration data.</param>
    /// <returns>The registered user.</returns>
    ///
    /// <response code="200">The user was successfully registered.</response>
    /// <response code="400">The data passed was invalid.</response>
    /// <response code="409">The user already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<Results<BadRequest, Conflict<ErrorResponse>, Ok<UserDto>>> Register(UserRegisterDto userRegisterDto)
    {
        _logger.LogInformation("Registering user {}.", userRegisterDto.Email);

        var existingUser = await _userContext.Users
            .Where(u => u.Email == userRegisterDto.Email)
            .FirstOrDefaultAsync();

        if (existingUser is not null)
        {
            _logger.LogInformation("User {} already exists.", userRegisterDto.Email);
            return TypedResults.Conflict(new ErrorResponse { Message = "User already exists." });
        }

        var user = new User
        {
            Name = userRegisterDto.Name,
            Email = userRegisterDto.Email,
            Password = userRegisterDto.Password
        };

        await _userContext.Users.AddAsync(user);
        await _userContext.SaveChangesAsync();

        _logger.LogInformation("User {} registered.", user.Id);
        return TypedResults.Ok(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
    }
}
