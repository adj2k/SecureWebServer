using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecureWebServer.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if the user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }
            else if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/MainFeed");
            }

            // Any additional logic for your index page
            return Page();
        }
    }
}
