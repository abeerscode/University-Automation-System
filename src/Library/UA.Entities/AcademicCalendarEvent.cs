namespace UniversityAutomation.Models;

/// <summary>Other dated events of the academic calendar.</summary>
/// <remarks>Requirements: FR-ACA-04, FR-FIN-14</remarks>
[Table("Academic_Calendar_Event")]
public class AcademicCalendarEvent
{
    /// <summary>Calendar event identifier</summary>
    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    /// <summary>Term</summary>
    [Column("term_id")]
    public int TermId { get; set; }

    /// <summary>Class period, Midterm period, Final period, Make-up days, Result date, Fee deadline or Break</summary>
    [Required, MaxLength(25)]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>Event title</summary>
    [Required, MaxLength(100)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Start date</summary>
    [Column("start_date", TypeName = "date")]
    public DateTime StartDate { get; set; }

    /// <summary>End date</summary>
    [Column("end_date", TypeName = "date")]
    public DateTime EndDate { get; set; }

    // Navigation
    [ForeignKey(nameof(TermId))]
    public virtual AcademicTerm Term { get; set; } = null!;
}
