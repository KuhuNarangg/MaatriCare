// AccountController.cs - login and logout using cookie authentication.
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Data;
namespace MaatriCare.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AppDbContext db;
    public AccountController(AppDbContext db) { this.db = db; }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var hash = DbSeeder.Hash(password ?? "");
        var u = db.Users.FirstOrDefault(x => x.Username == username && x.PasswordHash == hash);
        if (u == null) { ViewBag.Error = "Wrong username or password"; return View(); }
        var claims = new List<Claim> { new(ClaimTypes.Name, u.Username), new(ClaimTypes.Role, u.Role) };
        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Login");
    }
}
