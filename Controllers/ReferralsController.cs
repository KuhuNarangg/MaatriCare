// ReferralsController.cs - Admin & Receptionist only hospital referrals list and status updates.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class ReferralsController : Controller
{
    private readonly ReferralService referrals;
    public ReferralsController(ReferralService referrals) { this.referrals = referrals; }

    public async Task<IActionResult> Index(string status = null)
    {
        ViewBag.Status = status;
        return View(await referrals.GetAllAsync(status));
    }

    public async Task<IActionResult> Create(int? motherId = null, string reason = null)
    {
        ViewBag.Mothers = await referrals.GetMothersAsync();
        return View(new ReferralFormViewModel
        {
            MotherId = motherId ?? 0,
            Reason = reason ?? ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReferralFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Mothers = await referrals.GetMothersAsync();
            return View(vm);
        }
        await referrals.CreateAsync(vm, User);
        TempData["Success"] = "Hospital referral created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        if (status != "Pending" && status != "Accepted" && status != "Completed") return BadRequest();
        if (!await referrals.UpdateStatusAsync(id, status, User)) return NotFound();
        TempData["Success"] = $"Referral marked as {status}.";
        return RedirectToAction(nameof(Index));
    }
}
