namespace UniversityAutomation.Models;

/// <summary>Midterm or final examination of a term.</summary>
/// <remarks>Requirements: FR-EXM-01</remarks>
[Table("Exam_Event")]
public class ExamEvent
{
    /// <summary>Exam event identifier</summary>
    [Key]
    [Column("event_id")]
    public int EventId { get; set; }

    /// <summary>Term</summary>
    [Column("term_id")]
    public int TermId { get; set; }

    /// <summary>Midterm or Final</summary>
    [Required, MaxLength(10)]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>First day</summary>
    [Column("start_date", TypeName = "date")]
    public DateTime StartDate { get; set; }

    /// <summary>Last day</summary>
    [Column("end_date", TypeName = "date")]
    public DateTime EndDate { get; set; }

    /// <summary>Draft, Scheduled or Completed</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(TermId))]
    public virtual AcademicTerm Term { get; set; } = null!;
    [InverseProperty(nameof(ExamSitting.Event))]
    public virtual ICollection<ExamSitting> ExamSittings { get; set; } = new List<ExamSitting>();
    [InverseProperty(nameof(EligibilityHold.Event))]
    public virtual ICollection<EligibilityHold> EligibilityHolds { get; set; } = new List<EligibilityHold>();
}
