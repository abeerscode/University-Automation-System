using Microsoft.AspNetCore.Mvc;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Administration.Controllers;
[Area("Administration"), HasPermission("backup.manage")]
public sealed class BackupController : Controller
{
    public IActionResult Index() => View(); // Full database backup/restore awaits server-side operational approval.
}
