// NurseController.cs - Nurse Portal showing today's live patient queue for vitals, nurse notes, and vaccines.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Nurse")]
public class NurseController : Controller
{
    private readonly AppointmentService appts;
    public NurseController(AppointmentService appts) { this.appts = appts; }

    public async Task<IActionResult> Index() =>
        View(await appts.GetQueueTodayAsync(null));
}
