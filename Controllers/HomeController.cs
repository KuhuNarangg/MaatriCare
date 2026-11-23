// HomeController.cs - serves Admin and Mother dashboards; redirects Doctor, Nurse, and Receptionist to their own portals.
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
        var role = PermissionService.GetRole(User);

        // Send each role strictly to its own portal/dashboard
        if (role == Roles.Doctor) return RedirectToAction("Index", "Doctor");
        if (role == Roles.Nurse) return RedirectToAction("Index", "Nurse");
        if (role == Roles.Receptionist) return RedirectToAction("Index", "Reception");

        var vm = await dashboard.BuildDashboardAsync(username, role);
        return View(vm);
    }
}
