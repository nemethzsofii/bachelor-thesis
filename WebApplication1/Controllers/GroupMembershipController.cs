using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Model;

[Route("api/[controller]")]
[ApiController]
public class GroupMembershipController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GroupMembershipController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 GET: api/GroupMembership (Get all memberships)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupMembership>>> GetAllMemberships()
    {
        return await _context.GroupMemberships.ToListAsync();
    }

    // 🔹 GET: api/GroupMembership/{id} (Get membership by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<GroupMembership>> GetMembershipById(int id)
    {
        var membership = await _context.GroupMemberships.FindAsync(id);
        if (membership == null) return NotFound();

        return membership;
    }

    // 🔹 GET: api/GroupMembership/ByGroup/{groupId} (Get memberships in a group)
    [HttpGet("ByGroup/{groupId}")]
    public async Task<ActionResult<IEnumerable<GroupMembership>>> GetMembershipsByGroup(int groupId)
    {
        return await _context.GroupMemberships
            .Where(m => m.GroupId == groupId)
            .ToListAsync();
    }

    // 🔹 GET: api/GroupMembership/ByUser (Get memberships for current user)
    [HttpGet("ByUser")]
    public async Task<ActionResult<IEnumerable<GroupMembership>>> GetMembershipsByCurrentUser()
    {
        int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        return await _context.GroupMemberships
            .Where(m => m.UserId == currentUserId)
            .ToListAsync();
    }

    // 🔹 POST: api/GroupMembership (Add user to a group)
    [HttpPost]
    public async Task<ActionResult<GroupMembership>> AddMembership([FromBody] GroupMembership membership)
    {
        if (membership == null) return BadRequest("Invalid data.");

        _context.GroupMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMembershipById), new { id = membership.Id }, membership);
    }

    // 🔹 PUT: api/GroupMembership/{id} (Update a membership, e.g., change role)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMembership(int id, [FromBody] GroupMembership updatedMembership)
    {
        if (id != updatedMembership.Id) return BadRequest("ID mismatch.");

        var membership = await _context.GroupMemberships.FindAsync(id);
        if (membership == null) return NotFound();

        membership.RoleId = updatedMembership.RoleId;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 🔹 DELETE: api/GroupMembership/{id} (Remove membership)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMembership(int id)
    {
        var membership = await _context.GroupMemberships.FindAsync(id);
        if (membership == null) return NotFound();

        _context.GroupMemberships.Remove(membership);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
