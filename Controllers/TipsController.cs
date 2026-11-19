// TipsController.cs - Admin CRUD for trimester-based maternal & newborn health education tips.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin")]
public class TipsController : Controller
{
    private readonly HealthTipService tips;
    public TipsController(HealthTipService tips) { this.tips = tips; }

    public async Task<IActionResult> Index() => View(await tips.GetAllAsync());

    public IActionResult Create() => View(new HealthTipFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HealthTipFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await tips.CreateAsync(vm);
        TempData["Success"] = "Health tip published.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var t = await tips.GetByIdAsync(id);
        if (t == null) return NotFound();
        return View(new HealthTipFormViewModel
        {
            Id = t.Id,
            Title = t.Title,
            Category = t.Category,
            Trimester = t.Trimester,
            Content = t.Content,
            IsPublished = t.IsPublished
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HealthTipFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (!await tips.UpdateAsync(vm)) return NotFound();
        TempData["Success"] = "Health tip updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await tips.DeleteAsync(id)) return NotFound();
        TempData["Success"] = "Health tip deleted.";
        return RedirectToAction(nameof(Index));
    }
}
