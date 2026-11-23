// AppointmentsController.cs - forwards appointment routes to ReceptionController (Admin & Receptionist only).
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MaatriCare.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class AppointmentsController : Controller
{
    public IActionResult Index(string status = "") => RedirectToAction("Index", "Reception", new { status });
    public IActionResult Create(int? motherId = null) => RedirectToAction("Book", "Reception", new { motherId });
    public IActionResult Reschedule(int id) => RedirectToAction("Reschedule", "Reception", new { id });
}
