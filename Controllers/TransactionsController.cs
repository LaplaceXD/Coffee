using Microsoft.AspNetCore.Mvc;
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
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        return await _context.Transactions.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return NotFound();
        }

        return transaction;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Transaction>> PutTransaction(Guid id, TransactionDto transactionDto)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return NotFound();
        }

        transaction.Name = transactionDto.Name;
        transaction.Description = transactionDto.Description;
        transaction.Amount = transactionDto.Amount;
        transaction.Timestamp = transactionDto.Timestamp;
        transaction.Type = transactionDto.Type;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TransactionExists(id))
        {
            return NotFound();
        }

        return transaction;
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> PostTransaction(TransactionDto transactionDto)
    {
        var transaction = new Transaction
        {
            Name = transactionDto.Name,
            Description = transactionDto.Description,
            Amount = transactionDto.Amount,
            Timestamp = transactionDto.Timestamp,
            Type = transactionDto.Type
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
        {
            return NotFound();
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TransactionExists(Guid id)
    {
        return _context.Transactions.Any(e => e.Id == id);
    }
}
