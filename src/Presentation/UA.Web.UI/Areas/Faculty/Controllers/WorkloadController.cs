using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Faculty;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Faculty.Controllers;
[Area("Faculty"), HasPermission("workload.view")]
public sealed class WorkloadController(IFacultyFacade faculty, IDepartmentFacade departments) : Controller
{
    public async Task<IActionResult> Index(int? departmentId, int? termId)
    {
        ViewBag.Departments = await departments.AvailableAsync("workload.view");
        ViewBag.DepartmentId = departmentId; ViewBag.TermId = termId;
        IReadOnlyList<UA.Facade.DTO.Faculty.FacultyWorkloadRowDto> rows = departmentId.HasValue
            ? await faculty.WorkloadAsync(departmentId.Value, termId)
            : Array.Empty<UA.Facade.DTO.Faculty.FacultyWorkloadRowDto>();
        return View(rows);
    }
}
