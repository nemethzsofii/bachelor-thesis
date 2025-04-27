using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Utils;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Diagnostics;
using System.Globalization;

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
        if (transaction.Amount < 0) return BadRequest("Amount has to be a positive integer!");

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    // 🔹 POST: api/Transaction/csv (Create new transactions by uploading a csv file)
    [HttpPost("csv")]
    public async Task<IActionResult> UploadCsvTransactions([FromForm] IFormFile file)
    {
        int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }
        if (Path.GetExtension(file.FileName).ToLower() != ".csv")
        {
            return BadRequest("File type is not .csv");
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var lineNumber = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = new string[10000];
            try
            {
                parts = line.Split(',');
            }catch(Exception)
            {
                try
                {
                    parts = line.Split(';');
                }catch(Exception)
                {
                    return BadRequest("Fields need to be separated by either ',' or ';'.");
                }
            }
            

            if (parts.Length != 5)
            {
                return BadRequest($"Line {lineNumber} is invalid.");
            }
            // validate type
            var typeIsInt = int.TryParse(parts[0].Trim(), out int type);
            if(type != 1 && type != 2)
            {
                return BadRequest("Type can only be 1 or 2");
            }
            // validate category
            var category = parts[1].Trim();
            int? categoryId = null;
            Category? categoryInDB = _context.Categories.FirstOrDefault(c => c.Name == category);
            if (categoryInDB is not null)
            {
                categoryId = categoryInDB.Id;
            }
            // desc doesnt rlly need validation
            var description = parts[2].Trim();
            // validate amount
            bool amountIsInt = int.TryParse(parts[3].Trim(), out int amount);
            if(amount < 0 || !amountIsInt)
            {
                return BadRequest("Amount needs to be a positive integer");
            }
            // validate date
            var dateFromForm = parts[4].Trim();
            if(DateTime.TryParseExact(dateFromForm, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                date = date.Date.AddHours(12);
            }
            else
            {
                return BadRequest("Date format invalid!");
            }
            try
            {
                _context.Transactions.Add(new Transaction(currentUserId, amount, type, categoryId, date, description, groupId: null));

            } catch (Exception e)
            {
                Debug.WriteLine(e);
                return BadRequest("Something went wrong!");
            }
            
        }
        await _context.SaveChangesAsync();
        return Ok("Data saved successfully");
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
        transaction.TypeId = updatedTransaction.TypeId;
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

    // 🔹 GET: api/Transaction/user/{userId}/type/{typeId} (Fetch personal transactions by user by type)
    [HttpGet("user/{userId}/type/{typeId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserByType(int userId, int typeId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.TypeId == typeId && t.GroupId == null)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions;
    }

    // 🔹 GET: api/Transaction/user/{userId}/group/{group} (Fetch transactions by user by group)
    [HttpGet("user/{userId}/group/{groupId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserByGroup(int userId, int groupId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.GroupId == groupId)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions;
    }

    // 🔹 GET: api/Transaction/user/{userId}/type/{typeId}/group/{groupId} (Fetch transactions by user by type by group)
    [HttpGet("user/{userId}/type/{typeId}/group/{groupId}")]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByUserByTypeByGroup(int userId, int typeId, int? groupId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.TypeId == typeId && t.GroupId == groupId)
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

    // 🔹 GET: DistinctYears/{currentUserId} (Fetch distinct years of transactions for given user)
    [HttpGet("DistinctYears/{currentUserId}")]
    public async Task<ActionResult<IEnumerable<int>>> GetDistinctTransactionYears(int currentUserId)
    {
        var years = await _context.Transactions
            .Where(t => t.UserId == currentUserId)
            .Select(t => t.Date.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();


        return years;
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
