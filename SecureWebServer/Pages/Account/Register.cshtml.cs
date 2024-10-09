using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureWebServer.Data;
using SecureWebServer.Helper;
using SecureWebServer.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SecureWebServer.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string FullName { get; set; }
        [BindProperty]
        public string Username { get; set; }
        [BindProperty]
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }
        [BindProperty]
        public string ConfirmPassword { get; set; }
        [BindProperty]
        public bool IsApproved { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Password validation
            if (Password != ConfirmPassword)
            {
                ViewData["Error"] = "Passwords do not match.";
                return Page();
            }

            if (!IsPasswordValid(Password))
            {
                ViewData["Error"] = "Password must be at least 8 characters long, contain at least one number and one special character.";
                return Page();
            }

            // Hash the password before saving it
            var passwordHash = PasswordHasher.HashPassword(Password);

            var user = new User
            {
                FullName = FullName,
                UserName = Username,
                Email = Email,
                PasswordHash = passwordHash,
                Role = "general",
                IsApproved = IsApproved
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Redirect to login page after successful registration
            return RedirectToPage("/Account/Login");
        }

        private bool IsPasswordValid(string password)
        {
            // Check if the password meets the length requirement
            if (password.Length < 8)
            {
                return false;
            }

            // Check if the password contains at least one digit
            if (!Regex.IsMatch(password, @"\d"))
            {
                return false;
            }

            // Check if the password contains at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
            {
                return false;
            }

            return true;
        }
    }
}
