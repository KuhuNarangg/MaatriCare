// DoctorController.cs - Doctor Portal showing today's live appointment queue with auto-polling.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Doctor")]
public class DoctorController : Controller
{
    private readonly AppointmentService appts;
    public DoctorController(AppointmentService appts) { this.appts = appts; }

    public async Task<IActionResult> Index()
    {
        string docUser = User.IsInRole(Roles.Doctor) ? User.Identity?.Name : null;
        return View(await appts.GetQueueTodayAsync(docUser));
    }
}
