namespace UA.Framework.Time;
public interface IClock { DateTime Now { get; } DateTime Today { get; } }
public sealed class DhakaClock : IClock
{
    private static readonly TimeZoneInfo Zone = FindZone();
    private static TimeZoneInfo FindZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time"); }
    }
    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);
    public DateTime Today => Now.Date;
}
