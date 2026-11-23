using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Student;
using UA.Framework.Exceptions;
using UA.Framework.Time;
using UA.Web.UI.Areas.Student.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Student.Controllers;
[Area("Student"), HasPermission("status.change")]
public sealed class StudentStatusController(IStudentStatusFacade status, IStudentFacade students, IClock clock) : Controller
{
    [HttpGet] public async Task<IActionResult> Change(string id)
    {
        var student = await students.GetAsync(id);
        ViewBag.StudentName = student.FullName;
        return View(new StatusChangeForm { StudentId = student.StudentId, EffectiveDate = clock.Today });
    }
    [HttpPost] public async Task<IActionResult> Change(StatusChangeForm form)
    {
        if (ModelState.IsValid) try
        {
            await status.ChangeAsync(new StudentStatusChangeInput(form.StudentId,
                form.Status, form.EffectiveDate, form.Reason, form.ApprovalReference));
            TempData["Message"] = "Authorized status change recorded.";
            return RedirectToAction("Details", "Students", new { id = form.StudentId });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        return View(form);
    }
}
