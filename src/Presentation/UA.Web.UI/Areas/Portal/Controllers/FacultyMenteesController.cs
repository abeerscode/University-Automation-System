using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Faculty;
namespace UA.Web.UI.Areas.Portal.Controllers;
[Area("Portal")]
public sealed class FacultyMenteesController(IMentorFacade mentors) : Controller
{
    public async Task<IActionResult> Index() => View(await mentors.OwnMenteesAsync(HttpContext.RequestAborted));
}
