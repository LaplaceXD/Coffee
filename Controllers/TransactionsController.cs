using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Controllers;

/// <summary>Controller for managing transactions.</summary>
[ApiController]
[Route("api/v1/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionContext _context;

    /// <summary>Initializes a new instance of the <see cref="TransactionsController"/> class.</summary>
    /// <param name="context">The transaction context.</param>
    public TransactionsController(TransactionContext context)
    {
        _context = context;
    }

    /// <summary>Get all transactions.</summary>
    /// <returns>All transactions.</returns>
    ///
    /// <response code="200">All transactions.</response>
    [HttpGet]
    public async Task<Ok<IEnumerable<Transaction>>> GetTransactions()
    {
        var transactions = await _context.Transactions.ToListAsync();
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
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return TypedResults.NotFound();
        }

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
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return TypedResults.NotFound();
        }

        transaction.Name = transactionDto.Name;
        transaction.Description = transactionDto.Description;
        transaction.Amount = transactionDto.Amount;
        transaction.Type = Enum.Parse<TransactionType>(transactionDto.Type, true);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TransactionExists(id))
        {
            return TypedResults.NotFound();
        }

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
        var transaction = new Transaction
        {
            Name = transactionDto.Name,
            Description = transactionDto.Description,
            Amount = transactionDto.Amount,
            Type = Enum.Parse<TransactionType>(transactionDto.Type, true)
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var location = Url.Action(nameof(GetTransaction), new { id = transaction.Id }) ?? $"/{transaction.Id}";
        return TypedResults.Created(location, transaction);
    }

    /// <summary>Delete a transaction by its ID.</summary>
    /// <param name="id">The ID of the transaction to delete.</param>
    /// <returns>No content.</returns>
    ///
    /// <response code="204">The transaction was deleted successfully.</response>
    /// <response code="404">No transaction with the specified ID was found.</response>
    /// <response code="400">The transaction data was invalid.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<NotFound, NoContent>> DeleteTransaction(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return TypedResults.NotFound();
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

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
