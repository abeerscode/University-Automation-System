namespace UniversityAutomation.Models;

/// <summary>History of batch and section moves of a student.</summary>
/// <remarks>Requirements: FR-STU-05, FR-STU-06, FR-STU-09</remarks>
[Table("Student_Batch_History")]
public class StudentBatchHistory
{
    /// <summary>Move identifier</summary>
    [Key]
    [Column("move_id")]
    public int MoveId { get; set; }

    /// <summary>Student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Batch of the period</summary>
    [Column("batch_id")]
    public int BatchId { get; set; }

    /// <summary>Section of the period</summary>
    [Column("section_id")]
    public int SectionId { get; set; }

    /// <summary>Start of the period</summary>
    [Column("from_date", TypeName = "date")]
    public DateTime FromDate { get; set; }

    /// <summary>End of the period (empty while current)</summary>
    [Column("to_date", TypeName = "date")]
    public DateTime? ToDate { get; set; }

    /// <summary>Reason of the move</summary>
    [Required, MaxLength(255)]
    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>Physical approval reference</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(BatchId))]
    public virtual Batch Batch { get; set; } = null!;
    [ForeignKey(nameof(SectionId))]
    public virtual Section Section { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
}
