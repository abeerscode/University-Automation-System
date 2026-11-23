using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Student;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Student.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Student.Controllers;
[Area("Student"), HasPermission("section.change")]
public sealed class SectionChangesController(ISectionChangeFacade changes,
    IStudentFacade students, IBatchFacade batches) : Controller
{
    [HttpGet] public async Task<IActionResult> Create(string id)
    {
        await LoadSectionsAsync(id);
        return View(new SectionChangeForm { StudentId = id });
    }
    [HttpPost] public async Task<IActionResult> Create(SectionChangeForm form)
    {
        if (ModelState.IsValid) try
        {
            await changes.ChangeAsync(new SectionChangeInput(form.StudentId,
                form.SectionId, form.Reason, form.ApprovalReference));
            TempData["Message"] = "Section transfer and placement history recorded.";
            return RedirectToAction("Details", "Students", new { id = form.StudentId });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        await LoadSectionsAsync(form.StudentId);
        return View(form);
    }
    private async Task LoadSectionsAsync(string id)
    {
        var student = await students.GetAsync(id);
        var options = await batches.ListAsync(student.DepartmentId);
        ViewBag.Sections = options.FirstOrDefault(x => x.Id == student.CurrentBatchId)?.Sections;
        ViewBag.StudentName = student.FullName;
    }
}
