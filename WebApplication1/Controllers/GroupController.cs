using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class GroupController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GroupController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 GET: api/Group (Get all groups)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Group>>> GetGroups()
    {
        return await _context.Groups.ToListAsync();
    }

    // 🔹 GET: api/Group/{id} (Get group by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<Group>> GetGroup(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null) return NotFound();

        return group;
    }

    // 🔹 POST: api/Group (Create a new group)
    [HttpPost]
    public async Task<ActionResult<Group>> CreateGroup([FromBody] Group group)
    {
        if (group == null || string.IsNullOrWhiteSpace(group.Name))
            return BadRequest("Group name is required.");

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    // 🔹 PUT: api/Group/{id} (Update group)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] Group updatedGroup)
    {
        if (id != updatedGroup.Id) return BadRequest("ID mismatch.");

        var group = await _context.Groups.FindAsync(id);
        if (group == null) return NotFound();

        group.Name = updatedGroup.Name;
        group.Type = updatedGroup.Type;
        group.Description = updatedGroup.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 🔹 DELETE: api/Group/{id} (Delete group)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null) return NotFound();

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
