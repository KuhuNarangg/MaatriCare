// NotificationsController.cs - notification centre for simulated SMS reminders with read/unread management.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class NotificationsController : Controller
{
    private readonly ReminderService reminders;
    public NotificationsController(ReminderService reminders) { this.reminders = reminders; }

    public async Task<IActionResult> Index(string filter = "all")
    {
        bool unreadOnly = filter == "unread";
        ViewBag.Filter = unreadOnly ? "unread" : "all";
        ViewBag.UnreadCount = await reminders.GetUnreadCountAsync();
        return View(await reminders.GetNotificationsAsync(unreadOnly));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate()
    {
        int created = await reminders.GenerateRemindersAsync();
        TempData["Success"] = created > 0
            ? $"Generated {created} new simulated SMS reminder(s)."
            : "All upcoming visits, overdue ANC visits, and due vaccines already have reminders for today.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, string filter = "all")
    {
        await reminders.MarkReadAsync(id);
        return RedirectToAction(nameof(Index), new { filter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await reminders.MarkAllReadAsync();
        TempData["Success"] = "All notifications marked as read.";
        return RedirectToAction(nameof(Index));
    }
}
