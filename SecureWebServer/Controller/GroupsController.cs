using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SecureWebServer.Data;
using SecureWebServer.Models;


namespace SecureWebServer.Controller
{
    using Microsoft.AspNetCore.Mvc;

    [Authorize] // Only authenticated users can create groups
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Groups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                var group = new Group
                {
                    GroupName = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false 
                };

                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                // Add the creator to the group
                var userGroup = new UserGroup
                {
                    UserId = user.UserId,
                    GroupId = group.GroupId
                };
                _context.UserGroups.Add(userGroup);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Group created successfully and is pending admin approval.";
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // GET: Groups/List
        public async Task<IActionResult> List()
        {
            var groups = await _context.Groups.Where(g => g.IsApproved).ToListAsync();
            return View(groups);
        }

        // POST: Groups/Join
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            var membership = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == user.UserId && ug.GroupId == id);

            if (membership != null)
            {
                TempData["Message"] = "You have already requested to join this group.";
                return RedirectToAction("List");
            }

            var userGroup = new UserGroup
            {
                UserId = user.UserId,
                GroupId = id
            };

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Join request sent and is pending admin approval.";
            return RedirectToAction("List");
        }

    }

}
