using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Student;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Student.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Student.Controllers;
[Area("Student"), HasPermission("students.view")]
public sealed class StudentsController(IStudentFacade students) : Controller
{
    public async Task<IActionResult> Index(string? query, int? departmentId, int? batchId,
        int? sectionId, string? status, int page = 1)
    {
        ViewBag.Filter = new StudentSearchInput(query, departmentId, batchId, sectionId, status, page);
        return View(await students.SearchAsync(new StudentSearchInput(query, departmentId, batchId, sectionId, status, page)));
    }
    public async Task<IActionResult> Details(string id) => View(await students.GetAsync(id));
    [HasPermission("students.edit"), HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var s = await students.GetAsync(id);
        return View(new StudentEditForm { StudentId = s.StudentId, FullName = s.FullName,
            DateOfBirth = s.DateOfBirth, Gender = s.Gender, GuardianName = s.GuardianName,
            GuardianPhone = s.GuardianPhone, Phone = s.Phone, Address = s.Address });
    }
    [HasPermission("students.edit"), HttpPost]
    public async Task<IActionResult> Edit(StudentEditForm form)
    {
        if (ModelState.IsValid) try
        {
            await students.UpdateAsync(new StudentEditInput(form.StudentId, form.FullName,
                form.DateOfBirth, form.Gender, form.GuardianName, form.GuardianPhone,
                form.Phone, form.Address, form.ApprovalReference));
            TempData["Message"] = "Student record updated.";
            return RedirectToAction(nameof(Details), new { id = form.StudentId });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        return View(form);
    }
}
