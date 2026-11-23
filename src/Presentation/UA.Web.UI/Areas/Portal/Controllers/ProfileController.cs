using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Student;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Portal.Models;
namespace UA.Web.UI.Areas.Portal.Controllers;
[Area("Portal")]
public sealed class ProfileController(IStudentPortalFacade students) : Controller
{
    [HttpGet] public async Task<IActionResult> Index()
        => View(await students.OwnProfileAsync());
    [HttpGet] public async Task<IActionResult> Edit()
    {
        var student = await students.OwnProfileAsync();
        return View(new ProfileForm { Phone = student.Phone, Address = student.Address });
    }
    [HttpPost, RequestSizeLimit(1_200_000)]
    public async Task<IActionResult> Edit(ProfileForm form)
    {
        if (!ModelState.IsValid) return View(form);
        byte[]? bytes = null; string? mime = null;
        if (form.Photo is not null)
        {
            if (form.Photo.Length is < 1 or > 1_048_576 || form.Photo.ContentType is not ("image/png" or "image/jpeg"))
            { ModelState.AddModelError(nameof(form.Photo), "Use a PNG or JPEG photo no larger than 1 MB."); return View(form); }
            using var output = new MemoryStream();
            await form.Photo.CopyToAsync(output, HttpContext.RequestAborted);
            bytes = output.ToArray(); mime = form.Photo.ContentType;
        }
        try
        {
            await students.UpdateOwnAsync(form.Phone, form.Address, bytes, mime, HttpContext.RequestAborted);
            TempData["Message"] = "Your contact details were updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); return View(form); }
    }
    [HttpGet] public async Task<IActionResult> Photo()
    {
        var photo = await students.OwnPhotoAsync(HttpContext.RequestAborted);
        Response.Headers["Cache-Control"] = "private, no-store";
        return File(photo.Data, photo.ContentType);
    }
}
