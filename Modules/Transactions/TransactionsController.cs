using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using ExpenseTrackerAPI.Dtos;
using ExpenseTrackerAPI.Models;
using ExpenseTrackerAPI.Interfaces;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing transactions.</summary>
/// <param name="transactionContext">The transaction context.</param>
/// <param name="authService">The authentication service.</param>
/// <param name="logger">The logger.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController(TransactionContext transactionContext, IAuthService authService, ILogger<TransactionsController> logger) : ControllerBase
{
    private readonly TransactionContext _transactionContext = transactionContext;
    private readonly IAuthService _authService = authService;
    private readonly ILogger<TransactionsController> _logger = logger;

    /// <summary>Get the transactions of the currently authenticated user.</summary>
    /// <param name="type">The type of transactions to filter by.</param>
    /// <returns>The transactions of the currently authenticated user.</returns>
    ///
    /// <response code="200">The transactions of the currently authenticated user.</response>
    /// <response code="400">Invalid transaction type.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<Transaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Results<BadRequest, Ok<IEnumerable<Transaction>>>> GetTransactions([FromQuery] string? type)
    {
        var transactionsQuery = _transactionContext.Transactions.AsQueryable();

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

        _logger.LogInformation("Retrieving transactions...");
        var transactions = await transactionsQuery
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();

        _logger.LogInformation("Successfully retrieved {} transactions.", transactions.Count);
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<UnauthorizedHttpResult, ForbidHttpResult, NotFound, Ok<Transaction>>> GetTransaction(Guid id)
    {
        var user = await _authService.GetUser();
        if (user is null)
        {
            _logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        _logger.LogInformation("Retrieving transaction with ID {}.", id);
        var transaction = await _transactionContext.Transactions.FindAsync(id);

        if (transaction is null)
        {
            _logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound();
        }

        if (transaction.UserId != user.Id)
        {
            _logger.LogInformation("User {} does not have access to transaction with ID {}.", user.Id, id);
            return TypedResults.Forbid();
        }

        _logger.LogInformation("Successfully retrieved transaction with ID {}.", id);
        return TypedResults.Ok(transaction);
    }

    /// <summary>Update a transaction by its ID.</summary>
    /// <param name="id">The ID of the transaction to update.</param>
    /// <param name="transactionDto">The updated transaction data.</param>
    /// <returns>No content.</returns>
    ///
    /// <response code="204">The transaction was updated successfully.</response>
    /// <response code="400">The transaction data was invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have access to the transaction with the specified ID.</response>
    /// <response code="404">No transaction with the specified ID was found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<UnauthorizedHttpResult, ForbidHttpResult, NotFound, Ok<Transaction>>> PutTransaction(Guid id, TransactionDto transactionDto)
    {
        var user = await _authService.GetUser();
        if (user is null)
        {
            _logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        _logger.LogInformation("Updating transaction with ID {}.", id);
        var transaction = await _transactionContext.Transactions.FindAsync(id);

        if (transaction is null)
        {
            _logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound();
        }

        if (transaction.UserId != user.Id)
        {
            _logger.LogInformation("User {} does not have access to transaction with ID {}.", user.Id, id);
            return TypedResults.Forbid();
        }

        _logger.LogInformation("Updating transaction with ID {} with data: {}.", id, transactionDto);
        _transactionContext.Entry(transaction).State = EntityState.Modified;
        transaction.Name = transactionDto.Name;
        transaction.Description = transactionDto.Description;
        transaction.Amount = transactionDto.Amount;
        transaction.Type = Enum.Parse<TransactionType>(transactionDto.Type, true);

        try
        {
            _logger.LogInformation("Saving changes to transaction with ID {}.", id);
            await _transactionContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TransactionExists(id))
        {
            _logger.LogWarning("No transaction with ID {} was found.", id);
            return TypedResults.NotFound();
        }

        _logger.LogInformation("Successfully updated transaction with ID {}.", id);
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
    public async Task<Results<UnauthorizedHttpResult, Created<Transaction>>> PostTransaction(TransactionDto transactionDto)
    {
        var user = await _authService.GetUser();
        if (user is null)
        {
            _logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        _logger.LogInformation("Creating transaction with data: {}.", transactionDto);
        var transaction = new Transaction
        {
            Name = transactionDto.Name,
            UserId = user.Id,
            Description = transactionDto.Description,
            Amount = transactionDto.Amount,
            Type = Enum.Parse<TransactionType>(transactionDto.Type, true)
        };

        _logger.LogInformation("Adding transaction with data: {}.", transactionDto);
        _transactionContext.Transactions.Add(transaction);
        await _transactionContext.SaveChangesAsync();

        _logger.LogInformation("Successfully created transaction with ID {}.", transaction.Id);
        var location = Url.Action(nameof(GetTransaction), new { id = transaction.Id }) ?? $"/{transaction.Id}";
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<UnauthorizedHttpResult, ForbidHttpResult, NotFound, NoContent>> DeleteTransaction(Guid id)
    {
        var user = await _authService.GetUser();
        if (user is null)
        {
            _logger.LogInformation("User is not authenticated.");
            return TypedResults.Unauthorized();
        }

        _logger.LogInformation("Deleting transaction with ID {}.", id);
        var transaction = await _transactionContext.Transactions.FindAsync(id);

        if (transaction is null)
        {
            _logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound();
        }

        if (transaction.UserId != user.Id)
        {
            _logger.LogInformation("User {} does not have access to transaction with ID {}.", user.Id, id);
            return TypedResults.Forbid();
        }

        _logger.LogInformation("Deleting transaction with ID {}.", id);
        _transactionContext.Transactions.Remove(transaction);
        await _transactionContext.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted transaction with ID {}.", id);
        return TypedResults.NoContent();
    }

    /// <summary>Check if a transaction exists by its ID.</summary>
    /// <param name="id">The ID of the transaction to check.</param>
    /// <returns>True if the transaction exists, false otherwise.</returns>
    private bool TransactionExists(Guid id)
    {
        return _transactionContext.Transactions.Any(e => e.Id == id);
    }
}
