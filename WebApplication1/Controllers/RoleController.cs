using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Model;

[Route("api/[controller]")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoleController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 GET: api/Role (Fetch all roles)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        return await _context.Roles.ToListAsync();
    }

    // 🔹 GET: api/Role/{id} (Fetch a single role by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<Role>> GetRoleById(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return NotFound();
        return role;
    }

    // 🔹 POST: api/Role (Create a new role)
    [HttpPost]
    public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
    {
        if (role == null || string.IsNullOrWhiteSpace(role.Name))
            return BadRequest("Role name is required.");

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
    }

    // 🔹 PUT: api/Role/{id} (Update an existing role)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] Role updatedRole)
    {
        if (id != updatedRole.Id)
            return BadRequest("Role ID mismatch.");

        var role = await _context.Roles.FindAsync(id);
        if (role == null) return NotFound();

        role.Name = updatedRole.Name;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 🔹 DELETE: api/Role/{id} (Delete a role)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return NotFound();

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
