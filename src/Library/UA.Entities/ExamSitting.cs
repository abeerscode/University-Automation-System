namespace UA.Entities;

/// <summary>Examination of one course in an exam event: regular, retake or special retake.</summary>
/// <remarks>Requirements: FR-EXM-02, FR-EXM-08, FR-EXM-09</remarks>
[Table("Exam_Sitting")]
public class ExamSitting
{
    /// <summary>Sitting identifier</summary>
    [Key]
    [Column("sitting_id")]
    public int SittingId { get; set; }

    /// <summary>Exam event</summary>
    [Column("event_id")]
    public int EventId { get; set; }

    /// <summary>Course examined</summary>
    [Column("course_id")]
    public int CourseId { get; set; }

    /// <summary>Regular, Retake or Special retake</summary>
    [Required, MaxLength(15)]
    [Column("sitting_type")]
    public string SittingType { get; set; } = string.Empty;

    /// <summary>Exam date</summary>
    [Column("exam_date", TypeName = "date")]
    public DateTime ExamDate { get; set; }

    /// <summary>Start time</summary>
    [Column("start_time")]
    public TimeSpan StartTime { get; set; }

    /// <summary>End time</summary>
    [Column("end_time")]
    public TimeSpan EndTime { get; set; }

    // Navigation
    [ForeignKey(nameof(EventId))]
    public virtual ExamEvent Event { get; set; } = null!;
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;
    [InverseProperty(nameof(SeatAllocation.Sitting))]
    public virtual ICollection<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
}
