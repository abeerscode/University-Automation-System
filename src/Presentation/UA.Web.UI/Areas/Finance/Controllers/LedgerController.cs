using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Finance;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Finance.Controllers;
[Area("Finance"), HasPermission("ledger.view")]
public sealed class LedgerController(ILedgerFacade ledger,IFeeWaiverFacade waivers) : Controller
{
    [HttpPost,HasPermission("waivers.manage")] public async Task<IActionResult> Waiver(string studentId,int termId,decimal discount,int approvalId){if(!ModelState.IsValid)throw new UA.Framework.Exceptions.BusinessRuleException("Invalid waiver details.");await waivers.ApplyAsync(studentId,termId,discount,approvalId);return RedirectToAction(nameof(Index),new{studentId});}
    [HttpGet] public async Task<IActionResult> Invoice(string studentId,int? termId){Response.Headers["Cache-Control"]="private, no-store";ViewBag.TermId=termId;return View(await ledger.StudentAsync(studentId));}
    [HttpGet] public async Task<IActionResult> Index(string? studentId) => View(string.IsNullOrWhiteSpace(studentId)?null:await ledger.StudentAsync(studentId.Trim()));
}
