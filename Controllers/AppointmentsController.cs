// AppointmentsController.cs - book, reschedule, and mark ANC appointments as Attended or Missed.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Doctor,HealthWorker")]
public class AppointmentsController : Controller
{
    private readonly AppointmentService appts;
    public AppointmentsController(AppointmentService appts) { this.appts = appts; }

    public async Task<IActionResult> Index(string status = null)
    {
        ViewBag.Status = status;
        return View(await appts.GetAllAsync(status));
    }

    public async Task<IActionResult> Create(int? motherId = null)
    {
        ViewBag.Mothers = await appts.GetMothersAsync();
        return View(new AppointmentFormViewModel { MotherId = motherId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Mothers = await appts.GetMothersAsync();
            return View(vm);
        }
        await appts.BookAsync(vm);
        TempData["Success"] = "Appointment booked successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Reschedule(int id)
    {
        var a = await appts.GetByIdAsync(id);
        if (a == null) return NotFound();
        ViewBag.MotherName = a.Mother?.Name ?? "";
        return View(new AppointmentFormViewModel
        {
            Id = a.Id,
            MotherId = a.MotherId,
            ScheduledDate = a.ScheduledDate,
            Purpose = a.Purpose,
            Notes = a.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(AppointmentFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (!await appts.RescheduleAsync(vm)) return NotFound();
        TempData["Success"] = "Appointment rescheduled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkStatus(int id, string status, string returnTo = null)
    {
        if (status != "Attended" && status != "Missed") return BadRequest();
        var motherId = await appts.SetStatusAsync(id, status);
        if (motherId == null) return NotFound();

        TempData["Success"] = $"Appointment marked as {status}.";
        if (returnTo == "mother")
            return RedirectToAction("Details", "Mothers", new { id = motherId });
        return RedirectToAction(nameof(Index));
    }
}
