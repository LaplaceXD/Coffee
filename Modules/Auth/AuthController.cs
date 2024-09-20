using ExpenseTrackerAPI.Common;
using ExpenseTrackerAPI.Dtos;
using ExpenseTrackerAPI.Interfaces;
using ExpenseTrackerAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing authentication.</summary>
/// <param name="dbContext">The database context.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/[controller]")]
public class AuthController(ApplicationDbContext dbContext, ILogger<AuthController> logger)
    : ControllerBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<AuthController> _logger = logger;

    /// <summary>Login a user.</summary>
    /// <param name="userLoginDto">The user login data.</param>
    /// <param name="jwtService">The JWT service for working with user token.</param>
    /// <returns>The logged in user.</returns>
    ///
    /// <response code="200">The user was logged in successfully.</response>
    /// <response code="400">Invalid user credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<Results<BadRequest<ErrorResponse>, Ok<UserTokenDto>>> Login(
        UserLoginDto userLoginDto,
        [FromServices] IJwtService jwtService
    )
    {
        _logger.LogInformation("Logging in user {}.", userLoginDto.Email);

        var user = await _dbContext
            .Users.Where(u => u.Email == userLoginDto.Email)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            _logger.LogInformation("User {} not found.", userLoginDto.Email);
            return TypedResults.BadRequest(
                new ErrorResponse { Message = "Invalid user credentials." }
            );
        }

        if (!user.VerifyPassword(userLoginDto.Password))
        {
            _logger.LogInformation("User {} provided an incorrect password.", userLoginDto.Email);
            return TypedResults.BadRequest(
                new ErrorResponse { Message = "Invalid user credentials." }
            );
        }

        _logger.LogInformation("Generating access token of user {}...", user.Id);
        var token = jwtService.GenerateToken(user.Id.ToString());

        _logger.LogInformation("User {} logged in.", user.Id);
        return TypedResults.Ok(new UserTokenDto { Token = token });
    }

    /// <summary>Register a user.</summary>
    /// <returns>The registered user.</returns>
    /// <param name="userRegisterDto">The user registration data.</param>
    ///
    /// <response code="200">The user was successfully registered.</response>
    /// <response code="400">The data passed was invalid.</response>
    /// <response code="409">The data passed has conflicting values.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<Results<BadRequest, Conflict<ErrorResponse>, Ok<User>>> Register(
        UserRegisterDto userRegisterDto
    )
    {
        _logger.LogInformation("Registering user {}.", userRegisterDto.Email);

        var existingUser = await _dbContext
            .Users.Where(u => u.Email == userRegisterDto.Email)
            .FirstOrDefaultAsync();

        if (existingUser is not null)
        {
            _logger.LogInformation(
                "User with the same email {} already exists.",
                userRegisterDto.Email
            );
            return TypedResults.Conflict(
                new ErrorResponse { Message = "Email is already in-use." }
            );
        }

        var user = new User
        {
            Name = userRegisterDto.Name,
            Email = userRegisterDto.Email,
            Password = userRegisterDto.Password,
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User {} registered.", user.Id);
        return TypedResults.Ok(user);
    }
}
