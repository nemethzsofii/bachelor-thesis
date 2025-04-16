using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebApplication1.Data;
using WebApplication1.Model;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SavingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SavingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔹 GET: api/Saving
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Saving>>> GetAll()
        {
            return await _context.Savings.ToListAsync();
        }

        // 🔹 GET: api/Saving/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Saving>> GetById(int id)
        {
            var saving = await _context.Savings.FindAsync(id);
            if (saving == null) return NotFound();
            return saving;
        }

        // 🔹 GET: api/Saving/groupid/{id}
        [HttpGet("groupid/{groupId}")]
        public async Task<ActionResult<IEnumerable<Saving>>> GetByGroupId(int groupId)
        {
            var savings = await _context.Savings
                .Where(s => s.GroupId == groupId)
                .ToListAsync();

            return savings;
        }

        // 🔹 GET: api/Saving/current
        [HttpGet("current")]
        public async Task<ActionResult<IEnumerable<Saving>>> GetSavingsForCurrent()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User is not logged in.");

            int currentUserId = int.Parse(userIdClaim);

            return await _context.Savings
                .Where(s => s.UserId == currentUserId)
                .ToListAsync();
        }

        // 🔹 POST: api/Saving
        [HttpPost]
        public async Task<ActionResult<Saving>> Create([FromForm] SavingDto savingDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User is not logged in.");

            int currentUserId = int.Parse(userIdClaim);

            var saving = new Saving
            {
                Title = savingDto.Title,
                Description = savingDto.Description,
                GoalAmount = savingDto.GoalAmount,
                Deadline = savingDto.Deadline,
                CurrentAmount = 0,
                CreatedAt = DateTime.UtcNow,
                UserId = currentUserId
            };

            _context.Savings.Add(saving);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = saving.Id }, saving);
        }

        // 🔹 PUT: api/Saving/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Saving updatedSaving)
        {
            if (id != updatedSaving.Id) return BadRequest("Saving ID mismatch.");

            var saving = await _context.Savings.FindAsync(id);
            if (saving == null) return NotFound();

            saving.Title = updatedSaving.Title;
            saving.Description = updatedSaving.Description;
            saving.GoalAmount = updatedSaving.GoalAmount;
            saving.Deadline = updatedSaving.Deadline;
            saving.CurrentAmount = updatedSaving.CurrentAmount;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // 🔹 PATCH: api/Saving/{id}
        [HttpPatch("{id}")]
        public async Task<ActionResult<Saving>> Patch(int id, [FromBody] JsonElement patchData)
        {
            var saving = await _context.Savings.FindAsync(id);
            if (saving == null) return NotFound();

            foreach (var prop in patchData.EnumerateObject())
            {
                switch (prop.Name.ToLower())
                {
                    case "title":
                        saving.Title = prop.Value.GetString();
                        break;
                    case "description":
                        saving.Description = prop.Value.GetString();
                        break;
                    case "goalamount":
                        saving.GoalAmount = prop.Value.GetInt32();
                        break;
                    case "deadline":
                        saving.Deadline = prop.Value.GetDateTime();
                        break;
                    case "currentamount":
                        saving.CurrentAmount = prop.Value.GetInt32();
                        break;
                }
            }

            await _context.SaveChangesAsync();
            return saving;
        }

        // 🔹 DELETE: api/Saving/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var saving = await _context.Savings.FindAsync(id);
            if (saving == null) return NotFound();

            _context.Savings.Remove(saving);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class SavingDto
    {
        [FromForm]
        public string Title { get; set; }

        [FromForm]
        public string? Description { get; set; }

        [FromForm]
        public int? GoalAmount { get; set; }

        [FromForm]
        public DateTime? Deadline { get; set; }
    }
}
