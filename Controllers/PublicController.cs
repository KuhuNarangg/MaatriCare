// PublicController.cs - serves all public-facing website pages and handles the Contact form.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MaatriCare.Services;
using MaatriCare.ViewModels;
namespace MaatriCare.Controllers;

[AllowAnonymous]
public class PublicController : Controller
{
    private readonly ContactService contact;
    public PublicController(ContactService contact) { this.contact = contact; }

    [HttpGet("/")]
    public IActionResult Index() => View();

    [HttpGet("/About")]
    public IActionResult About() => View();

    [HttpGet("/Services")]
    public IActionResult Services() => View();

    [HttpGet("/HowItWorks")]
    public IActionResult HowItWorks() => View();

    [HttpGet("/HealthTips")]
    public IActionResult HealthTips() => View();

    [HttpGet("/Faq")]
    public IActionResult Faq() => View();

    [HttpGet("/Contact")]
    public IActionResult Contact() => View(new ContactViewModel());

    [HttpPost("/Contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await contact.SaveMessageAsync(vm);
        TempData["ContactSuccess"] = "Thank you for reaching out! Our team has received your message and will reply soon.";
        return RedirectToAction(nameof(Contact));
    }
}
