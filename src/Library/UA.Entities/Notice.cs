namespace UniversityAutomation.Models;

/// <summary>University and department notices, public or internal.</summary>
/// <remarks>Requirements: FR-SEC-09, FR-WEB-03</remarks>
[Table("Notice")]
public class Notice
{
    /// <summary>Notice identifier</summary>
    [Key]
    [Column("notice_id")]
    public int NoticeId { get; set; }

    /// <summary>Department (empty for university-wide)</summary>
    [Column("department_id")]
    public int? DepartmentId { get; set; }

    /// <summary>Title</summary>
    [Required, MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Notice text</summary>
    [Column("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>Shown on the public website</summary>
    [Column("is_public")]
    public bool IsPublic { get; set; }

    /// <summary>Publication time</summary>
    [Column("published_at")]
    public DateTime? PublishedAt { get; set; }

    /// <summary>Author</summary>
    [Column("author_id")]
    public int AuthorId { get; set; }

    /// <summary>Approver</summary>
    [Column("approved_by")]
    public int? ApprovedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }
    [ForeignKey(nameof(AuthorId))]
    public virtual UserAccount Author { get; set; } = null!;
    [ForeignKey(nameof(ApprovedBy))]
    public virtual UserAccount? ApprovedByUser { get; set; }
}
