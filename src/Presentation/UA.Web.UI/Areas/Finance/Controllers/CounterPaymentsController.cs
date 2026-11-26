using Microsoft.AspNetCore.Mvc;
using UA.Facade.Interface.Finance;
using UA.Framework.Exceptions;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Finance.Controllers;
[Area("Finance"), HasPermission("payments.record")]
public sealed class CounterPaymentsController(IPaymentFacade payments, ILedgerFacade ledger) : Controller
{
    [HttpGet] public async Task<IActionResult> Create(string? studentId) => View(string.IsNullOrWhiteSpace(studentId)?null:await ledger.StudentAsync(studentId.Trim()));
    [HttpPost] public async Task<IActionResult> Create(string studentId,decimal amount,int[]? dueIds) { if(!ModelState.IsValid || string.IsNullOrWhiteSpace(studentId)) { TempData["Error"]="Invalid payment."; return RedirectToAction(nameof(Create),new {studentId}); } try { int id=await payments.CounterAsync(studentId.Trim(),amount,dueIds); return RedirectToAction(nameof(Success),new {id}); } catch(BusinessRuleException ex) { TempData["Error"]=ex.Message; return RedirectToAction(nameof(Create),new {studentId}); } }
    [HttpGet] public async Task<IActionResult> Success(int id) { ViewBag.PaymentId=id; return View(await payments.ReceiptAsync(id,false)); }
}
