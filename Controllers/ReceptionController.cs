// ReceptionController.cs - Reception Desk for booking, rescheduling, cancelling, check-in, and combined registration.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class ReceptionController : Controller
{
    private readonly AppointmentService appts;
    public ReceptionController(AppointmentService appts) { this.appts = appts; }

    public async Task<IActionResult> Index(string range = "today", string status = "", string doctor = "") =>
        View(await appts.GetReceptionDeskAsync(range, status, doctor));

    public async Task<IActionResult> Book(int? motherId = null)
    {
        await LoadLookupsAsync();
        return View(new AppointmentFormViewModel { MotherId = motherId ?? 0, ScheduledDate = DateTime.Today, StartTime = DateTime.Now.ToString("HH:mm") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(AppointmentFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return View(vm);
        }
        await appts.BookAsync(vm, User);
        TempData["Success"] = "Appointment booked successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> RegisterAndBook()
    {
        ViewBag.Doctors = await appts.GetDoctorsAsync();
        return View(new RegisterAndBookViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAndBook(RegisterAndBookViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await appts.GetDoctorsAsync();
            return View(vm);
        }
        var (_, apptId) = await appts.RegisterAndBookAsync(vm, User);
        TempData["Success"] = $"Registered {vm.Name} and booked appointment #{apptId} at {vm.StartTime}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Reschedule(int id)
    {
        var a = await appts.GetByIdAsync(id);
        if (a == null) return NotFound();
        ViewBag.Doctors = await appts.GetDoctorsAsync();
        ViewBag.MotherName = a.PatientDisplayName;
        return View(new AppointmentFormViewModel
        {
            Id = a.Id,
            MotherId = a.MotherId,
            NewbornId = a.NewbornId,
            DoctorUsername = a.DoctorUsername,
            ScheduledDate = a.ScheduledDate,
            StartTime = a.StartTime,
            DurationMinutes = a.DurationMinutes,
            Purpose = a.Purpose,
            Notes = a.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(AppointmentFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Doctors = await appts.GetDoctorsAsync();
            return View(vm);
        }
        if (!await appts.RescheduleAsync(vm, User)) return NotFound();
        TempData["Success"] = "Appointment rescheduled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status, string returnTo = null)
    {
        if (status is not ("Waiting" or "Attended" or "Missed" or "Cancelled" or "Scheduled"))
            return BadRequest();
        var motherId = await appts.SetStatusAsync(id, status, User);
        if (motherId == null) return NotFound();

        TempData["Success"] = $"Appointment marked as {status}.";
        if (returnTo == "mother") return RedirectToAction("Details", "Mothers", new { id = motherId });
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Mothers = await appts.GetMothersAsync();
        ViewBag.Newborns = await appts.GetNewbornsAsync();
        ViewBag.Doctors = await appts.GetDoctorsAsync();
    }
}
