using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Faculty;
namespace UA.Web.UI.Areas.Portal.Controllers;
[Area("Portal")]
public sealed class FacultyDashboardController(IFacultyPortalFacade portal, IFacultyFacade faculty) : Controller
{
    public async Task<IActionResult> Index() => View(await portal.DashboardAsync(HttpContext.RequestAborted));
    [HttpGet]
    public async Task<IActionResult> Photo()
    {
        var own = await portal.DashboardAsync(HttpContext.RequestAborted);
        var photo = await faculty.PhotoAsync(own.Profile.Id, HttpContext.RequestAborted);
        Response.Headers["Cache-Control"] = "private, no-store";
        return File(photo.Data, photo.ContentType);
    }
}
