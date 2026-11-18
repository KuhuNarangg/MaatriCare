// MothersController.cs - register mothers, search, view details, add ANC visits.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Models;
namespace MaatriCare.Controllers;

[Authorize]
public class MothersController : Controller
{
    private readonly AppDbContext db;
    public MothersController(AppDbContext db) { this.db = db; }

    public IActionResult Index(string q)
    {
        var list = db.Mothers.Include(m => m.Visits).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) list = list.Where(m => m.Name.Contains(q) || m.Village.Contains(q));
        ViewBag.Q = q;
        return View(list.OrderBy(m => m.Name).ToList());
    }

    public IActionResult Create() => View(new Mother { Lmp = DateTime.Today.AddDays(-60) });

    [HttpPost]
    public IActionResult Create(Mother m)
    {
        if (!ModelState.IsValid) return View(m);
        db.Mothers.Add(m);
        db.SaveChanges();
        return RedirectToAction("Details", new { id = m.Id });
    }

    public IActionResult Details(int id)
    {
        var m = db.Mothers.Include(x => x.Visits).Include(x => x.Newborns).FirstOrDefault(x => x.Id == id);
        return m == null ? NotFound() : View(m);
    }

    public IActionResult AddVisit(int id) => View(new AncVisit { MotherId = id, VisitDate = DateTime.Today });

    [HttpPost]
    public IActionResult AddVisit(AncVisit v)
    {
        db.AncVisits.Add(v);
        db.SaveChanges();
        return RedirectToAction("Details", new { id = v.MotherId });
    }
}
