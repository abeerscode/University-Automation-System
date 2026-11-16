namespace UA.Entities;

/// <summary>Approval chain of the marks of one offering.</summary>
/// <remarks>Requirements: FR-EXM-14, FR-EXM-16</remarks>
[Table("Mark_Submission")]
public class MarkSubmission
{
    /// <summary>Submission identifier</summary>
    [Key]
    [Column("submission_id")]
    public int SubmissionId { get; set; }

    /// <summary>Offering</summary>
    [Column("offering_id")]
    public int OfferingId { get; set; }

    /// <summary>Draft, Submitted, Verified, Approved, Returned or Published</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Teacher who submitted</summary>
    [Column("submitted_by")]
    public int SubmittedBy { get; set; }

    /// <summary>Department Head who verified</summary>
    [Column("verified_by")]
    public int? VerifiedBy { get; set; }

    /// <summary>Controller who approved</summary>
    [Column("approved_by")]
    public int? ApprovedBy { get; set; }

    /// <summary>Comment when marks are returned</summary>
    [MaxLength(255)]
    [Column("return_comment")]
    public string? ReturnComment { get; set; }

    /// <summary>Submission time</summary>
    [Column("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Verification time</summary>
    [Column("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    /// <summary>Approval time</summary>
    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(OfferingId))]
    public virtual Offering Offering { get; set; } = null!;
    [ForeignKey(nameof(SubmittedBy))]
    public virtual UserAccount SubmittedByUser { get; set; } = null!;
    [ForeignKey(nameof(VerifiedBy))]
    public virtual UserAccount? VerifiedByUser { get; set; }
    [ForeignKey(nameof(ApprovedBy))]
    public virtual UserAccount? ApprovedByUser { get; set; }
    [InverseProperty(nameof(MarkEntry.Submission))]
    public virtual ICollection<MarkEntry> MarkEntries { get; set; } = new List<MarkEntry>();
}
