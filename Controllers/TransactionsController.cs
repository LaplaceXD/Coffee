using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing transactions.</summary>
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionContext _context;
    private readonly ILogger<TransactionsController> _logger;

    /// <summary>Initializes a new instance of the <see cref="TransactionsController"/> class.</summary>
    /// <param name="context">The transaction context.</param>
    /// <param name="logger">The logger.</param>
    public TransactionsController(TransactionContext context, ILogger<TransactionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>Get all transactions.</summary>
    /// <param name="type">The type of transactions to get.</param>
    /// <returns>All transactions.</returns>
    ///
    /// <response code="200">All transactions.</response>
    /// <response code="400">The transaction type is invalid.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Results<BadRequest, Ok<IEnumerable<Transaction>>>> GetTransactions([FromQuery] string? type)
    {
        List<Transaction> transactions;

        if (type is not null)
        {
            if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                _logger.LogWarning("Invalid transaction type: {}.", type);
                return TypedResults.BadRequest();
            }

            _logger.LogInformation("Retrieving all transactions of type {}.", transactionType);
            transactions = await _context.Transactions
                .Where(t => t.Type == transactionType)
                .ToListAsync();
        }
        else
        {
            _logger.LogInformation("Retrieving all transactions.");
            transactions = await _context.Transactions.ToListAsync();
        }

        _logger.LogInformation("Successfully retrieved {} transactions.", transactions.Count);
        return TypedResults.Ok<IEnumerable<Transaction>>(transactions);
    }

    /// <summary>Get a transaction by its ID.</summary>
    /// <param name="id">The ID of the transaction to get.</param>
    /// <returns>The transaction with the specified ID.</returns>
    ///
    /// <response code="200">The transaction with the specified ID.</response>
    /// <response code="404">No transaction with the specified ID was found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NotFound, Ok<Transaction>>> GetTransaction(Guid id)
    {
        _logger.LogInformation("Retrieving transaction with ID {}.", id);
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            _logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound();
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
    /// <response code="404">No transaction with the specified ID was found.</response>
    /// <response code="400">The transaction data was invalid.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Results<NotFound, Ok<Transaction>>> PutTransaction(Guid id, TransactionDto transactionDto)
    {
        _logger.LogInformation("Updating transaction with ID {}.", id);
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            _logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound();
        }

        _logger.LogInformation("Updating transaction with ID {} with data: {}.", id, transactionDto);
        transaction.Name = transactionDto.Name;
        transaction.Description = transactionDto.Description;
        transaction.Amount = transactionDto.Amount;
        transaction.Type = Enum.Parse<TransactionType>(transactionDto.Type, true);

        try
        {
            _logger.LogInformation("Saving changes to transaction with ID {}.", id);
            await _context.SaveChangesAsync();
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
    /// <response code="404">No transaction with the specified ID was found.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<Created<Transaction>> PostTransaction(TransactionDto transactionDto)
    {
        _logger.LogInformation("Creating transaction with data: {}.", transactionDto);
        var transaction = new Transaction
        {
            Name = transactionDto.Name,
            Description = transactionDto.Description,
            Amount = transactionDto.Amount,
            Type = Enum.Parse<TransactionType>(transactionDto.Type, true)
        };

        _logger.LogInformation("Adding transaction with data: {}.", transactionDto);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created transaction with ID {}.", transaction.Id);
        var location = Url.Action(nameof(GetTransaction), new { id = transaction.Id }) ?? $"/{transaction.Id}";
        return TypedResults.Created(location, transaction);
    }

    /// <summary>Delete a transaction by its ID.</summary>
    /// <param name="id">The ID of the transaction to delete.</param>
    /// <returns>No content.</returns>
    ///
    /// <response code="204">The transaction was deleted successfully.</response>
    /// <response code="404">No transaction with the specified ID was found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NotFound, NoContent>> DeleteTransaction(Guid id)
    {
        _logger.LogInformation("Deleting transaction with ID {}.", id);
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            _logger.LogInformation("No transaction with ID {} was found.", id);
            return TypedResults.NotFound();
        }

        _logger.LogInformation("Deleting transaction with ID {}.", id);
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted transaction with ID {}.", id);
        return TypedResults.NoContent();
    }

    /// <summary>Check if a transaction exists by its ID.</summary>
    /// <param name="id">The ID of the transaction to check.</param>
    /// <returns>True if the transaction exists, false otherwise.</returns>
    private bool TransactionExists(Guid id)
    {
        return _context.Transactions.Any(e => e.Id == id);
    }
}
