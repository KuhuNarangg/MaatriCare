// NewbornsController.cs - register a baby (auto vaccine schedule) and mark vaccines given.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize]
public class NewbornsController : Controller
{
    private readonly AppDbContext db;
    public NewbornsController(AppDbContext db) { this.db = db; }

    public IActionResult Create(int motherId) => View(new Newborn { MotherId = motherId, Dob = DateTime.Today });

    [HttpPost]
    public IActionResult Create(Newborn n)
    {
        ModelState.Remove("Mother");
        if (!ModelState.IsValid) return View(n);
        n.Vaccines = VaccineService.Schedule(n.Dob);   // due dates calculated from date of birth
        db.Newborns.Add(n);
        db.SaveChanges();
        return RedirectToAction("Details", new { id = n.Id });
    }

    public IActionResult Details(int id)
    {
        var n = db.Newborns.Include(x => x.Mother).Include(x => x.Vaccines).FirstOrDefault(x => x.Id == id);
        return n == null ? NotFound() : View(n);
    }

    [HttpPost]
    public IActionResult Give(int id)
    {
        var v = db.Vaccinations.Find(id);
        v.GivenDate = DateTime.Today;
        db.SaveChanges();
        return RedirectToAction("Details", new { id = v.NewbornId });
    }
}
