using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Faculty;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Faculty;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Faculty.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Faculty.Controllers;
[Area("Faculty"), HasPermission("mentors.manage")]
public sealed class MentorsController(IMentorFacade mentors, IDepartmentFacade departments) : Controller
{
    public async Task<IActionResult> Index(int? departmentId)
    {
        ViewBag.Departments = await departments.AvailableAsync("mentors.manage");
        ViewBag.DepartmentId = departmentId;
        IReadOnlyList<MentorAssignmentDto> rows = departmentId.HasValue
            ? await mentors.ListAsync(departmentId.Value) : Array.Empty<MentorAssignmentDto>();
        return View(rows);
    }
    [HttpGet]
    public async Task<IActionResult> Assign(int departmentId)
    {
        ViewBag.Options = await mentors.OptionsAsync(departmentId);
        return View(new MentorAssignForm { DepartmentId = departmentId });
    }
    [HttpPost]
    public async Task<IActionResult> Assign(MentorAssignForm form)
    {
        if (ModelState.IsValid) try
        {
            await mentors.AssignAsync(new MentorSaveDto(form.FacultyId, form.BatchId, form.SectionId));
            TempData["Message"] = "Mentor assigned; previous assignment history preserved.";
            return RedirectToAction(nameof(Index), new { departmentId = form.DepartmentId });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Options = await mentors.OptionsAsync(form.DepartmentId);
        return View(form);
    }
    [HttpPost]
    public async Task<IActionResult> End(int id, int departmentId)
    {
        try { await mentors.EndAsync(id); TempData["Message"] = "Mentor assignment ended."; }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index), new { departmentId });
    }
}
