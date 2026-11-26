using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Finance;
using UA.Framework.Exceptions;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Finance.Controllers;
[Area("Finance"), HasPermission("fees.manage")]
public sealed class FeeTypesController(IFeeTypeFacade fees, IDueFacade dues) : Controller
{
    [HttpGet] public async Task<IActionResult> Index() { ViewBag.Lookups=await dues.LookupAsync("fees.manage"); return View(await fees.RatesAsync()); }
    [HttpPost] public async Task<IActionResult> Create(int departmentId,string name,decimal amount,DateTime effectiveFrom) { if(!ModelState.IsValid) { TempData["Error"]="Invalid rate input."; return RedirectToAction(nameof(Index)); } try { await fees.AddRateAsync(departmentId,name,amount,effectiveFrom); TempData["Message"]="Rate added."; } catch(BusinessRuleException ex) {TempData["Error"]=ex.Message;} return RedirectToAction(nameof(Index)); }
}
