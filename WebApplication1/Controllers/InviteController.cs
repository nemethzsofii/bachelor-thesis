using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class InviteController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InviteController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 GET: api/Invite (Fetch all invites)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invite>>> GetAllInvites()
    {
        return await _context.Invites.ToListAsync();
    }

    // 🔹 GET: api/Invite/{id} (Fetch a single invite by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<Invite>> GetInviteById(int id)
    {
        var invite = await _context.Invites.FindAsync(id);
        if (invite == null) return NotFound();
        return invite;
    }

    // 🔹 POST: api/Invite (Create a new invite)
    [HttpPost]
    public async Task<ActionResult<Invite>> CreateInvite([FromBody] Invite invite)
    {
        if (invite == null) return BadRequest("Invalid invite data");

        bool exists = await _context.Invites.AnyAsync(i =>
            i.ReceiverUserId == invite.ReceiverUserId &&
            i.SenderUserId == invite.SenderUserId &&
            i.GroupId == invite.GroupId);

        if (exists)
        {
            return Conflict("Invite already exists between these users for this group.");
        }

        _context.Invites.Add(invite);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInviteById), new { id = invite.Id }, invite);
    }

    // 🔹 PUT: api/Invite/{id} (Update an invite)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInvite(int id, [FromBody] Invite updatedInvite)
    {
        if (id != updatedInvite.Id) return BadRequest("Invite ID mismatch");

        var invite = await _context.Invites.FindAsync(id);
        if (invite == null) return NotFound();

        // Update fields
        invite.SenderUserId = updatedInvite.SenderUserId;
        invite.ReceiverUserId = updatedInvite.ReceiverUserId;
        invite.Accepted = updatedInvite.Accepted;
        invite.CreatedAt = updatedInvite.CreatedAt;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 🔹 DELETE: api/Invite/{id} (Delete an invite)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvite(int id)
    {
        var invite = await _context.Invites.FindAsync(id);
        if (invite == null) return NotFound();

        _context.Invites.Remove(invite);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 🔹 GET: api/Invite/receiver/{receiverUserId} (Fetch invites sent to a specific user)
    [HttpGet("receiver/{receiverUserId}")]
    public async Task<ActionResult<IEnumerable<Invite>>> GetInvitesByReceiver(int receiverUserId)
    {
        var invites = await _context.Invites
            .Where(i => i.ReceiverUserId == receiverUserId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invites;
    }

    // 🔹 GET: api/Invite/sender/{senderUserId} (Fetch invites sent by a specific user)
    [HttpGet("sender/{senderUserId}")]
    public async Task<ActionResult<IEnumerable<Invite>>> GetInvitesBySender(int senderUserId)
    {
        var invites = await _context.Invites
            .Where(i => i.SenderUserId == senderUserId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invites;
    }

    // 🔹 GET: api/Invite/receiver/{receiverUserId}/pending (Get only pending invites for a receiver)
    [HttpGet("receiver/{receiverUserId}/pending")]
    public async Task<ActionResult<IEnumerable<Invite>>> GetPendingInvitesForReceiver(int receiverUserId)
    {
        var invites = await _context.Invites
            .Where(i => i.ReceiverUserId == receiverUserId && i.Accepted == 0)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invites;
    }

    // 🔹 GET: api/Invite/current (Fetch invites for the current user)
    [HttpGet("current")]
    public async Task<ActionResult<IEnumerable<Invite>>> GetInvitesForCurrentUser()
    {
        int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var invites = await _context.Invites
            .Where(i => i.ReceiverUserId == currentUserId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invites;
    }

}
