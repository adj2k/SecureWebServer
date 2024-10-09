using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SecureWebServer;
using SecureWebServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.LoginPath = "/Account/Login";  // Redirect to login page if not authenticated
    options.Cookie.Name = "MyAuthCookie";  // Name of the authentication cookie
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Set cookie expiration time
    options.SlidingExpiration = true;  // Sliding expiration renews the cookie if user remains active
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;  // Make the cookie HTTP-only to prevent client-side access
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.Redirect(context.RedirectUri ?? "/MainFeed");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
