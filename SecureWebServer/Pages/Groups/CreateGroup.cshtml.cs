using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecureWebServer.Data;
using SecureWebServer.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecureWebServer.Pages.Groups
{
    public class CreateGroupModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateGroupModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GroupInputModel Input { get; set; }

        public class GroupInputModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Account/Login");
            }

            int creatorId = int.Parse(userIdClaim.Value);
            bool isAdmin = User.IsInRole("admin");

            // Check if a group with the same name already exists
            bool groupExists = await _context.Groups
                .AnyAsync(g => g.GroupName.ToLower() == Input.Name.ToLower());

            if (groupExists)
            {
                ModelState.AddModelError(string.Empty, "A group with this name already exists.");
                return Page();
            }

            var group = new Group
            {
                GroupName = Input.Name,
                Description = Input.Description,
                CreatorUserId = creatorId,
                IsApproved = isAdmin,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            if (isAdmin)
            {
                TempData["Message"] = "Group created successfully!";
                TempData["IsAdmin"] = true;
            }
            else
            {
                TempData["Message"] = "Your group is pending admin approval.";
                TempData["IsAdmin"] = false;
            }

            // Clear input fields
            ModelState.Clear();
            Input = new GroupInputModel();

            return Page();
        }
    }
}
