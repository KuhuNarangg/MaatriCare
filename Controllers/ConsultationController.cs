// ConsultationController.cs - unified Doctor/Nurse Consultation Portal, server-enforced saves, and live queue JSON.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")]
public class ConsultationController : Controller
{
    private readonly ConsultationService consult;
    private readonly AppointmentService appts;

    public ConsultationController(ConsultationService consult, AppointmentService appts)
    {
        this.consult = consult;
        this.appts = appts;
    }

    [HttpGet]
    public async Task<IActionResult> Open(int id)
    {
        var vm = await consult.OpenConsultationAsync(id, User);
        return vm == null ? NotFound() : View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ConsultationPostModel post)
    {
        var (ok, forbidden, message, _) = await consult.SaveConsultationAsync(post, User);
        if (forbidden) return Forbid();
        if (!ok)
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Open), new { id = post.AppointmentId });
        }

        TempData["Success"] = message;
        if (post.SubmitMode == "submit_next")
        {
            string docUser = User.IsInRole(Roles.Doctor) ? User.Identity?.Name : null;
            var nextId = await appts.GetNextQueueAppointmentIdAsync(post.AppointmentId, docUser);
            if (nextId.HasValue) return RedirectToAction(nameof(Open), new { id = nextId.Value });
            TempData["Success"] = "Consultation submitted. No more waiting patients in today's queue!";
            return RedirectToAction("Index", User.IsInRole(Roles.Doctor) ? "Doctor" : "Home");
        }

        if (post.SubmitMode == "submit")
            return RedirectToAction("Index", User.IsInRole(Roles.Doctor) ? "Doctor" : "Home");

        return RedirectToAction(nameof(Open), new { id = post.AppointmentId });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> QueueJson(string mode = "doctor")
    {
        string docUser = (mode == "doctor" && User.IsInRole(Roles.Doctor)) ? User.Identity?.Name : null;
        var list = await appts.GetQueueTodayAsync(docUser);
        var payload = list.Select(a => new
        {
            id = a.Id,
            patient = a.PatientDisplayName,
            village = a.Mother?.Village ?? "",
            date = a.ScheduledDate.ToString("dd MMM yyyy"),
            time = a.StartTime,
            duration = a.DurationMinutes,
            reason = a.Purpose,
            status = a.Status,
            doctor = a.DoctorName,
            isNow = a.IsLiveNow,
            isWaiting = a.Status == "Waiting",
            openUrl = Url.Action("Open", "Consultation", new { id = a.Id })
        });
        return Json(payload);
    }
}
