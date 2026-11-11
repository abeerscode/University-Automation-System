namespace UniversityAutomation.Models;

/// <summary>Enrollment of a student in an offering, regular or retake.</summary>
/// <remarks>Requirements: FR-ACA-07, FR-ACA-08, BR-13</remarks>
[Table("Enrollment")]
[Index(nameof(StudentId), nameof(OfferingId), IsUnique = true, Name = "UQ_Enrollment_student_id_offering_id")]
public class Enrollment
{
    /// <summary>Enrollment identifier</summary>
    [Key]
    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    /// <summary>Student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Offering</summary>
    [Column("offering_id")]
    public int OfferingId { get; set; }

    /// <summary>Regular or Retake</summary>
    [Required, MaxLength(10)]
    [Column("enrollment_type")]
    public string EnrollmentType { get; set; } = string.Empty;

    /// <summary>Attempt number of the course</summary>
    [Column("attempt_no")]
    public byte AttemptNo { get; set; }

    /// <summary>Registered, Confirmed or Completed</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Late retake override reference</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(OfferingId))]
    public virtual Offering Offering { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
    [InverseProperty(nameof(AttendanceRecord.Enrollment))]
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    [InverseProperty(nameof(MarkEntry.Enrollment))]
    public virtual ICollection<MarkEntry> MarkEntries { get; set; } = new List<MarkEntry>();
    [InverseProperty(nameof(Result.Enrollment))]
    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    [InverseProperty(nameof(Due.Enrollment))]
    public virtual ICollection<Due> Dues { get; set; } = new List<Due>();
}
