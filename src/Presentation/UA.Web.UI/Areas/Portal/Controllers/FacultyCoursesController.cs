using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Faculty;
namespace UA.Web.UI.Areas.Portal.Controllers;
[Area("Portal")]
public sealed class FacultyCoursesController(ITeacherAssignmentFacade assignments) : Controller
{
    public async Task<IActionResult> Index(int? termId) => View(await assignments.OwnAsync(termId, HttpContext.RequestAborted));
}
