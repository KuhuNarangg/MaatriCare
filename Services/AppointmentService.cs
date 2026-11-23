// AppointmentService.cs - manages Reception Desk scheduling, status workflow, live queues, and Live Portal.
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class AppointmentService
{
    private readonly AppDbContext db;
    private readonly AuditService audit;

    public AppointmentService(AppDbContext db, AuditService audit)
    {
        this.db = db;
        this.audit = audit;
    }

    public async Task<List<AppUser>> GetDoctorsAsync() =>
        await db.Users.Where(u => u.Role == Roles.Doctor && u.IsActive).OrderBy(u => u.FullName).ToListAsync();

    public async Task<List<Mother>> GetMothersAsync() =>
        await db.Mothers.OrderBy(m => m.Name).ToListAsync();

    public async Task<List<Newborn>> GetNewbornsAsync() =>
        await db.Newborns.Include(n => n.Mother).OrderBy(n => n.Name).ToListAsync();

    public async Task<ReceptionDeskViewModel> GetReceptionDeskAsync(string range = "today", string status = "", string doctor = "")
    {
        var today = DateTime.Today;
        var all = await db.Appointments
            .Include(a => a.Mother)
            .Include(a => a.Newborn)
            .ToListAsync();

        var q = all.AsEnumerable();
        range = string.IsNullOrWhiteSpace(range) ? "today" : range.ToLower();
        if (range == "today") q = q.Where(a => a.ScheduledDate.Date == today);
        else if (range == "week") q = q.Where(a => a.ScheduledDate.Date >= today && a.ScheduledDate.Date <= today.AddDays(7));

        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(doctor)) q = q.Where(a => a.DoctorUsername == doctor);

        return new ReceptionDeskViewModel
        {
            Range = range,
            Status = status ?? "",
            Doctor = doctor ?? "",
            Doctors = await GetDoctorsAsync(),
            Appointments = q.OrderBy(a => a.ScheduledDate).ThenBy(a => a.StartTime).ToList(),
            TodaysCalendar = all.Where(a => a.ScheduledDate.Date == today && a.Status != "Cancelled")
                .OrderBy(a => a.StartTime)
                .ToList()
        };
    }

    public async Task<Appointment> GetByIdAsync(int id) =>
        await db.Appointments.Include(a => a.Mother).Include(a => a.Newborn).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<int> BookAsync(AppointmentFormViewModel vm, ClaimsPrincipal user)
    {
        var doc = await db.Users.FirstOrDefaultAsync(u => u.Username == vm.DoctorUsername);
        var appt = new Appointment
        {
            MotherId = vm.MotherId,
            NewbornId = vm.NewbornId,
            DoctorUsername = vm.DoctorUsername ?? "doctor",
            DoctorName = doc != null && !string.IsNullOrWhiteSpace(doc.FullName) ? doc.FullName : "Dr. Manpreet Gill",
            ScheduledDate = vm.ScheduledDate.Date,
            StartTime = string.IsNullOrWhiteSpace(vm.StartTime) ? "10:00" : vm.StartTime.Trim(),
            DurationMinutes = vm.DurationMinutes > 0 ? vm.DurationMinutes : 20,
            Purpose = vm.Purpose.Trim(),
            Status = "Scheduled",
            CreatedBy = user?.Identity?.Name ?? "receptionist",
            Notes = vm.Notes?.Trim() ?? ""
        };
        db.Appointments.Add(appt);
        await db.SaveChangesAsync();
        await audit.LogAsync(user, "Create", "Appointment", appt.Id, $"Booked {appt.Purpose} on {appt.ScheduledDate:dd MMM} at {appt.StartTime} with {appt.DoctorName}");
        return appt.Id;
    }

    public async Task<(int MotherId, int AppointmentId)> RegisterAndBookAsync(RegisterAndBookViewModel vm, ClaimsPrincipal user)
    {
        var mother = new Mother
        {
            Name = vm.Name.Trim(),
            Age = vm.Age,
            Village = vm.Village.Trim(),
            Phone = vm.Phone.Trim(),
            BloodGroup = vm.BloodGroup?.Trim() ?? "",
            Lmp = vm.Lmp,
            PreviousCSection = vm.PreviousCSection,
            RegisteredAt = DateTime.Today
        };
        db.Mothers.Add(mother);
        await db.SaveChangesAsync();
        await audit.LogAsync(user, "Create", "Mother", mother.Id, $"Registered mother {mother.Name} ({mother.Village}) via Reception Desk");

        var doc = await db.Users.FirstOrDefaultAsync(u => u.Username == vm.DoctorUsername);
        var appt = new Appointment
        {
            MotherId = mother.Id,
            DoctorUsername = vm.DoctorUsername ?? "doctor",
            DoctorName = doc != null && !string.IsNullOrWhiteSpace(doc.FullName) ? doc.FullName : "Dr. Manpreet Gill",
            ScheduledDate = vm.ScheduledDate.Date,
            StartTime = string.IsNullOrWhiteSpace(vm.StartTime) ? DateTime.Now.ToString("HH:mm") : vm.StartTime.Trim(),
            DurationMinutes = vm.DurationMinutes > 0 ? vm.DurationMinutes : 20,
            Purpose = vm.Purpose.Trim(),
            Status = "Scheduled",
            CreatedBy = user?.Identity?.Name ?? "receptionist",
            Notes = vm.Notes?.Trim() ?? ""
        };
        db.Appointments.Add(appt);
        await db.SaveChangesAsync();
        await audit.LogAsync(user, "Create", "Appointment", appt.Id, $"Booked {appt.Purpose} for {mother.Name} at {appt.StartTime}");
        return (mother.Id, appt.Id);
    }

    public async Task<bool> RescheduleAsync(AppointmentFormViewModel vm, ClaimsPrincipal user)
    {
        var a = await db.Appointments.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (a == null) return false;
        var doc = await db.Users.FirstOrDefaultAsync(u => u.Username == vm.DoctorUsername);
        a.ScheduledDate = vm.ScheduledDate.Date;
        a.StartTime = string.IsNullOrWhiteSpace(vm.StartTime) ? a.StartTime : vm.StartTime.Trim();
        a.DurationMinutes = vm.DurationMinutes > 0 ? vm.DurationMinutes : 20;
        a.DoctorUsername = vm.DoctorUsername ?? a.DoctorUsername;
        if (doc != null && !string.IsNullOrWhiteSpace(doc.FullName)) a.DoctorName = doc.FullName;
        a.Purpose = vm.Purpose.Trim();
        a.Notes = vm.Notes?.Trim() ?? "";
        a.Status = "Scheduled";
        await db.SaveChangesAsync();
        await audit.LogAsync(user, "Edit", "Appointment", a.Id, $"Rescheduled appointment #{a.Id} to {a.ScheduledDate:dd MMM} {a.StartTime}");
        return true;
    }

    public async Task<int?> SetStatusAsync(int id, string status, ClaimsPrincipal user)
    {
        var a = await db.Appointments.Include(x => x.Mother).FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return null;
        a.Status = status;
        if (status == "Waiting") a.CheckedInAt = DateTime.Now;
        if (status == "In Consultation" && a.ConsultationStartedAt == null) a.ConsultationStartedAt = DateTime.Now;
        await db.SaveChangesAsync();
        await audit.LogAsync(user, "StatusChange", "Appointment", a.Id, $"Appointment #{a.Id} ({a.Mother?.Name}) marked {status}");
        return a.MotherId;
    }

    public async Task<List<Appointment>> GetQueueTodayAsync(string doctorUsername = null)
    {
        var today = DateTime.Today;
        var q = db.Appointments
            .Include(a => a.Mother).ThenInclude(m => m.Visits)
            .Include(a => a.Newborn)
            .Where(a => a.ScheduledDate.Date == today && a.Status != "Cancelled");

        if (!string.IsNullOrWhiteSpace(doctorUsername))
            q = q.Where(a => a.DoctorUsername == doctorUsername);

        var list = await q.ToListAsync();
        return list
            .OrderByDescending(a => a.Status == "In Consultation")
            .ThenByDescending(a => a.Status == "Waiting")
            .ThenBy(a => a.Status == "Attended" || a.Status == "Missed")
            .ThenBy(a => a.StartTime)
            .ToList();
    }

    public async Task<LivePortalViewModel> GetLivePortalAsync()
    {
        var today = DateTime.Today;
        var todays = await db.Appointments
            .Include(a => a.Mother)
            .Include(a => a.Newborn)
            .Where(a => a.ScheduledDate.Date == today && a.Status != "Cancelled")
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return new LivePortalViewModel
        {
            TotalToday = todays.Count,
            AttendedToday = todays.Count(a => a.Status == "Attended"),
            MissedToday = todays.Count(a => a.Status == "Missed"),
            WaitingCount = todays.Count(a => a.Status == "Waiting"),
            InConsultationCount = todays.Count(a => a.Status == "In Consultation"),
            LiveNow = todays.Where(a => a.IsLiveNow || a.Status == "In Consultation").ToList(),
            Waiting = todays.Where(a => a.Status == "Waiting").ToList(),
            InConsultation = todays.Where(a => a.Status == "In Consultation").ToList(),
            CompletedToday = todays.Where(a => a.Status == "Attended").ToList(),
            MissedTodayList = todays.Where(a => a.Status == "Missed").ToList(),
            UpcomingToday = todays.Where(a => a.Status == "Scheduled").ToList()
        };
    }

    public async Task<int?> GetNextQueueAppointmentIdAsync(int currentAppointmentId, string doctorUsername)
    {
        var queue = await GetQueueTodayAsync(doctorUsername);
        var next = queue.FirstOrDefault(a => a.Id != currentAppointmentId && (a.Status == "Waiting" || a.Status == "Scheduled"));
        return next?.Id;
    }
}
