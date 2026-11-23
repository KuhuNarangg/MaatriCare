// AuditService.cs - records and queries Admin-only audit logs for clinical, appointment, and referral actions.
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
namespace MaatriCare.Services;

public class AuditService
{
    private readonly AppDbContext db;
    public AuditService(AppDbContext db) { this.db = db; }

    public async Task LogAsync(ClaimsPrincipal user, string action, string entityType, int? entityId, string details)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Username = user?.Identity?.Name ?? "system",
            Role = PermissionService.GetRole(user),
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details ?? "",
            Timestamp = DateTime.Now
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetLogsAsync(string role = null, string entityType = null)
    {
        var q = db.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(role)) q = q.Where(a => a.Role == role);
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(a => a.EntityType == entityType);
        return await q.OrderByDescending(a => a.Timestamp).Take(150).ToListAsync();
    }
}
