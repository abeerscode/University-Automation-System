namespace UA.Entities;

/// <summary>Student request for a transcript, testimonial, certificate or ID card.</summary>
/// <remarks>Requirements: FR-STU-16, UR-11</remarks>
[Table("Document_Request")]
public class DocumentRequest
{
    /// <summary>Request identifier</summary>
    [Key]
    [Column("request_id")]
    public int RequestId { get; set; }

    /// <summary>Requesting student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Transcript, Testimonial, Certificate or ID card</summary>
    [Required, MaxLength(20)]
    [Column("request_type")]
    public string RequestType { get; set; } = string.Empty;

    /// <summary>Requested, Issued or Rejected</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Due generated for the request</summary>
    [Column("due_id")]
    public int? DueId { get; set; }

    /// <summary>Time of the request</summary>
    [Column("requested_at")]
    public DateTime RequestedAt { get; set; }

    /// <summary>Time of issue</summary>
    [Column("issued_at")]
    public DateTime? IssuedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(DueId))]
    public virtual Due? Due { get; set; }
}
