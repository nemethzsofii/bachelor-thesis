using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using WebApplication1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Model;
using WebApplication1.Utils;

namespace WebApplication1.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RegisterViewModel RegisterData { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            string generatedSalt = UtilFunctions.GenerateSalt();
            string hashedPassword = UtilFunctions.HashPassword(RegisterData.Password, generatedSalt);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == RegisterData.Username);
            if (existingUser != null)
            {
                ErrorMessage = "Username already taken. Please choose another.";
                return Page();
            }else if (!UtilFunctions.ValidatePasswordStrength(RegisterData.Password))
            {
                ErrorMessage = "Password must be at least 8 characters and include a lowercase letter, an uppercase letter, a digit, and a special character";
                return Page();
            }

                var newUser = new User
                {
                    Username = RegisterData.Username,
                    Email = RegisterData.Email,
                    Fullname = RegisterData.Fullname,
                    PasswordHash = hashedPassword,
                    Salt = generatedSalt
                };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToPage("/Account/Login");
        }
    }
}
