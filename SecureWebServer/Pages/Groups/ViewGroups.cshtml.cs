using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecureWebServer.Data;
using SecureWebServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecureWebServer.Pages.Groups
{
    public class ViewGroupsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ViewGroupsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Group> UserGroups { get; set; }
        public List<Group> OtherGroups { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Account/Login");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Fetch groups the user has already joined
            UserGroups = await _context.UserGroups
                .Where(ug => ug.UserId == userId)
                .Select(ug => ug.Group)
                .ToListAsync();

            // Fetch groups that the user hasn't joined yet and are approved
            OtherGroups = await _context.Groups
                .Where(g => g.IsApproved && !g.UserGroups.Any(ug => ug.UserId == userId))
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostJoinGroupAsync(int groupId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Account/Login");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Check if the user is already a member of the group
            var existingMembership = await _context.UserGroups
                .AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (!existingMembership)
            {
                // Add user to the group
                var userGroup = new UserGroup
                {
                    UserId = userId,
                    GroupId = groupId
                };

                _context.UserGroups.Add(userGroup);
                await _context.SaveChangesAsync();

                TempData["Message"] = "You have successfully joined the group!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLeaveGroupAsync(int groupId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Account/Login");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Find the membership entry
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup != null)
            {
                // Remove the user from the group
                _context.UserGroups.Remove(userGroup);
                await _context.SaveChangesAsync();

                TempData["Message"] = "You have successfully left the group.";
            }

            return RedirectToPage();
        }
    }
}
