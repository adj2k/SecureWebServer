using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecureWebServer.Data;
using SecureWebServer.Models;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SecureWebServer.Pages
{
    public class MainFeedModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public MainFeedModel(ApplicationDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        public List<Item> DataItems { get; set; }

        public List<string> FileContents { get; set; }

        [BindProperty]
        public string ItemName { get; set; }

        [BindProperty]
        public string ItemDescription { get; set; }

        [BindProperty]
        public IFormFile Upload { get; set; }
        public List<Group> UserGroups { get; set; }

        public async Task OnGetAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim?.Value;
            var isAdmin = User.IsInRole("admin");

            // Fetch data from the database
            if (isAdmin)
            {
                // Admin can see all posts
                DataItems = await _dbContext.Items
                    .Include(item => item.Creator)
                    .ToListAsync();
            }
            else if (userId != null)
            {
                // Fetch the user's group(s) from the UserGroups table
                var userGroups = await _dbContext.Users
                    .Where(u => u.UserId == int.Parse(userId))
                    .SelectMany(u => u.UserGroups.Select(ug => ug.GroupId))
                    .ToListAsync();

                // Populate UserGroups for the dropdown
                UserGroups = await _dbContext.Groups
                    .Where(g => userGroups.Contains(g.GroupId)) // Get groups that user belongs to
                    .ToListAsync();

                // Regular users can only see posts from their group or public posts
                DataItems = await _dbContext.Items
                    .Include(item => item.Creator)
                    .Where(item => userGroups.Contains(item.GroupId))
                    .ToListAsync();
            }
            else
            {
                DataItems = new List<Item>();
            }

            // Read file contents
            FileContents = new List<string>();
            foreach (var item in DataItems)
            {
                if (!string.IsNullOrEmpty(item.FileName))
                {
                    string filePath = Path.Combine(_environment.WebRootPath, "uploads", item.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        FileContents.Add(System.IO.File.ReadAllText(filePath));
                    }
                    else
                    {
                        FileContents.Add("File not found.");
                    }
                }
                else
                {
                    FileContents.Add("No associated file content.");
                }
            }
        }



        public async Task<IActionResult> OnGetAccessItemAsync(int id)
        {
            var item = await _dbContext.Items.FindAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            // Update the LastAccessed field
            item.LastAccessedAt = DateTime.UtcNow;

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            // Add logic to return the file or whatever you need to do here
            return File(item.FilePath, "application/octet-stream", Path.GetFileName(item.FilePath));
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Find the item in the database
            var item = await _dbContext.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Check if the user is authorized to delete the item
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("Account/Login");
            }

            int userId = int.Parse(userIdClaim.Value);
            var isAdmin = User.IsInRole("admin");

            // User not authorized to delete the item
            if (item.CreatorId != userId && !isAdmin)
            {
                return Forbid(); 
            }

            // Remove the item from the database
            _dbContext.Items.Remove(item);
            await _dbContext.SaveChangesAsync();

            // Redirect to the MainFeed page after deletion
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetViewItemAsync(int id)
        {
            try
            {
                var item = await _dbContext.Items.FindAsync(id);

                if (item == null)
                {
                    return new JsonResult(new { error = "Item not found" });
                }

                var metadata = new
                {
                    itemName = item.ItemName,
                    fileSize = item.FileSize,
                    fileType = item.FileType,
                    fileName = item.FileName,
                    createdAt = item.CreatedAt
                };

                return new JsonResult(metadata);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }



        public async Task<IActionResult> OnPostAsync()
        {
            string fileName = null;
            string fileType = null;
            long fileSize = 0;

            // Handle file upload
            if (Upload != null && Upload.Length > 0)
            {
                fileName = Path.GetFileName(Upload.FileName);
                fileType = Upload.ContentType;
                fileSize = Upload.Length;

                var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("Account/Login");
            }

            var userId = int.Parse(userIdClaim.Value);

            // Retrieve the user with their associated groups
            var user = await _dbContext.Users
                .Include(u => u.UserGroups)
                .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Extract GroupId from UserGroups
            int groupId = user.UserGroups.FirstOrDefault()?.GroupId ?? 0;

            if (groupId == 0)
            {
                // If no group is found, assign the "NoGroup" or create it if needed
                var noGroup = await _dbContext.Groups.FirstOrDefaultAsync(g => g.GroupName == "NoGroup");

                if (noGroup == null)
                {
                    noGroup = new Group { GroupName = "NoGroup" };
                    _dbContext.Groups.Add(noGroup);
                    await _dbContext.SaveChangesAsync();
                }

                groupId = noGroup.GroupId;
            }

            // Create a new item
            var newItem = new Item
            {
                ItemName = ItemName,
                ItemDescription = ItemDescription,
                FileName = fileName,
                FileType = fileType,
                FileSize = fileSize,
                FilePath = fileName,
                GroupId = groupId,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Items.Add(newItem);
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }

    }
}
