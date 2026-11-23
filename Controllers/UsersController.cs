// UsersController.cs - Admin-only user management, role assignment, village mapping, and deactivation.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserService users;
    public UsersController(UserService users) { this.users = users; }

    public async Task<IActionResult> Index()
    {
        ViewBag.Mothers = (await users.GetMothersForSelectAsync()).ToDictionary(m => m.Id, m => m.Name);
        return View(await users.GetAllUsersAsync());
    }

    public async Task<IActionResult> Create()
    {
        await LoadFormLookupsAsync();
        return View(new UserFormViewModel { Role = "Nurse", IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadFormLookupsAsync();
            return View(vm);
        }

        var error = await users.CreateUserAsync(vm);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            await LoadFormLookupsAsync();
            return View(vm);
        }

        TempData["Success"] = $"Account '{vm.Username}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var user = await users.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        await LoadFormLookupsAsync();
        var vm = new UserFormViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            SelectedVillages = (user.AssignedVillages ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            MotherId = user.MotherId
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadFormLookupsAsync();
            return View(vm);
        }

        var error = await users.UpdateUserAsync(vm);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            await LoadFormLookupsAsync();
            return View(vm);
        }

        TempData["Success"] = $"Updated user '{vm.Username}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var (ok, message) = await users.ToggleActiveAsync(id, User.Identity?.Name ?? "");
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFormLookupsAsync()
    {
        ViewBag.Villages = await users.GetAllVillagesAsync();
        ViewBag.Mothers = await users.GetMothersForSelectAsync();
    }
}
