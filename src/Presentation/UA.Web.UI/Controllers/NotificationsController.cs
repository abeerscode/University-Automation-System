using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Administration;
namespace UA.Web.UI.Controllers;
public sealed class NotificationsController(INotificationFacade facade) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.InboxAsync());
    [HttpPost] public async Task<IActionResult> MarkRead(long id) { await facade.MarkReadAsync(id); return RedirectToAction(nameof(Index)); }
}
