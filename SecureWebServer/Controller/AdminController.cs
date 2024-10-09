using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SecureWebServer.Data;


namespace SecureWebServer.Controller
{
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/PendingUsers
        public async Task<IActionResult> PendingUsers()
        {
            if(!User.Identity.IsAuthenticated && !User.IsInRole("admin"))
            {
                return NotFound();
            }
            var pendingUsers = await _context.Users.Where(u => !u.IsApproved).ToListAsync();
            return View(pendingUsers);
        }

        // POST: Admin/ApproveUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"User {user.UserName} approved successfully.";
            }
            return RedirectToAction("PendingUsers");
        }

        // GET: Admin/PendingGroups
        public async Task<IActionResult> PendingGroups()
        {
            if (!User.Identity.IsAuthenticated && !User.IsInRole("admin"))
            {
                return NotFound();
            }

            var pendingGroups = await _context.Groups
                .Include(g => g.Creator)
                .Where(g => !g.IsApproved)
                .ToListAsync();
            return View(pendingGroups);
        }

        // POST: Admin/ApproveGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                group.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Group {group.GroupName} approved successfully.";
            }
            return RedirectToAction("PendingGroups");
        }

        // GET: Admin/PendingGroupMembers
        public async Task<IActionResult> PendingGroupMembers()
        {
            var pendingMembers = await _context.UserGroups
                .Include(ug => ug.User)
                .Include(ug => ug.Group)
                .ToListAsync();

            return View(pendingMembers);
        }

        // POST: Admin/ApproveGroupMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveGroupMember(int userId, int groupId)
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup != null)
            {
                await _context.SaveChangesAsync();
                TempData["Message"] = $"User {userGroup.User.UserName} added to group {userGroup.Group.GroupName}.";
            }
            return RedirectToAction("PendingGroupMembers");
        }
    }

}
