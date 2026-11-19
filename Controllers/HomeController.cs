// HomeController.cs - serves the role-based dashboard using DashboardService.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly DashboardService dashboard;
    public HomeController(DashboardService dashboard) { this.dashboard = dashboard; }

    public async Task<IActionResult> Index()
    {
        var username = User.Identity?.Name ?? "";
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Admin";
        var vm = await dashboard.BuildDashboardAsync(username, role);
        return View(vm);
    }
}
