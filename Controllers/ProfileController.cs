// ProfileController.cs - allows any signed-in user to update their name, phone, and password.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserService users;
    public ProfileController(UserService users) { this.users = users; }

    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserByUsernameAsync(User.Identity?.Name ?? "");
        if (user == null) return RedirectToAction("Logout", "Account");

        var vm = new ProfileViewModel
        {
            Username = user.Username,
            Role = user.Role,
            FullName = user.FullName,
            Phone = user.Phone
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel vm)
    {
        var username = User.Identity?.Name ?? "";
        if (!ModelState.IsValid)
        {
            var user = await users.GetUserByUsernameAsync(username);
            vm.Username = user?.Username;
            vm.Role = user?.Role;
            return View("Index", vm);
        }

        await users.UpdateProfileAsync(username, vm);
        TempData["ProfileSuccess"] = "Your profile details have been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        var username = User.Identity?.Name ?? "";
        if (!ModelState.IsValid)
        {
            var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                           ?? "Please check your password fields.";
            TempData["PasswordError"] = firstErr;
            return RedirectToAction(nameof(Index));
        }

        var error = await users.ChangePasswordAsync(username, vm);
        if (error != null)
        {
            TempData["PasswordError"] = error;
            return RedirectToAction(nameof(Index));
        }

        TempData["PasswordSuccess"] = "Your password has been changed successfully.";
        return RedirectToAction(nameof(Index));
    }
}
