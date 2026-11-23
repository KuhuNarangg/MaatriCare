// LivePortalController.cs - Admin & Receptionist Live Appointment Portal with 10-second JSON refresh.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class LivePortalController : Controller
{
    private readonly AppointmentService appts;
    public LivePortalController(AppointmentService appts) { this.appts = appts; }

    public async Task<IActionResult> Index() => View(await appts.GetLivePortalAsync());

    [HttpGet]
    public async Task<IActionResult> FeedJson()
    {
        var vm = await appts.GetLivePortalAsync();
        object MapItem(Models.Appointment a)
        {
            int elapsed = a.ConsultationStartedAt.HasValue
                ? Math.Max(0, (int)(DateTime.Now - a.ConsultationStartedAt.Value).TotalMinutes)
                : Math.Max(0, (int)(DateTime.Now - a.SlotStart).TotalMinutes);
            return new
            {
                id = a.Id,
                patient = a.PatientDisplayName,
                village = a.Mother?.Village ?? "",
                doctor = a.DoctorName,
                time = a.StartTime,
                reason = a.Purpose,
                status = a.Status,
                isLive = a.IsLiveNow || a.Status == "In Consultation",
                elapsedMinutes = elapsed,
                openUrl = Url.Action("Open", "Consultation", new { id = a.Id })
            };
        }

        return Json(new
        {
            totalToday = vm.TotalToday,
            attendedToday = vm.AttendedToday,
            missedToday = vm.MissedToday,
            waitingCount = vm.WaitingCount,
            inConsultationCount = vm.InConsultationCount,
            liveNow = vm.LiveNow.Select(MapItem),
            waiting = vm.Waiting.Select(MapItem),
            inConsultation = vm.InConsultation.Select(MapItem),
            completedToday = vm.CompletedToday.Select(MapItem),
            missedTodayList = vm.MissedTodayList.Select(MapItem),
            upcomingToday = vm.UpcomingToday.Select(MapItem)
        });
    }
}
