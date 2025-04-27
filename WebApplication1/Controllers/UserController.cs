using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Mail;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Utils;

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

    // 🔹 GET: api/User/username/{username} (Fetch a single user by username)
    [HttpGet("username/{username}")]
    public async Task<ActionResult<User>> GetUserByUsername(string username)
    {
        var user = await _context.Users
            .Where(u => u.Username == username)
            .FirstOrDefaultAsync();
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
        System.Diagnostics.Debug.WriteLine("######");
        System.Diagnostics.Debug.WriteLine(userUpdate);

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
            if (UtilFunctions.IsValidEmail(emailString))
            {
                user.Email = email.GetString();
            }
            else
            {
                return BadRequest(new { message = "Invalid email format." });
            }
        }
        if (userUpdate.TryGetProperty("Fullname", out var fullname)) user.Fullname = fullname.GetString();
        
        try
        {
            if (userUpdate.TryGetProperty("MonthlySpendingLimit", out var limit))
            {
                if(limit.GetInt32() < 0)
                {
                    return BadRequest("Amount has to be a positive integer!");
                }
                user.MonthlySpendingLimit = limit.GetInt32();
            }
        }catch(Exception)
        {
            return BadRequest(new { message = "MonthlySpendingLimit should be an integer." });
        }

        if (userUpdate.TryGetProperty("Password", out var passwordJson))
        {
            try
            {
                string oldPassword = passwordJson.GetProperty("oldPw").GetString();
                string newPassword = passwordJson.GetProperty("newPw").GetString();

                var salt = user.Salt;
                var oldHash = UtilFunctions.HashPassword(oldPassword, salt);
                if (user.PasswordHash != oldHash)
                {
                    return BadRequest(new { message = "Old password is incorrect." });
                }else if (!UtilFunctions.ValidatePasswordStrength(newPassword))
                {
                    return BadRequest(new { message = "Password is not strong enough." });
                }

                    var newHash = UtilFunctions.HashPassword(newPassword, salt);
                user.PasswordHash = newHash;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return BadRequest(new { message = "Failed to update password." });
            }
        }

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

}
