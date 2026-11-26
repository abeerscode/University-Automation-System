using UA.DataAccess.Interface.Academic;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Administration;
using UA.Facade.Rules.Academic;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Academic;

public sealed class AcademicCalendarFacade(IAcademicTermRepository terms, IRepository<AcademicCalendarEvent> events,
    IUserAccessFacade access, ICalendarDeadlineService deadlines, IUnitOfWork work, IClock clock) : IAcademicCalendarFacade
{
    public async Task<IReadOnlyList<CalendarTermDto>> TermsAsync(CancellationToken ct = default)
    {
        await access.RequireAsync("calendar.manage", ct: ct);
        return (await terms.ListAsync(ct: ct)).OrderByDescending(x => x.Year).ThenByDescending(x => x.TermNo).Select(Map).ToArray();
    }
    private static CalendarTermDto Map(AcademicTerm t) => new(t.TermId, t.Year, t.TermNo, t.StartDate, t.EndDate,
        t.RegistrationDeadline, t.DropDeadline, t.RetakeDeadline);
    public async Task<int> SaveTermAsync(CalendarTermInput input, CancellationToken ct = default)
    {
        await access.RequireGlobalAsync("calendar.manage", ct);
        deadlines.ValidateTerm(input.Year, input.TermNo, input.RegistrationDeadline, input.DropDeadline, input.RetakeDeadline);
        var duplicate = await terms.FirstAsync(x => x.Year == input.Year && x.TermNo == input.TermNo && x.TermId != input.Id, ct);
        if (duplicate is not null) throw new BusinessRuleException("Academic term already exists.");
        var existing = input.Id == 0 ? null : await terms.FirstAsync(x => x.TermId == input.Id, ct) ?? throw new NotFoundException();
        if (existing is not null && (existing.Year != input.Year || existing.TermNo != input.TermNo))
            throw new BusinessRuleException("Term year and number cannot change after creation.");
        if (existing is not null && existing.EndDate.Date < clock.Today) throw new BusinessRuleException("Historical terms cannot be edited.");
        if (existing is null && new DateTime(input.Year, input.TermNo == 1 ? 1 : 7, 1).AddMonths(6).AddDays(-1) < clock.Today)
            throw new BusinessRuleException("Cannot create a historical academic term.");
        var start = new DateTime(input.Year, input.TermNo == 1 ? 1 : 7, 1);
        var term = existing ?? new AcademicTerm
        {
            Year = input.Year,
            TermNo = input.TermNo,
            StartDate = start,
            EndDate = start.AddMonths(6).AddDays(-1)
        };
        term.RegistrationDeadline = input.RegistrationDeadline.Date;
        term.DropDeadline = input.DropDeadline.Date; term.RetakeDeadline = input.RetakeDeadline.Date;
        if (existing is null) await terms.AddAsync(term, ct);
        await work.SaveChangesAsync(ct); return term.TermId;
    }
    public async Task<IReadOnlyList<CalendarEventDto>> EventsAsync(int termId, CancellationToken ct = default)
    {
        await access.RequireAsync("calendar.manage", ct: ct);
        if (await terms.FirstAsync(x => x.TermId == termId, ct) is null) throw new NotFoundException("Term not found.");
        return (await events.ListAsync(x => x.TermId == termId, ct)).OrderBy(x => x.StartDate).ThenBy(x => x.EventType)
            .Select(x => new CalendarEventDto(x.EventId, x.TermId, x.EventType, x.Title, x.StartDate, x.EndDate)).ToArray();
    }
    public async Task<int> SaveEventAsync(CalendarEventInput input, CancellationToken ct = default)
    {
        await access.RequireGlobalAsync("calendar.manage", ct);
        string[] types = ["Class period", "Midterm period", "Final period", "Make-up days", "Result date", "Fee deadline", "Break"];
        var term = await terms.FirstAsync(x => x.TermId == input.TermId, ct) ?? throw new NotFoundException("Term not found.");
        if (!types.Contains(input.Type) || input.Title.Trim().Length is < 3 or > 100 ||
            input.Starts.Date < term.StartDate.Date || input.Ends.Date > term.EndDate.Date || input.Starts.Date > input.Ends.Date)
            throw new BusinessRuleException("Event type, title or dates are invalid or outside the term.");
        if (input.Type == "Make-up days")
        {
            int days = (input.Ends.Date - input.Starts.Date).Days + 1;
            if (days is < 2 or > 3) throw new BusinessRuleException("Make-up period should reserve 2–3 days.");
            var finals = await events.ListAsync(x => x.TermId == input.TermId && x.EventType == "Final period", ct);
            if (finals.Count > 0 && input.Starts.Date <= finals.Max(x => x.EndDate.Date))
                throw new BusinessRuleException("Make-up days must begin after the final exam period.");
        }
        var existing = input.Id == 0 ? null : await events.FirstAsync(x => x.EventId == input.Id, ct) ?? throw new NotFoundException();
        if (existing is not null && existing.TermId != input.TermId) throw new BusinessRuleException("Event term cannot change.");
        if (existing is not null && existing.EndDate.Date < clock.Today) throw new BusinessRuleException("Past calendar events cannot be changed.");
        var ev = existing ?? new AcademicCalendarEvent { TermId = input.TermId };
        ev.EventType = input.Type; ev.Title = input.Title.Trim(); ev.StartDate = input.Starts.Date; ev.EndDate = input.Ends.Date;
        if (existing is null) await events.AddAsync(ev, ct);
        await work.SaveChangesAsync(ct); return ev.EventId;
    }
}
