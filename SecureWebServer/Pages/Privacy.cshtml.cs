﻿namespace SecureWebServer.Pages
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    namespace SecureWebServer.Pages
    {
        [Authorize]
        public class PrivacyModel : PageModel
        {
            public void OnGet()
            {
            }
        }
    }

}