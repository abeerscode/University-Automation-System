using Microsoft.AspNetCore.Mvc;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Framework.Exceptions;
using UA.Web.UI.Areas.Academic.Models;
using UA.Web.UI.Infrastructure.Authorization;
namespace UA.Web.UI.Areas.Academic.Controllers;
[Area("Academic"), HasPermission("calendar.manage")]
public sealed class AcademicCalendarController(IAcademicCalendarFacade facade) : Controller
{
    public async Task<IActionResult> Index() => View(await facade.TermsAsync());
    public IActionResult Create()
    {
        int year = DateTime.Today.Month <= 6 ? DateTime.Today.Year : DateTime.Today.Year + 1;
        var start = new DateTime(year, 1, 1);
        return View("TermForm", new TermForm { Year = (short)year, TermNo = 1,
            RegistrationDeadline = start.AddDays(14), DropDeadline = start.AddDays(9), RetakeDeadline = start.AddMonths(1).AddDays(14) });
    }
    public async Task<IActionResult> Edit(int id)
    {
        var t = (await facade.TermsAsync()).FirstOrDefault(x => x.Id == id); if (t is null) return NotFound();
        return View("TermForm", new TermForm { Id = t.Id, Year = t.Year, TermNo = t.TermNo,
            RegistrationDeadline = t.RegistrationDeadline, DropDeadline = t.DropDeadline, RetakeDeadline = t.RetakeDeadline });
    }
    [HttpPost] public async Task<IActionResult> Save(TermForm form)
    {
        if (ModelState.IsValid) try
        {
            await facade.SaveTermAsync(new CalendarTermInput(form.Id, form.Year, form.TermNo,
                form.RegistrationDeadline, form.DropDeadline, form.RetakeDeadline));
            TempData["Message"] = "Academic term saved."; return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        return View("TermForm", form);
    }
    public async Task<IActionResult> Events(int id)
    {
        ViewBag.Term = (await facade.TermsAsync()).FirstOrDefault(x => x.Id == id);
        if (ViewBag.Term is null) return NotFound();
        return View(await facade.EventsAsync(id));
    }
    public async Task<IActionResult> CreateEvent(int termId)
    {
        var term = (await facade.TermsAsync()).FirstOrDefault(x => x.Id == termId); if (term is null) return NotFound();
        return View("EventForm", new CalendarEventForm { TermId = termId, Starts = term.Starts, Ends = term.Starts });
    }
    public async Task<IActionResult> EditEvent(int id, int termId)
    {
        var e = (await facade.EventsAsync(termId)).FirstOrDefault(x => x.Id == id); if (e is null) return NotFound();
        return View("EventForm", new CalendarEventForm { Id = e.Id, TermId = e.TermId, Type = e.Type,
            Title = e.Title, Starts = e.Starts, Ends = e.Ends });
    }
    [HttpPost] public async Task<IActionResult> SaveEvent(CalendarEventForm form)
    {
        if (ModelState.IsValid) try
        {
            await facade.SaveEventAsync(new CalendarEventInput(form.Id, form.TermId, form.Type, form.Title, form.Starts, form.Ends));
            TempData["Message"] = "Calendar event saved."; return RedirectToAction(nameof(Events), new { id = form.TermId });
        }
        catch (BusinessRuleException ex) { ModelState.AddModelError("", ex.Message); }
        return View("EventForm", form);
    }
}
