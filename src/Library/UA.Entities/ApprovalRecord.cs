namespace UA.Entities;

/// <summary>Outcome of a physical approval with its reference number.</summary>
/// <remarks>Requirements: FR-SEC-08, FR-SEC-15, BR-19</remarks>
[Table("Approval_Record")]
[Index(nameof(ReferenceNo), IsUnique = true, Name = "UQ_Approval_Record_reference_no")]
public class ApprovalRecord
{
    /// <summary>Approval identifier</summary>
    [Key]
    [Column("approval_id")]
    public int ApprovalId { get; set; }

    /// <summary>Reference in the format DEPT-TYPE-YYYY-NNNN</summary>
    [Required, MaxLength(30)]
    [Column("reference_no")]
    public string ReferenceNo { get; set; } = string.Empty;

    /// <summary>Type of approval</summary>
    [Required, MaxLength(30)]
    [Column("approval_type")]
    public string ApprovalType { get; set; } = string.Empty;

    /// <summary>Approved or Rejected</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Recording approver</summary>
    [Column("approver_id")]
    public int ApproverId { get; set; }

    /// <summary>Date of approval</summary>
    [Column("approval_date", TypeName = "date")]
    public DateTime ApprovalDate { get; set; }

    /// <summary>Record it corrects</summary>
    [Column("supersedes_id")]
    public int? SupersedesId { get; set; }

    /// <summary>Path of the scanned copy</summary>
    [MaxLength(255)]
    [Column("scan_path")]
    public string? ScanPath { get; set; }

    // Navigation
    [ForeignKey(nameof(ApproverId))]
    public virtual UserAccount Approver { get; set; } = null!;
    [ForeignKey(nameof(SupersedesId))]
    public virtual ApprovalRecord? Supersedes { get; set; }
    [InverseProperty(nameof(Application.Approval))]
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    [InverseProperty(nameof(StudentStatusHistory.Approval))]
    public virtual ICollection<StudentStatusHistory> StudentStatusHistories { get; set; } = new List<StudentStatusHistory>();
    [InverseProperty(nameof(StudentBatchHistory.Approval))]
    public virtual ICollection<StudentBatchHistory> StudentBatchHistories { get; set; } = new List<StudentBatchHistory>();
    [InverseProperty(nameof(SemesterDrop.Approval))]
    public virtual ICollection<SemesterDrop> SemesterDrops { get; set; } = new List<SemesterDrop>();
    [InverseProperty(nameof(CourseEquivalence.Approval))]
    public virtual ICollection<CourseEquivalence> CourseEquivalences { get; set; } = new List<CourseEquivalence>();
    [InverseProperty(nameof(CurriculumVersion.Approval))]
    public virtual ICollection<CurriculumVersion> CurriculumVersions { get; set; } = new List<CurriculumVersion>();
    [InverseProperty(nameof(Enrollment.Approval))]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    [InverseProperty(nameof(EligibilityHold.Approval))]
    public virtual ICollection<EligibilityHold> EligibilityHolds { get; set; } = new List<EligibilityHold>();
    [InverseProperty(nameof(Result.Approval))]
    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    [InverseProperty(nameof(FeeWaiver.Approval))]
    public virtual ICollection<FeeWaiver> FeeWaivers { get; set; } = new List<FeeWaiver>();
    [InverseProperty(nameof(ApprovalRecord.Supersedes))]
    public virtual ICollection<ApprovalRecord> SupersedingApprovalRecords { get; set; } = new List<ApprovalRecord>();
}
