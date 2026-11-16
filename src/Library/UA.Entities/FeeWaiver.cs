namespace UA.Entities;

/// <summary>Waiver or scholarship discount per student and semester.</summary>
/// <remarks>Requirements: FR-FIN-15</remarks>
[Table("Fee_Waiver")]
[Index(nameof(StudentId), nameof(TermId), IsUnique = true, Name = "UQ_Fee_Waiver_student_id_term_id")]
public class FeeWaiver
{
    /// <summary>Waiver identifier</summary>
    [Key]
    [Column("waiver_id")]
    public int WaiverId { get; set; }

    /// <summary>Student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Term</summary>
    [Column("term_id")]
    public int TermId { get; set; }

    /// <summary>Discount in Taka</summary>
    [Column("discount_amount", TypeName = "decimal(10,2)")]
    public decimal DiscountAmount { get; set; }

    /// <summary>Approval reference</summary>
    [Column("approval_id")]
    public int ApprovalId { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(TermId))]
    public virtual AcademicTerm Term { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord Approval { get; set; } = null!;
}
