namespace UA.Entities;

/// <summary>Semester-drop request, approval and rejoin placement of a student.</summary>
/// <remarks>Requirements: FR-STU-07 to FR-STU-09, BR-16</remarks>
[Table("Semester_Drop")]
[Index(nameof(StudentId), IsUnique = true, Name = "UQ_Semester_Drop_student_id")]
public class SemesterDrop
{
    /// <summary>Drop identifier</summary>
    [Key]
    [Column("drop_id")]
    public int DropId { get; set; }

    /// <summary>Student (one drop per student)</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Semester that is dropped</summary>
    [Column("dropped_semester_no")]
    public byte DroppedSemesterNo { get; set; }

    /// <summary>Junior batch the student rejoins</summary>
    [Column("target_batch_id")]
    public int? TargetBatchId { get; set; }

    /// <summary>Semester at which the student rejoins</summary>
    [Column("rejoin_semester_no")]
    public byte RejoinSemesterNo { get; set; }

    /// <summary>Requested, Approved, Rejected or Rejoined</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Department user who recommended the drop</summary>
    [Column("recommended_by")]
    public int RecommendedBy { get; set; }

    /// <summary>Registrar approval reference</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    /// <summary>Date of the request</summary>
    [Column("requested_on", TypeName = "date")]
    public DateTime RequestedOn { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(TargetBatchId))]
    public virtual Batch? TargetBatch { get; set; }
    [ForeignKey(nameof(RecommendedBy))]
    public virtual UserAccount RecommendedByUser { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
}
