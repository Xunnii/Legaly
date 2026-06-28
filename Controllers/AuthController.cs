using Legaly.Web.Data;
using Legaly.Web.Models.Entities;
using Legaly.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Legaly.Web.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /auth/login
    [HttpGet("login")]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect("/template");

        return View(new LoginViewModel());
    }

    // POST /auth/login
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email.ToLower() == model.Email.ToLower());

        if (admin == null || !BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
        {
            ViewData["Error"] = "Email atau password salah.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.Email),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Redirect("/template");
    }

    // POST /auth/logout
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/auth/login");
    }
}
