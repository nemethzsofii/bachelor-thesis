using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Utils;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TransactionController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 GET: api/Transaction (Fetch all transactions)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        return await _context.Transactions.ToListAsync();
    }

    // 🔹 GET: api/Transaction/{id} (Fetch a single transaction by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<Transaction>> GetTransaction(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();
        return transaction;
    }

    // 🔹 POST: api/Transaction (Create a new transaction)
    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] Transaction transaction)
    {
        if (transaction == null) return BadRequest("Invalid transaction data");

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    // 🔹 PUT: api/Transaction/{id} (Update a transaction)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(int id, [FromBody] Transaction updatedTransaction)
    {
        if (id != updatedTransaction.Id) return BadRequest("Transaction ID mismatch");

        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        // Update transaction fields
        transaction.Amount = updatedTransaction.Amount;
        transaction.Type = updatedTransaction.Type;
        transaction.CategoryId = updatedTransaction.CategoryId;
        transaction.Date = updatedTransaction.Date;
        transaction.Description = updatedTransaction.Description;
        transaction.GroupId = updatedTransaction.GroupId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 🔹 DELETE: api/Transaction/{id} (Delete a transaction)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null) return NotFound();

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 🔹 GET: api/Transaction/user/{userId} (Fetch personal transactions by user)
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUser(int userId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.GroupId == null)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions;
    }

    // 🔹 GET: api/Transaction/group/{groupId} (Fetch group transactions by group)
    [HttpGet("group/{groupId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByGroup(int groupId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.GroupId == groupId)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions;
    }

    // 🔹 GET: api/Transaction/user/{userId}/type/{type} (Fetch personal transactions by user by type)
    [HttpGet("user/{userId}/type/{type}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserByType(int userId, string type)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == type && t.GroupId == null)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions;
    }

    // 🔹 GET: api/Transaction/user/{userId}/type/{type} (Fetch transactions by user by group)
    [HttpGet("user/{userId}/group/{groupId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserByGroup(int userId, int groupId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.GroupId == groupId)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions;
    }

    // 🔹 GET: api/Transaction/user/{userId}/type/{type}/group/{groupId} (Fetch transactions by user by type by group)
    [HttpGet("user/{userId}/type/{type}/group/{groupId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserByTypeByGroup(int userId, string type, int? groupId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == type && t.GroupId == groupId)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions;
    }

    // 🔹 GET: api/Transaction/category/{categoryId} (Fetch transactions by category)
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByCategory(int categoryId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();

        return transactions;
    }

    [HttpGet("DownloadReportForCurrent")]
    public async Task<IActionResult> DownloadReport()
    {
        int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null)
            return NotFound("User not found");

        var transactions = await _context.Transactions
            .Where(t => t.UserId == currentUserId)
            .ToListAsync();

        var pdf = new PdfGenerator(user.Fullname, transactions);
        var pdfBytes = pdf.GeneratePdf();

        return File(pdfBytes, "application/pdf", $"report_{user.Fullname}_{DateTime.Now:yyyyMMdd}.pdf");
    }

}
