using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;   // Namespace where ApplicationDbContext is
using WebApplication1.Model;  // Namespace where User model is
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Mail;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }
    // 🔹 GET: api/User/CurrentUser (Fetch current user)
    [HttpGet("Current")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var currentUser = await _context.Users.FindAsync(currentUserId);
        return currentUser;
    }

    // 🔹 GET: api/User (Fetch all users)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.ToListAsync();
    }

    // 🔹 GET: api/User/{id} (Fetch a single user by ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return user;
    }

    // 🔹 POST: api/User (Create a new user)
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        if (user == null) return BadRequest("Invalid user data");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] JsonElement userUpdate)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        if (userUpdate.TryGetProperty("Username", out var username))
        {
            string newUsername = username.GetString();
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == newUsername);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Username already taken." });
            }

            user.Username = newUsername;

            // Refresh authentication claims
            var claimsPrincipal = HttpContext.User;
            var identity = (System.Security.Claims.ClaimsIdentity)claimsPrincipal.Identity;
            identity.RemoveClaim(identity.FindFirst(System.Security.Claims.ClaimTypes.Name));
            identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, newUsername));

            await HttpContext.SignInAsync(claimsPrincipal);
        }
        if (userUpdate.TryGetProperty("Email", out var email))
        {
            string emailString = email.GetString();
            if (IsValidEmail(emailString))
            {
                user.Email = email.GetString();
            }
            else
            {
                return BadRequest(new { message = "Invalid email format." });
            }
        }
        if (userUpdate.TryGetProperty("Fullname", out var fullname)) user.Fullname = fullname.GetString();
        if (userUpdate.TryGetProperty("PasswordHash", out var passwordHash)) user.PasswordHash = passwordHash.GetString();

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 🔹 DELETE: api/User/{id} (Delete a user)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // 🔹 POST: api/User/{action}/{id} (Change one attribute of a user)
    [HttpPut("{action}/{userId}")]
    public async Task<IActionResult> UpdateUserAttribute(string action, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        switch (action)
        {
            case "change-email":
                user.Email = "email";
                break;
            case "change-fullname":
                user.Fullname = "fullname";
                break;
            case "change-username":
                user.Username = "email";
                break;
            default:
                return BadRequest("Invalid action.");
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "User attribute updated successfully" });
    }

    // Helper functions
    private bool IsValidEmail(string emailaddress)
    {
        try
        {
            MailAddress m = new MailAddress(emailaddress);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

}
