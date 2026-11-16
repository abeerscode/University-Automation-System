namespace UA.Entities;

/// <summary>Amount owed by an applicant or student.</summary>
/// <remarks>Requirements: FR-FIN-03, FR-FIN-04, FR-FIN-07</remarks>
// CHECK constraint (to be configured in the DbContext): CHECK (student_id IS NOT NULL OR applicant_id IS NOT NULL)  -- Every due and payment has an owner (A-15)
[Table("Due")]
public class Due
{
    /// <summary>Due identifier</summary>
    [Key]
    [Column("due_id")]
    public int DueId { get; set; }

    /// <summary>Student (set after enrollment)</summary>
    [MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string? StudentId { get; set; }

    /// <summary>Applicant</summary>
    [Column("applicant_id")]
    public int? ApplicantId { get; set; }

    /// <summary>Fee type</summary>
    [Column("fee_type_id")]
    public int FeeTypeId { get; set; }

    /// <summary>Term the due is for</summary>
    [Column("term_id")]
    public int? TermId { get; set; }

    /// <summary>Retake enrollment the fee is for</summary>
    [Column("enrollment_id")]
    public int? EnrollmentId { get; set; }

    /// <summary>Amount due</summary>
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>Payment deadline</summary>
    [Column("due_date", TypeName = "date")]
    public DateTime DueDate { get; set; }

    /// <summary>Unpaid, Partially paid, Paid, Waived or Reversed</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student? Student { get; set; }
    [ForeignKey(nameof(ApplicantId))]
    public virtual Applicant? Applicant { get; set; }
    [ForeignKey(nameof(FeeTypeId))]
    public virtual FeeType FeeType { get; set; } = null!;
    [ForeignKey(nameof(TermId))]
    public virtual AcademicTerm? Term { get; set; }
    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment? Enrollment { get; set; }
    [InverseProperty(nameof(DocumentRequest.Due))]
    public virtual ICollection<DocumentRequest> DocumentRequests { get; set; } = new List<DocumentRequest>();
    [InverseProperty(nameof(PaymentAllocation.Due))]
    public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
}
