using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Student;
namespace UA.Web.UI.Areas.Portal.Controllers;
[Area("Portal")]
public sealed class DashboardController(IStudentPortalFacade students) : Controller
{
    public async Task<IActionResult> Index() => View(await students.OwnProfileAsync());
}
