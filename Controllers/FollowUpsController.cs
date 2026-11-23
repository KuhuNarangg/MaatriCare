// FollowUpsController.cs - Admin & Receptionist only Today/Overdue/Upcoming follow-up queue with Mark Contacted.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class FollowUpsController : Controller
{
    private readonly ReminderService reminders;
    private readonly AppointmentService appts;
    private readonly AuditService audit;

    public FollowUpsController(ReminderService reminders, AppointmentService appts, AuditService audit)
    {
        this.reminders = reminders;
        this.appts = appts;
        this.audit = audit;
    }

    public async Task<IActionResult> Index(string tab = "overdue") =>
        View(await reminders.GetFollowUpsAsync(tab));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkContacted(string targetKey, string note, string tab = "overdue")
    {
        var fullName = User.FindFirstValue("FullName") ?? User.Identity?.Name ?? "staff";
        await reminders.MarkContactedAsync(targetKey, fullName, note);
        await audit.LogAsync(User, "Create", "FollowUp", null, $"Marked {targetKey} contacted: {note}");
        TempData["Success"] = "Follow-up contact note saved.";
        return RedirectToAction(nameof(Index), new { tab });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAppointment(int id, string tab = "today")
    {
        await appts.SetStatusAsync(id, "Attended", User);
        TempData["Success"] = "Appointment marked as Attended.";
        return RedirectToAction(nameof(Index), new { tab });
    }
}
