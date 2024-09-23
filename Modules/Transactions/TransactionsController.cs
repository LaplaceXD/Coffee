using ExpenseTrackerAPI.Common;
using ExpenseTrackerAPI.Dtos;
using ExpenseTrackerAPI.Interfaces;
using ExpenseTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing transactions.</summary>
/// <param name="Context">The database context.</param>
/// <param name="AuthService">The authentication service.</param>
/// <param name="Logger">The logger.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController(
    ApplicationDbContext Context,
    IAuthService AuthService,
    ILogger<TransactionsController> Logger
) : ControllerBase
{
    /// <summary>Get all the transactions in the system or based on a filter.</summary>
    /// <param name="type">The type of transactions to filter by.</param>
    /// <returns>The list of transactions.</returns>
    ///
    /// <response code="200">The transactions available in the system.</response>
    /// <response code="400">Invalid transaction type.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<
        Results<BadRequest<ErrorResponse>, Ok<IEnumerable<Transaction>>>
    > GetTransactions([FromQuery] string? type)
    {
        var transactionsQuery = Context.Transactions.AsQueryable();

        if (type is not null)
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                Logger.LogWarning("Invalid transaction type: {}.", type);
                return TypedResults.BadRequest(
                    new ErrorResponse { Message = "Invalid transaction type." }
                );
            }

            Logger.LogInformation("Filtering transactions by type: {}.", transactionType);
            transactionsQuery = transactionsQuery.Where(t => t.Type == transactionType);
        }

        Logger.LogInformation("Retrieving transactions...");
        var transactions = await transactionsQuery
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        Logger.LogInformation("Successfully retrieved {} transactions.", transactions.Count);
        return TypedResults.Ok<IEnumerable<Transaction>>(transactions);
    }

    /// <summary>Get a transaction by its ID.</summary>
    /// <param name="id">The ID of the transaction to get.</param>
    /// <returns>The transaction with the specified ID.</returns>
    ///
    /// <response code="200">The transaction with the specified ID.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have access to the transaction with the specified ID.</response>
    /// <response code="404">No transaction with the specified ID was found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<
        Results<UnauthorizedHttpResult, ForbidHttpResult, NotFound<ErrorResponse>, Ok<Transaction>>
    > GetTransaction(Guid id)
    {
        var user = await AuthService.GetUser();
        if (user is null)
        {
            Logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        Logger.LogInformation("Retrieving transaction with ID {}.", id);
        var transaction = await Context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            Logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound(
                new ErrorResponse { Message = "No transaction with the specified ID was found." }
            );
        }

        if (transaction.OwnerId != user.Id)
        {
            Logger.LogInformation(
                "User {} does not have access to transaction with ID {}.",
                user.Id,
                id
            );
            return TypedResults.Forbid();
        }

        Logger.LogInformation("Successfully retrieved transaction with ID {}.", id);
        return TypedResults.Ok(transaction);
    }

    /// <summary>Update a transaction by its ID.</summary>
    /// <param name="id">The ID of the transaction to update.</param>
    /// <param name="transactionDto">The updated transaction data.</param>
    /// <returns>The updated transaction.</returns>
    ///
    /// <response code="200">The transaction was updated successfully.</response>
    /// <response code="400">The transaction data was invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have access to the transaction with the specified ID.</response>
    /// <response code="404">No transaction with the specified ID was found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<
        Results<UnauthorizedHttpResult, ForbidHttpResult, NotFound<ErrorResponse>, Ok<Transaction>>
    > PutTransaction(Guid id, TransactionDto transactionDto)
    {
        var user = await AuthService.GetUser();
        if (user is null)
        {
            Logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        Logger.LogInformation("Updating transaction with ID {}.", id);
        var transaction = await Context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            Logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound(
                new ErrorResponse { Message = "No transaction with the specified ID was found." }
            );
        }

        if (transaction.OwnerId != user.Id)
        {
            Logger.LogInformation(
                "User {} does not have access to transaction with ID {}.",
                user.Id,
                id
            );
            return TypedResults.Forbid();
        }

        Logger.LogInformation("Updating transaction with ID {} with data: {}.", id, transactionDto);
        Context.Entry(transaction).State = EntityState.Modified;
        transaction.Name = transactionDto.Name;
        transaction.Description = transactionDto.Description;
        transaction.Amount = transactionDto.Amount;
        transaction.Type = Enum.Parse<TransactionType>(transactionDto.Type, true);

        try
        {
            Logger.LogInformation("Saving changes to transaction with ID {}.", id);
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TransactionExists(id))
        {
            Logger.LogWarning("No transaction with ID {} was found.", id);
            return TypedResults.NotFound(
                new ErrorResponse { Message = "No transaction with the specified ID was found." }
            );
        }

        Logger.LogInformation("Successfully updated transaction with ID {}.", id);
        return TypedResults.Ok(transaction);
    }

    /// <summary>Create a new transaction.</summary>
    /// <param name="transactionDto">The transaction data.</param>
    /// <returns>The created transaction.</returns>
    ///
    /// <response code="201">The transaction was created successfully.</response>
    /// <response code="400">The transaction data was invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Transaction), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<Results<UnauthorizedHttpResult, Created<Transaction>>> PostTransaction(
        TransactionDto transactionDto
    )
    {
        var user = await AuthService.GetUser();
        if (user is null)
        {
            Logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        Logger.LogInformation("Creating transaction with data: {}.", transactionDto);
        var transaction = new Transaction
        {
            Name = transactionDto.Name,
            OwnerId = user.Id,
            Description = transactionDto.Description,
            Amount = transactionDto.Amount,
            Type = Enum.Parse<TransactionType>(transactionDto.Type, true),
        };

        Logger.LogInformation("Adding transaction with data: {}.", transactionDto);
        Context.Transactions.Add(transaction);
        await Context.SaveChangesAsync();

        Logger.LogInformation("Successfully created transaction with ID {}.", transaction.Id);
        var location =
            Url.Action(nameof(GetTransaction), new { id = transaction.Id }) ?? $"/{transaction.Id}";
        return TypedResults.Created(location, transaction);
    }

    /// <summary>Delete a transaction by its ID.</summary>
    /// <param name="id">The ID of the transaction to delete.</param>
    /// <returns>No content.</returns>
    ///
    /// <response code="204">The transaction was deleted successfully.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have access to the transaction with the specified ID.</response>
    /// <response code="404">No transaction with the specified ID was found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<
        Results<UnauthorizedHttpResult, ForbidHttpResult, NotFound<ErrorResponse>, NoContent>
    > DeleteTransaction(Guid id)
    {
        var user = await AuthService.GetUser();
        if (user is null)
        {
            Logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        Logger.LogInformation("Deleting transaction with ID {}.", id);
        var transaction = await Context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            Logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound(
                new ErrorResponse { Message = "No transaction with the specified ID was found." }
            );
        }

        if (transaction.OwnerId != user.Id)
        {
            Logger.LogInformation(
                "User {} does not have access to transaction with ID {}.",
                user.Id,
                id
            );
            return TypedResults.Forbid();
        }

        Logger.LogInformation("Deleting transaction with ID {}.", id);
        Context.Transactions.Remove(transaction);
        await Context.SaveChangesAsync();

        Logger.LogInformation("Successfully deleted transaction with ID {}.", id);
        return TypedResults.NoContent();
    }

    /// <summary>Check if a transaction exists by its ID.</summary>
    /// <param name="id">The ID of the transaction to check.</param>
    /// <returns>True if the transaction exists, false otherwise.</returns>
    private bool TransactionExists(Guid id)
    {
        return Context.Transactions.Any(e => e.Id == id);
    }
}
