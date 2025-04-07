using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Data;
using WebApplication1.Model;

namespace WebApplication1.Pages.Groups
{
    public class MyGroupsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public User UserData { get; set; }

        public MyGroupsModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var username = User.Identity.Name;
            UserData = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (UserData == null)
            {
                return RedirectToPage("/Error");
            }

            return Page();
        }
    }
}
