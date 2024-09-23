using ExpenseTrackerAPI.Common;
using ExpenseTrackerAPI.Interfaces;
using ExpenseTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing the currently authenticated user.</summary>
/// <param name="Logger">The logger.</param>
/// <param name="AuthService">The authentication service.</param>
/// <param name="Context">The database context.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MeController(
    ILogger<MeController> Logger,
    IAuthService AuthService,
    ApplicationDbContext Context
) : ControllerBase
{
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
        Logger.LogInformation("Getting the currently authenticated user.");
        var user = await AuthService.GetUser();
        if (user is null)
        {
            Logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        Logger.LogInformation("Returning the currently authenticated user.");
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
    public async Task<
        Results<BadRequest, UnauthorizedHttpResult, Ok<IEnumerable<Transaction>>>
    > GetTransactions([FromQuery] string? type)
    {
        var user = await AuthService.GetUser();
        if (user is null)
        {
            Logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        var transactionsQuery = Context.Transactions.Where(t => t.OwnerId == user.Id);

        if (type is not null)
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                Logger.LogWarning("Invalid transaction type: {}.", type);
                return TypedResults.BadRequest();
            }

            Logger.LogInformation("Filtering transactions by type: {}.", transactionType);
            transactionsQuery = transactionsQuery.Where(t => t.Type == transactionType);
        }

        Logger.LogInformation("Retrieving transactions for user {}.", user.Id);
        var transactions = await transactionsQuery
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        Logger.LogInformation("Successfully retrieved {} transactions.", transactions.Count);
        return TypedResults.Ok<IEnumerable<Transaction>>(transactions);
    }
}
