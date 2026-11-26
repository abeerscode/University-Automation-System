using UA.Facade.DTO.Academic;
namespace UA.Facade.Interface.Academic;
public interface IAcademicCalendarFacade
{
Task<IReadOnlyList<CalendarTermDto>> TermsAsync(CancellationToken ct = default);
Task<int> SaveTermAsync(CalendarTermInput input, CancellationToken ct = default);
Task<IReadOnlyList<CalendarEventDto>> EventsAsync(int termId, CancellationToken ct = default);
Task<int> SaveEventAsync(CalendarEventInput input, CancellationToken ct = default);
}
