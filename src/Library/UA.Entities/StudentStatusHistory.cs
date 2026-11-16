namespace UA.Entities;

/// <summary>History of every status change of a student.</summary>
/// <remarks>Requirements: FR-STU-03, BR-23, BR-24</remarks>
[Table("Student_Status_History")]
public class StudentStatusHistory
{
    /// <summary>History row identifier</summary>
    [Key]
    [Column("history_id")]
    public int HistoryId { get; set; }

    /// <summary>Student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>New status</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Date the status takes effect</summary>
    [Column("effective_date", TypeName = "date")]
    public DateTime EffectiveDate { get; set; }

    /// <summary>Reason of the change</summary>
    [Required, MaxLength(255)]
    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>Physical approval reference</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    /// <summary>User who recorded the change</summary>
    [Column("changed_by")]
    public int ChangedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
    [ForeignKey(nameof(ChangedBy))]
    public virtual UserAccount ChangedByUser { get; set; } = null!;
}
