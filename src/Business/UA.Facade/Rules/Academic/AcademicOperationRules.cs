namespace UA.Facade.Rules.Academic;
public static class AcademicOperationRules
{
    public static int Semester(int admissionYear, int year, int term) => (year - admissionYear) * 2 + term;
    public static bool Overlaps(TimeSpan a, TimeSpan b, TimeSpan c, TimeSpan d) => a < d && c < b;
    public static decimal? Percentage(int present, int total) => total == 0 ? null : 100m * present / total;
    public static byte Weekday(DateTime date) => (byte)(((int)date.DayOfWeek + 6) % 7 + 1); // Monday=1
    public static bool IsShort(decimal? percent, decimal threshold) => percent.HasValue && percent.Value < threshold;
    public static string JobType(int batch, int term) => $"Reg:{batch}:{term}";
    public static string AutoJobType(int batch, int term) => $"AutoReg:{batch}:{term}";
    public static bool TryJob(string type, out int batch, out int term)
    {
        batch = term = 0;
        var parts = type.Split(':');
        return parts.Length == 3 && (parts[0] is "Reg" or "AutoReg") && int.TryParse(parts[1], out batch) && int.TryParse(parts[2], out term) && batch > 0 && term > 0;
    }
}
public interface IAttendanceMarkConverter
{
    decimal Convert(decimal percentage, decimal maximum, IEnumerable<(decimal Minimum, decimal MarkPercent)> bands);
}
public sealed class AttendanceMarkConverter : IAttendanceMarkConverter
{
    public decimal Convert(decimal percentage, decimal maximum, IEnumerable<(decimal Minimum, decimal MarkPercent)> bands)
    {
        if (percentage < 0 || percentage > 100 || maximum < 0) throw new ArgumentOutOfRangeException();
        var band = bands.OrderByDescending(b => b.Minimum).First(b => b.Minimum <= percentage);
        return Math.Round(maximum * band.MarkPercent / 100m, 2, MidpointRounding.AwayFromZero);
    }
}
