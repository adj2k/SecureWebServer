

namespace SecureWebServer.Controller
{
    using Microsoft.AspNetCore.Mvc;

    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "password")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Invalid credentials";
                return View();
            }
        }
    }
}
