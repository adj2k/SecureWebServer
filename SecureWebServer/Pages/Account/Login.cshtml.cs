namespace SecureWebServer.Pages
{
    using global::SecureWebServer.Data;
    using global::SecureWebServer.Helper;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ReturnUrl { get; set; }

         public IActionResult OnGet()
    {
        // If user is already authenticated, redirect to the home page
        if (User.Identity.IsAuthenticated)
        {
            // Redirect to Dashboard if already logged in
            return RedirectToPage("/Index"); 
        }

        return Page();
    }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == Username);

            
            if (user == null || user.PasswordHash != PasswordHasher.HashPassword(Password))
            {
                ViewData["Error"] = "Invalid username or password";
                return Page();
            }

            if (user == null || !user.IsApproved)
            {
                ViewData["Error"] = "Your account is pending approval.";
                return Page();
            }

            // Create user identity (claims)
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role),
            // Add NameIdentifier as UserId
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) 
        };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

            // Define authentication properties, setting IsPersistent based on the session duration
            var authProperties = new AuthenticationProperties
            {
                // Set to false for session-based (non-persistent) cookie
                IsPersistent = false, 
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                // Allow cookie to be refreshed when sliding expiration is enabled
                AllowRefresh = true
            };

            // Sign in user with claims
            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity));
            // Redirect to ReturnUrl or home page after successful login
            if (string.IsNullOrEmpty(ReturnUrl))
            {
                ReturnUrl = Url.Page("/MainFeed"); // Default redirect to Index (home page)
            }


            return LocalRedirect(ReturnUrl); // Safely redirect to specified URL or home page

        }
    }

}
