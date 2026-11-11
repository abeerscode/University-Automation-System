namespace UniversityAutomation.Models;

/// <summary>One application of an applicant to one department in one circular, with test result and selection outcome.</summary>
/// <remarks>Requirements: FR-ADM-03 to FR-ADM-14, FR-ADM-17</remarks>
[Table("Application")]
[Index(nameof(ApplicantId), nameof(CircularId), IsUnique = true, Name = "UQ_Application_applicant_id_circular_id")]
public class Application
{
    /// <summary>Application identifier</summary>
    [Key]
    [Column("application_id")]
    public int ApplicationId { get; set; }

    /// <summary>Applicant who applies</summary>
    [Column("applicant_id")]
    public int ApplicantId { get; set; }

    /// <summary>Circular applied under</summary>
    [Column("circular_id")]
    public int CircularId { get; set; }

    /// <summary>Department applied to (one per application)</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Submitted, Paid, Result published, Selected or Enrolled</summary>
    [Required, MaxLength(20)]
    [Column("application_status")]
    public string ApplicationStatus { get; set; } = string.Empty;

    /// <summary>Imported test marks</summary>
    [Column("test_marks", TypeName = "decimal(6,2)")]
    public decimal? TestMarks { get; set; }

    /// <summary>Merit rank in the department</summary>
    [Column("merit_rank")]
    public int? MeritRank { get; set; }

    /// <summary>Selected, Not selected or Replacement</summary>
    [MaxLength(15)]
    [Column("selection_status")]
    public string? SelectionStatus { get; set; }

    /// <summary>Physical documents verified</summary>
    [Column("documents_verified")]
    public bool DocumentsVerified { get; set; }

    /// <summary>Reference of a replacement enrollment</summary>
    [Column("approval_id")]
    public int? ApprovalId { get; set; }

    /// <summary>Submission time</summary>
    [Column("submitted_at")]
    public DateTime SubmittedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(ApplicantId))]
    public virtual Applicant Applicant { get; set; } = null!;
    [ForeignKey(nameof(CircularId))]
    public virtual AdmissionCircular Circular { get; set; } = null!;
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord? Approval { get; set; }
    [InverseProperty(nameof(Student.Application))]
    public virtual Student? Student { get; set; }
}
