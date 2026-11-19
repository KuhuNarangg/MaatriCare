// MothersController.cs - filtered Mothers directory, full CRUD for mothers and ANC visits, and pregnancy details.
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[Authorize]
public class MothersController : Controller
{
    private readonly MotherService mothers;
    public MothersController(MotherService mothers) { this.mothers = mothers; }

    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> Index(string q, string village, string risk, int? trimester, string sort = "name", int page = 1)
    {
        var vm = await mothers.GetMothersListAsync(q, village, risk, trimester, sort, page, pageSize: 8);
        return View(vm);
    }

    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public IActionResult Create() => View(new MotherFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> Create(MotherFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        int id = await mothers.CreateMotherAsync(vm);
        TempData["Success"] = $"{vm.Name} has been registered.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> Edit(int id)
    {
        var m = await mothers.GetMotherByIdAsync(id);
        if (m == null) return NotFound();
        return View(new MotherFormViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Age = m.Age,
            Village = m.Village,
            Phone = m.Phone,
            BloodGroup = m.BloodGroup,
            PreviousCSection = m.PreviousCSection,
            PreviousComplications = m.PreviousComplications,
            Lmp = m.Lmp
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> Edit(MotherFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (!await mothers.UpdateMotherAsync(vm)) return NotFound();
        TempData["Success"] = $"Updated details for {vm.Name}.";
        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await mothers.DeleteMotherAsync(id)) return NotFound();
        TempData["Success"] = "Mother record deleted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        if (User.IsInRole("Mother"))
        {
            var myId = User.FindFirstValue("MotherId");
            if (myId != id.ToString()) return Forbid();
        }
        var vm = await mothers.GetMotherDetailsAsync(id);
        return vm == null ? NotFound() : View(vm);
    }

    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> AddVisit(int id)
    {
        var m = await mothers.GetMotherByIdAsync(id);
        if (m == null) return NotFound();
        return View(new AncVisitFormViewModel
        {
            MotherId = id,
            MotherName = m.Name,
            VisitDate = DateTime.Today,
            NextVisitDate = DateTime.Today.AddDays(30)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> AddVisit(AncVisitFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await mothers.AddVisitAsync(vm);
        TempData["Success"] = "ANC visit recorded and risk score updated.";
        return RedirectToAction(nameof(Details), new { id = vm.MotherId });
    }

    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> EditVisit(int id)
    {
        var v = await mothers.GetVisitByIdAsync(id);
        if (v == null) return NotFound();
        var m = await mothers.GetMotherByIdAsync(v.MotherId);
        return View(new AncVisitFormViewModel
        {
            Id = v.Id,
            MotherId = v.MotherId,
            MotherName = m?.Name ?? "",
            VisitDate = v.VisitDate,
            Systolic = v.Systolic,
            Diastolic = v.Diastolic,
            Hb = v.Hb,
            BloodSugar = v.BloodSugar,
            WeightKg = v.WeightKg,
            UrineProtein = v.UrineProtein ?? "Nil",
            FetalHeartbeat = v.FetalHeartbeat,
            NextVisitDate = v.NextVisitDate,
            Notes = v.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> EditVisit(AncVisitFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var motherId = await mothers.UpdateVisitAsync(vm);
        if (motherId == null) return NotFound();
        TempData["Success"] = "ANC visit updated.";
        return RedirectToAction(nameof(Details), new { id = motherId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Doctor,HealthWorker")]
    public async Task<IActionResult> DeleteVisit(int id)
    {
        var motherId = await mothers.DeleteVisitAsync(id);
        if (motherId == null) return NotFound();
        TempData["Success"] = "ANC visit deleted.";
        return RedirectToAction(nameof(Details), new { id = motherId });
    }
}
