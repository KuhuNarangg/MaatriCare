// HomeController.cs - dashboard with summary numbers.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaatriCare.Data;
using MaatriCare.Services;
namespace MaatriCare.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AppDbContext db;
    public HomeController(AppDbContext db) { this.db = db; }

    public IActionResult Index()
    {
        var mothers = db.Mothers.Include(m => m.Visits).ToList();
        var levels = mothers.Select(m => RiskService.Evaluate(m).Level).ToList();
        ViewBag.Total = mothers.Count;
        ViewBag.Low = levels.Count(l => l == "Low");
        ViewBag.Medium = levels.Count(l => l == "Medium");
        ViewBag.High = levels.Count(l => l == "High");
        ViewBag.Newborns = db.Newborns.Count();
        ViewBag.OverdueVaccines = db.Vaccinations.Where(v => v.GivenDate == null && v.DueDate < DateTime.Today).Count();
        ViewBag.DueSoon = mothers.Where(m => m.Edd >= DateTime.Today && m.Edd <= DateTime.Today.AddDays(30)).ToList();
        return View();
    }
}
