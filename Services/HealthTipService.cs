// HealthTipService.cs - Admin CRUD for health education tips and trimester filtering for Mother dashboards.
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.ViewModels;
namespace MaatriCare.Services;

public class HealthTipService
{
    private readonly AppDbContext db;
    public HealthTipService(AppDbContext db) { this.db = db; }

    public async Task<List<HealthTip>> GetAllAsync() =>
        await db.HealthTips.OrderBy(t => t.Trimester).ThenByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<List<HealthTip>> GetForTrimesterAsync(int trimester) =>
        await db.HealthTips
            .Where(t => t.IsPublished && (t.Trimester == trimester || t.Trimester == 0))
            .OrderByDescending(t => t.Trimester == trimester)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<HealthTip> GetByIdAsync(int id) =>
        await db.HealthTips.FindAsync(id);

    public async Task CreateAsync(HealthTipFormViewModel vm)
    {
        db.HealthTips.Add(new HealthTip
        {
            Title = vm.Title.Trim(),
            Category = vm.Category.Trim(),
            Trimester = vm.Trimester,
            Content = vm.Content.Trim(),
            IsPublished = vm.IsPublished,
            CreatedAt = DateTime.Today
        });
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(HealthTipFormViewModel vm)
    {
        var tip = await db.HealthTips.FindAsync(vm.Id);
        if (tip == null) return false;
        tip.Title = vm.Title.Trim();
        tip.Category = vm.Category.Trim();
        tip.Trimester = vm.Trimester;
        tip.Content = vm.Content.Trim();
        tip.IsPublished = vm.IsPublished;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tip = await db.HealthTips.FindAsync(id);
        if (tip == null) return false;
        db.HealthTips.Remove(tip);
        await db.SaveChangesAsync();
        return true;
    }
}
