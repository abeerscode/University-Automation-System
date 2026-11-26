using Microsoft.AspNetCore.Mvc;
namespace UA.Web.UI.Controllers;
public sealed class HomeController : Controller { [Microsoft.AspNetCore.Authorization.AllowAnonymous] public IActionResult Index() => User.Identity?.IsAuthenticated==true ? View() : RedirectToAction("Index","Public"); }
