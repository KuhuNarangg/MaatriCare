// ReferralService.cs - manages hospital referrals for high-risk mothers and status transitions.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class ReferralService
{
    private readonly AppDbContext db;
    public ReferralService(AppDbContext db) { this.db = db; }

    public async Task<List<Referral>> GetAllAsync(string status = null)
    {
        var q = db.Referrals.Include(r => r.Mother).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status == status);
        return await q.OrderByDescending(r => r.ReferredDate).ToListAsync();
    }

    public async Task<List<Mother>> GetMothersAsync() =>
        await db.Mothers.OrderBy(m => m.Name).ToListAsync();

    public async Task CreateAsync(ReferralFormViewModel vm)
    {
        db.Referrals.Add(new Referral
        {
            MotherId = vm.MotherId,
            HospitalName = vm.HospitalName.Trim(),
            Reason = vm.Reason.Trim(),
            Priority = vm.Priority ?? "Urgent",
            Status = "Pending",
            ReferredDate = vm.ReferredDate,
            Notes = vm.Notes?.Trim() ?? ""
        });
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var r = await db.Referrals.FindAsync(id);
        if (r == null) return false;
        r.Status = status;
        await db.SaveChangesAsync();
        return true;
    }
}
