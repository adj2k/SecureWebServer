using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecureWebServer.Data;
using SecureWebServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecureWebServer.Pages.Admin
{
    public class PendingUsersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PendingUsersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> PendingUsers { get; set; }
        public List<User> ApprovedUsers { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch pending users (IsApproved == false)
            PendingUsers = await _context.Users
                .Where(u => !u.IsApproved)
                .ToListAsync();

            // Fetch approved users (IsApproved == true)
            ApprovedUsers = await _context.Users
                .Where(u => u.IsApproved)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsApproved = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
