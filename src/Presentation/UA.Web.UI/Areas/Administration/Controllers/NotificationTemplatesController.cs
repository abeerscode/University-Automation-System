using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
using UA.Web.UI.Areas.Administration.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("templates.manage")]
public sealed class NotificationTemplatesController(INotificationFacade facade) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.TemplatesAsync());
    [HttpGet] public async Task<IActionResult> Edit(int id)
    {
        var t = (await facade.TemplatesAsync()).FirstOrDefault(x => x.Id == id);
        return t is null ? NotFound() : View(new TemplateForm { TemplateId = t.Id, Code = t.Code,
            Subject = t.Subject, Body = t.Body, Enabled = t.Active });
    }
    [HttpPost] public async Task<IActionResult> Edit(TemplateForm form)
    {
        if (!ModelState.IsValid) return View(form);
        await facade.SetTemplateAsync(form.TemplateId, form.Subject, form.Body, form.Enabled);
        TempData["Message"] = "Template updated and audit logged.";
        return RedirectToAction(nameof(Index));
    }
}
