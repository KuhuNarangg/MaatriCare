// ReferralService.cs - manages hospital referrals for high-risk mothers with creator tracking and audit logging.
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class ReferralService
{
    private readonly AppDbContext db;
    private readonly AuditService audit;

    public ReferralService(AppDbContext db, AuditService audit)
    {
        this.db = db;
        this.audit = audit;
    }

    public async Task<List<Referral>> GetAllAsync(string status = null)
    {
        var q = db.Referrals.Include(r => r.Mother).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status == status);
        return await q.OrderByDescending(r => r.ReferredDate).ThenByDescending(r => r.Id).ToListAsync();
    }

    public async Task<List<Mother>> GetMothersAsync() =>
        await db.Mothers.OrderBy(m => m.Name).ToListAsync();

    public async Task CreateAsync(ReferralFormViewModel vm, ClaimsPrincipal user)
    {
        var fullName = user.FindFirstValue("FullName") ?? user.Identity?.Name ?? "staff";
        var role = PermissionService.GetRole(user);
        var referral = new Referral
        {
            MotherId = vm.MotherId,
            HospitalName = vm.HospitalName.Trim(),
            Reason = vm.Reason.Trim(),
            Priority = vm.Priority ?? "Urgent",
            Status = "Pending",
            ReferredDate = vm.ReferredDate,
            CreatedBy = $"{fullName} ({role})",
            Notes = vm.Notes?.Trim() ?? ""
        };
        db.Referrals.Add(referral);
        await db.SaveChangesAsync();
        await audit.LogAsync(user, "Create", "Referral", referral.Id, $"Referred Mother #{referral.MotherId} to {referral.HospitalName}");
    }

    public async Task<bool> UpdateStatusAsync(int id, string status, ClaimsPrincipal user)
    {
        var r = await db.Referrals.FindAsync(id);
        if (r == null) return false;
        r.Status = status;
        await db.SaveChangesAsync();
        await audit.LogAsync(user, "StatusChange", "Referral", r.Id, $"Referral #{r.Id} status updated to {status}");
        return true;
    }
}
