using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
namespace UA.Web.UI.Controllers;
public sealed class JobsController(IBackgroundJobFacade facade) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.ListAsync());
    public async Task<IActionResult> Status(int id)
    {
        var job = await facade.GetAsync(id); return job is null ? NotFound() : Json(job);
    }
}
