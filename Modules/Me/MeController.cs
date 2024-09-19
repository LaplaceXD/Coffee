using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing the currently authenticated user.</summary>
/// <param name="logger">The logger.</param>
/// <param name="authService">The authentication service.</param>
/// <param name="transactionContext">The transaction context.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MeController(ILogger<MeController> logger, IAuthService authService, TransactionContext transactionContext) : ControllerBase
{
    private readonly ILogger<MeController> _logger = logger;
    private readonly IAuthService _authService = authService;
    private readonly TransactionContext _transactionContext = transactionContext;

    /// <summary>Get the currently authenticated user.</summary>
    /// <returns>The currently authenticated user.</returns>
    ///
    /// <response code="200">The currently authenticated user.</response>
    /// <response code="401">The user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<Results<UnauthorizedHttpResult, Ok<User>>> Get()
    {
        _logger.LogInformation("Getting the currently authenticated user.");
        var user = await _authService.GetUser();
        if (user is null)
        {
            _logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        _logger.LogInformation("Returning the currently authenticated user.");
        return TypedResults.Ok(user);
    }

    /// <summary>Get the transactions of the currently authenticated user.</summary>
    /// <param name="type">The type of transactions to filter by.</param>
    /// <returns>The transactions of the currently authenticated user.</returns>
    ///
    /// <response code="200">The transactions of the currently authenticated user.</response>
    /// <response code="400">Invalid transaction type.</response>
    /// <response code="401">The user is not authenticated.</response>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<Results<BadRequest, UnauthorizedHttpResult, Ok<IEnumerable<Transaction>>>> GetTransactions([FromQuery] string? type)
    {
        var user = await _authService.GetUser();
        if (user is null)
        {
            _logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        var transactionsQuery = _transactionContext.Transactions
            .Where(t => t.UserId == user.Id);

        if (type is not null)
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                _logger.LogWarning("Invalid transaction type: {}.", type);
                return TypedResults.BadRequest();
            }

            _logger.LogInformation("Filtering transactions by type: {}.", transactionType);
            transactionsQuery = transactionsQuery.Where(t => t.Type == transactionType);
        }

        _logger.LogInformation("Retrieving transactions for user {}.", user.Id);
        var transactions = await transactionsQuery
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();

        _logger.LogInformation("Successfully retrieved {} transactions.", transactions.Count);
        return TypedResults.Ok<IEnumerable<Transaction>>(transactions);
    }
}
