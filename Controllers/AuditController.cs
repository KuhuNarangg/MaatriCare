// AuditController.cs - Admin-only audit trail showing every create, edit, and delete across MaatriCare.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly AuditService audit;
    public AuditController(AuditService audit) { this.audit = audit; }

    public async Task<IActionResult> Index(string role = "", string entityType = "")
    {
        ViewBag.Role = role;
        ViewBag.EntityType = entityType;
        return View(await audit.GetLogsAsync(role, entityType));
    }
}
