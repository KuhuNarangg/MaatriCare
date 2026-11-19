// ErrorController.cs - renders friendly error pages such as 404 Not Found.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MaatriCare.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("Error/{statusCode:int}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        Response.StatusCode = statusCode;
        if (statusCode == 404) return View("NotFound");
        ViewBag.StatusCode = statusCode;
        return View("NotFound");
    }
}
