namespace UA.Entities;

/// <summary>Mark of an enrollment in one component.</summary>
/// <remarks>Requirements: FR-EXM-14, FR-EXM-15, FR-EXM-27</remarks>
[Table("Mark_Entry")]
[Index(nameof(EnrollmentId), nameof(ComponentId), IsUnique = true, Name = "UQ_Mark_Entry_enrollment_id_component_id")]
public class MarkEntry
{
    /// <summary>Entry identifier</summary>
    [Key]
    [Column("entry_id")]
    public long EntryId { get; set; }

    /// <summary>Enrollment</summary>
    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    /// <summary>Component</summary>
    [Column("component_id")]
    public int ComponentId { get; set; }

    /// <summary>Submission the entry belongs to</summary>
    [Column("submission_id")]
    public int SubmissionId { get; set; }

    /// <summary>Marks obtained</summary>
    [Column("marks", TypeName = "decimal(5,2)")]
    public decimal? Marks { get; set; }

    /// <summary>Absent in the component</summary>
    [Column("is_absent")]
    public bool IsAbsent { get; set; }

    /// <summary>User who entered the mark</summary>
    [Column("entered_by")]
    public int EnteredBy { get; set; }

    // Navigation
    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;
    [ForeignKey(nameof(ComponentId))]
    public virtual MarkComponent Component { get; set; } = null!;
    [ForeignKey(nameof(SubmissionId))]
    public virtual MarkSubmission Submission { get; set; } = null!;
    [ForeignKey(nameof(EnteredBy))]
    public virtual UserAccount EnteredByUser { get; set; } = null!;
}
