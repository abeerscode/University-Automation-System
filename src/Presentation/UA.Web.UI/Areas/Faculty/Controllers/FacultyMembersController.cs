using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Faculty;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Faculty;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Faculty.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Faculty.Controllers;
[Area("Faculty"), HasPermission("faculty.view")]
public sealed class FacultyMembersController(IFacultyFacade faculty, IDepartmentFacade departments) : Controller
{
    public async Task<IActionResult> Index(string? query, int? departmentId, string? type, string? status, int page = 1)
    {
        ViewBag.Filter = new FacultySearchInput(query, departmentId, type, status, page);
        return View(await faculty.SearchAsync(new FacultySearchInput(query, departmentId, type, status, page)));
    }
    public async Task<IActionResult> Details(int id) => View(await faculty.GetAsync(id));
    [HasPermission("faculty.manage"), HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = await departments.AvailableAsync("faculty.manage");
        return View(new FacultyForm());
    }
    [HasPermission("faculty.manage"), HttpPost, RequestSizeLimit(1_200_000)]
    public async Task<IActionResult> Create(FacultyForm form)
    {
        var photo = await GetPhotoAsync(form);
        if (ModelState.IsValid) try
        {
            var created = await faculty.CreateAsync(form.ToDto(), photo, form.Photo?.ContentType, HttpContext.RequestAborted);
            Response.Headers["Cache-Control"] = "no-store";
            return View("Created", created);
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Departments = await departments.AvailableAsync("faculty.manage");
        return View(form);
    }
    [HasPermission("faculty.manage"), HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var x = await faculty.GetAsync(id);
        ViewBag.Departments = await departments.AvailableAsync("faculty.manage");
        return View(new FacultyForm { DepartmentId = x.DepartmentId, EmployeeRef = x.EmployeeRef,
            FullName = x.FullName, Designation = x.Designation, Type = x.Type, Status = x.Status,
            Qualifications = x.Qualifications, Phone = x.Phone, Email = x.Email });
    }
    [HasPermission("faculty.manage"), HttpPost, RequestSizeLimit(1_200_000)]
    public async Task<IActionResult> Edit(int id, FacultyForm form)
    {
        var photo = await GetPhotoAsync(form);
        if (ModelState.IsValid) try
        {
            await faculty.EditAsync(id, form.ToDto(), photo, form.Photo?.ContentType, HttpContext.RequestAborted);
            TempData["Message"] = "Faculty profile updated.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        ViewBag.Departments = await departments.AvailableAsync("faculty.manage");
        return View(form);
    }
    [HttpGet]
    public async Task<IActionResult> Photo(int id)
    {
        var result = await faculty.PhotoAsync(id, HttpContext.RequestAborted);
        Response.Headers["Cache-Control"] = "private, no-store";
        return File(result.Data, result.ContentType);
    }
    private async Task<byte[]?> GetPhotoAsync(FacultyForm form)
    {
        if (form.Photo is null) return null;
        if (form.Photo.Length is < 1 or > 1_048_576 || form.Photo.ContentType is not ("image/png" or "image/jpeg"))
        { ModelState.AddModelError(nameof(form.Photo), "Use a PNG or JPEG of at most 1 MB."); return null; }
        using var stream = new MemoryStream();
        await form.Photo.CopyToAsync(stream, HttpContext.RequestAborted);
        return stream.ToArray();
    }
}
