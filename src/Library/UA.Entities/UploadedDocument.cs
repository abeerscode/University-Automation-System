namespace UniversityAutomation.Models;

/// <summary>Photos and documents kept in the file store.</summary>
/// <remarks>Requirements: SR-21, FR-ADM-03, FR-STU-01</remarks>
// CHECK constraint (to be configured in the DbContext): CHECK (applicant_id IS NOT NULL OR student_id IS NOT NULL)  -- Every document has an owner
[Table("Uploaded_Document")]
public class UploadedDocument
{
    /// <summary>Document identifier</summary>
    [Key]
    [Column("document_id")]
    public int DocumentId { get; set; }

    /// <summary>Owning applicant</summary>
    [Column("applicant_id")]
    public int? ApplicantId { get; set; }

    /// <summary>Owning student (set after enrollment)</summary>
    [MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string? StudentId { get; set; }

    /// <summary>Certificate, Photo or other type</summary>
    [Required, MaxLength(30)]
    [Column("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>Storage path</summary>
    [Required, MaxLength(255)]
    [Column("file_path")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>File size in KB</summary>
    [Column("file_size_kb")]
    public int FileSizeKb { get; set; }

    /// <summary>File type</summary>
    [Required, MaxLength(50)]
    [Column("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Upload time</summary>
    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(ApplicantId))]
    public virtual Applicant? Applicant { get; set; }
    [ForeignKey(nameof(StudentId))]
    public virtual Student? Student { get; set; }
}
