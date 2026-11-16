namespace UA.Entities;

/// <summary>Attendance or dues hold on a student for an exam event, with its release.</summary>
/// <remarks>Requirements: FR-EXM-05, FR-EXM-06, A-17</remarks>
[Table("Eligibility_Hold")]
public class EligibilityHold
{
    /// <summary>Hold identifier</summary>
    [Key]
    [Column("hold_id")]
    public int HoldId { get; set; }

    /// <summary>Student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Exam event</summary>
    [Column("event_id")]
    public int EventId { get; set; }

    /// <summary>Course (filled for attendance holds only)</summary>
    [Column("course_id")]
    public int? CourseId { get; set; }

    /// <summary>Attendance or Dues</summary>
    [Required, MaxLength(15)]
    [Column("hold_type")]
    public string HoldType { get; set; } = string.Empty;

    /// <summary>Visible reason</summary>
    [Required, MaxLength(255)]
    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>User who released the hold</summary>
    [Column("released_by")]
    public int? ReleasedBy { get; set; }

    /// <summary>Reason of the release</summary>
    [MaxLength(255)]
    [Column("release_reason")]
    public string? ReleaseReason { get; set; }

    /// <summary>Time of the release</summary>
    [Column("released_at")]
    public DateTime? ReleasedAt { get; set; }

    /// <summary>Approval reference</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(EventId))]
    public virtual ExamEvent Event { get; set; } = null!;
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }
    [ForeignKey(nameof(ReleasedBy))]
    public virtual UserAccount? ReleasedByUser { get; set; }
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
}
