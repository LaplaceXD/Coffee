using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Controllers;

[ApiController]
[Route("api/v1/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionContext _context;

    public TransactionsController(TransactionContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<Ok<IEnumerable<Transaction>>> GetTransactions()
    {
        var transactions = await _context.Transactions.ToListAsync();
        return TypedResults.Ok<IEnumerable<Transaction>>(transactions);
    }

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

    private bool TransactionExists(Guid id)
    {
        return _context.Transactions.Any(e => e.Id == id);
    }
}
