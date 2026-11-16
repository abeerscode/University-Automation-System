using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace UA.Web.UI.Controllers;
[AllowAnonymous] public sealed class ErrorController : Controller
{
    public IActionResult Index() => View("~/Views/Shared/Error.cshtml");
    public IActionResult NotFound() { Response.StatusCode = 404; return View("~/Views/Shared/NotFound.cshtml"); }
}
