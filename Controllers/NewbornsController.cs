// NewbornsController.cs - delivery records, newborn profiles, growth tracking, and vaccine tracker with strict role checks.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize]
public class NewbornsController : Controller
{
    private readonly NewbornService newborns;
    private readonly AuditService audit;

    public NewbornsController(NewbornService newborns, AuditService audit)
    {
        this.newborns = newborns;
        this.audit = audit;
    }

    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> RecordDelivery(int motherId)
    {
        var m = await newborns.GetMotherAsync(motherId);
        if (m == null) return NotFound();
        return View(new DeliveryFormViewModel { MotherId = motherId, MotherName = m.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> RecordDelivery(DeliveryFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        int deliveryId = await newborns.RecordDeliveryAsync(vm);
        await audit.LogAsync(User, "Create", "Delivery", deliveryId, $"Recorded {vm.DeliveryType} delivery for Mother #{vm.MotherId}");
        TempData["Success"] = "Delivery recorded successfully.";
        if (vm.RegisterBabyNext && vm.Outcome != "Stillbirth")
            return RedirectToAction(nameof(Create), new { motherId = vm.MotherId, deliveryId });
        return RedirectToAction("Details", "Mothers", new { id = vm.MotherId });
    }

    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> Create(int motherId, int? deliveryId = null)
    {
        var m = await newborns.GetMotherAsync(motherId);
        if (m == null) return NotFound();
        ViewBag.Deliveries = await newborns.GetDeliveriesForMotherAsync(motherId);
        return View(new NewbornFormViewModel
        {
            MotherId = motherId,
            MotherName = m.Name,
            DeliveryId = deliveryId,
            Name = $"Baby of {m.Name.Split(' ')[0]}",
            Dob = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> Create(NewbornFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Deliveries = await newborns.GetDeliveriesForMotherAsync(vm.MotherId);
            return View(vm);
        }
        int id = await newborns.CreateNewbornAsync(vm);
        await audit.LogAsync(User, "Create", "Newborn", id, $"Registered newborn {vm.Name} ({vm.BirthWeight} kg)");
        TempData["Success"] = $"{vm.Name} registered with 7 UIP vaccine milestones.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var baby = await newborns.GetNewbornDetailsAsync(id);
        if (baby == null) return NotFound();

        if (User.IsInRole(Roles.Mother))
        {
            var myId = User.FindFirstValue("MotherId");
            if (myId != baby.MotherId.ToString()) return Forbid();
        }
        return View(baby);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> Give(int id, DateTime? givenDate, string batchNumber, string returnTo = null, string filter = null)
    {
        var newbornId = await newborns.MarkVaccineGivenAsync(id, givenDate, batchNumber);
        if (newbornId == null) return NotFound();

        await audit.LogAsync(User, "Edit", "Vaccine", id, $"Marked vaccine #{id} given (Batch {batchNumber})");
        TempData["Success"] = "Vaccination marked as given.";
        if (returnTo == "tracker")
            return RedirectToAction(nameof(Vaccines), new { filter });
        return RedirectToAction(nameof(Details), new { id = newbornId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    public async Task<IActionResult> AddGrowth(GrowthRecordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please enter valid weight (0.5–25 kg) and height (30–120 cm).";
            return RedirectToAction(nameof(Details), new { id = vm.NewbornId });
        }
        await newborns.AddGrowthRecordAsync(vm);
        await audit.LogAsync(User, "Create", "GrowthRecord", vm.NewbornId, $"Logged {vm.WeightKg} kg / {vm.HeightCm} cm for Newborn #{vm.NewbornId}");
        TempData["Success"] = "Growth measurement recorded.";
        return RedirectToAction(nameof(Details), new { id = vm.NewbornId });
    }

    [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist")]
    public async Task<IActionResult> Vaccines(string filter = "pending")
    {
        var vm = await newborns.GetVaccineTrackerAsync(filter);
        return View(vm);
    }
}
