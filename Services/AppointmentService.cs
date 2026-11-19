// AppointmentService.cs - books, reschedules, lists, and updates status on ANC appointments.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class AppointmentService
{
    private readonly AppDbContext db;
    public AppointmentService(AppDbContext db) { this.db = db; }

    public async Task<List<Appointment>> GetAllAsync(string status = null)
    {
        var q = db.Appointments.Include(a => a.Mother).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(a => a.Status == status);
        return await q.OrderBy(a => a.ScheduledDate).ToListAsync();
    }

    public async Task<Appointment> GetByIdAsync(int id) =>
        await db.Appointments.Include(a => a.Mother).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<List<Mother>> GetMothersAsync() =>
        await db.Mothers.OrderBy(m => m.Name).ToListAsync();

    public async Task BookAsync(AppointmentFormViewModel vm)
    {
        db.Appointments.Add(new Appointment
        {
            MotherId = vm.MotherId,
            ScheduledDate = vm.ScheduledDate,
            Purpose = vm.Purpose.Trim(),
            Status = "Scheduled",
            Notes = vm.Notes?.Trim() ?? ""
        });
        await db.SaveChangesAsync();
    }

    public async Task<bool> RescheduleAsync(AppointmentFormViewModel vm)
    {
        var a = await db.Appointments.FirstOrDefaultAsync(x => x.Id == vm.Id);
        if (a == null) return false;
        a.ScheduledDate = vm.ScheduledDate;
        a.Purpose = vm.Purpose.Trim();
        a.Notes = vm.Notes?.Trim() ?? "";
        a.Status = "Scheduled";
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int?> SetStatusAsync(int id, string status)
    {
        var a = await db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return null;
        a.Status = status;
        await db.SaveChangesAsync();
        return a.MotherId;
    }
}
