using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecureWebServer.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // Sign the user out
            await HttpContext.SignOutAsync("MyCookieAuth");

            // Clear the cookie
            HttpContext.Response.Cookies.Delete("MyAuthCookie");

            // Redirect to login page after logout
            return RedirectToPage("/Account/Login");
        }
    }
}
