// FollowUpsController.cs - tabbed Today/Overdue/Upcoming follow-up queue with quick Mark Done actions.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Doctor,HealthWorker")]
public class FollowUpsController : Controller
{
    private readonly ReminderService reminders;
    private readonly AppointmentService appts;
    private readonly NewbornService newborns;

    public FollowUpsController(ReminderService reminders, AppointmentService appts, NewbornService newborns)
    {
        this.reminders = reminders;
        this.appts = appts;
        this.newborns = newborns;
    }

    public async Task<IActionResult> Index(string tab = "today") =>
        View(await reminders.GetFollowUpsAsync(tab));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAppointment(int id, string tab = "today")
    {
        await appts.SetStatusAsync(id, "Attended");
        TempData["Success"] = "Appointment marked as completed.";
        return RedirectToAction(nameof(Index), new { tab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteVaccine(int id, string tab = "today")
    {
        await newborns.MarkVaccineGivenAsync(id, DateTime.Today, "FOLLOWUP");
        TempData["Success"] = "Vaccine marked as given.";
        return RedirectToAction(nameof(Index), new { tab });
    }
}
