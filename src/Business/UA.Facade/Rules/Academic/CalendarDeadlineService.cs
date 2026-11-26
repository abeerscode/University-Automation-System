using UA.Framework.Exceptions;
namespace UA.Facade.Rules.Academic;
public enum AcademicDeadline { Registration, Drop, Retake }
public interface ICalendarDeadlineService
{
    DateTime Deadline(AcademicDeadline kind, DateTime registration, DateTime drop, DateTime retake);
    bool IsOpen(DateTime today, DateTime termStart, AcademicDeadline kind, DateTime registration, DateTime drop, DateTime retake);
    void ValidateTerm(short year, byte termNo, DateTime registration, DateTime drop, DateTime retake);
}
public sealed class CalendarDeadlineService : ICalendarDeadlineService
{
    public DateTime Deadline(AcademicDeadline kind, DateTime registration, DateTime drop, DateTime retake) => kind switch
    { AcademicDeadline.Registration => registration.Date, AcademicDeadline.Drop => drop.Date, AcademicDeadline.Retake => retake.Date, _ => throw new ArgumentOutOfRangeException(nameof(kind)) };
    public bool IsOpen(DateTime today, DateTime termStart, AcademicDeadline kind, DateTime registration, DateTime drop, DateTime retake)
        => today.Date >= termStart.Date && today.Date <= Deadline(kind, registration, drop, retake);
    public void ValidateTerm(short year, byte termNo, DateTime registration, DateTime drop, DateTime retake)
    {
        if (year < 2000 || year > 2100 || termNo is not (1 or 2)) throw new BusinessRuleException("Year or term number is invalid.");
        var start = new DateTime(year, termNo == 1 ? 1 : 7, 1);
        var end = start.AddMonths(6).AddDays(-1);
        if (registration.Date < start || registration.Date > end || drop.Date < start || drop.Date > end ||
            retake.Date < start || retake.Date > end)
            throw new BusinessRuleException("All term deadlines must be within the term.");
        if (drop.Date > registration.Date) throw new BusinessRuleException("Semester drop deadline must not follow registration deadline.");
    }
}
