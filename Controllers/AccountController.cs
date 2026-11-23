// AccountController.cs - login, logout, and Access Denied page using cookie authentication.
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AuthService auth;
    public AccountController(AuthService auth) { this.auth = auth; }

    public IActionResult Login(string returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var (action, controller) = auth.GetDashboardRouteForRole(role);
            return RedirectToAction(action, controller);
        }
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var (user, error) = await auth.ValidateUserAsync(vm.Username, vm.Password);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
            new("FullName", string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName)
        };
        if (user.MotherId.HasValue)
            claims.Add(new Claim("MotherId", user.MotherId.Value.ToString()));

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        var (action, controller) = auth.GetDashboardRouteForRole(user.Role);
        return RedirectToAction(action, controller);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();
}
