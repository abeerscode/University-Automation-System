using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Academic.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Academic.Controllers;
[Area("Academic"), HasPermission("curriculum.manage")]
public sealed class CurriculumController(ICurriculumFacade facade, IDepartmentFacade departments, ICourseFacade courses) : Controller
{
    public async Task<IActionResult> Index(int? departmentId) => View(await facade.ListAsync(departmentId));
    public async Task<IActionResult> Details(int id)
    {
        var c = (await facade.ListAsync()).FirstOrDefault(x => x.Id == id); if (c is null) return NotFound();
        ViewBag.Courses = await courses.ListAsync(c.DepartmentId);
        return View(c);
    }
    public async Task<IActionResult> Create()
    {
        ViewBag.Programs = (await departments.AvailableAsync("curriculum.manage")).Where(x => x.ProgramId is not null).ToArray();
        return View(new CurriculumCreateForm());
    }
    [HttpPost] public async Task<IActionResult> Create(CurriculumCreateForm form)
    {
        if (ModelState.IsValid) try
        {
            int id = await facade.CreateDraftAsync(new CurriculumInput(form.ProgramId, form.EffectiveYear));
            TempData["Message"] = "Draft curriculum created.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Programs = (await departments.AvailableAsync("curriculum.manage")).Where(x => x.ProgramId is not null).ToArray();
        return View(form);
    }
    [HttpPost] public async Task<IActionResult> SetCourse(int id, int courseId, byte semester)
    {
        try { await facade.SetCourseAsync(id, courseId, semester); TempData["Message"] = "Semester course updated."; }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }
    [HttpPost] public async Task<IActionResult> RemoveCourse(int id, int courseId)
    {
        try { await facade.RemoveCourseAsync(id, courseId); TempData["Message"] = "Course removed from draft."; }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }
    [HttpPost] public async Task<IActionResult> Approve(int id, int approvalId)
    {
        try { await facade.ApproveAsync(id, approvalId); TempData["Message"] = "Curriculum approved and activated."; }
        catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }
}
