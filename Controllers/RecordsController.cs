// RecordsController.cs - views full VisitRecords and renders a clean print-ready prescription sheet.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize]
public class RecordsController : Controller
{
    private readonly ConsultationService consult;
    public RecordsController(ConsultationService consult) { this.consult = consult; }

    public async Task<IActionResult> Details(int id)
    {
        var rec = await consult.GetVisitRecordByIdAsync(id);
        if (rec == null) return NotFound();
        if (User.IsInRole(Roles.Mother) && User.FindFirstValue("MotherId") != rec.MotherId.ToString())
            return Forbid();
        return View(rec);
    }

    public async Task<IActionResult> Print(int id)
    {
        var rec = await consult.GetVisitRecordByIdAsync(id);
        if (rec == null) return NotFound();
        if (User.IsInRole(Roles.Mother) && User.FindFirstValue("MotherId") != rec.MotherId.ToString())
            return Forbid();
        return View(rec);
    }
}
