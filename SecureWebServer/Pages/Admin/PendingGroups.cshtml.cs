using Microsoft.AspNetCore.Authorization;
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
    public class PendingGroupsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PendingGroupsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Group> PendingGroups { get; set; }
        public List<Group> ActiveGroups { get; set; }
        public Dictionary<int, string> Usernames { get; set; }

        
        public async Task OnGetAsync()
        {
            
                // Fetch pending groups
                PendingGroups = await _context.Groups
                .Where(g => !g.IsApproved)
                .ToListAsync();

            // Fetch active groups
            ActiveGroups = await _context.Groups
                .Where(g => g.IsApproved)
                .ToListAsync();

            // Fetch all usernames
            var users = await _context.Users.ToListAsync();
            Usernames = users.ToDictionary(u => u.UserId, u => u.UserName);

        }

        public async Task<IActionResult> OnPostApproveGroup(int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            group.IsApproved = true;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Group approved successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDenyGroup(int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Group denied and removed successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteGroup(int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Group deleted successfully!";
            return RedirectToPage();
        }
    }
}
