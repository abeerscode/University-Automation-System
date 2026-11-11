namespace UniversityAutomation.Models;

/// <summary>Version history of website pages.</summary>
/// <remarks>Requirements: FR-WEB-07</remarks>
[Table("Web_Page_Version")]
[Index(nameof(PageId), nameof(VersionNo), IsUnique = true, Name = "UQ_Web_Page_Version_page_id_version_no")]
public class WebPageVersion
{
    /// <summary>Version identifier</summary>
    [Key]
    [Column("version_id")]
    public int VersionId { get; set; }

    /// <summary>Page</summary>
    [Column("page_id")]
    public int PageId { get; set; }

    /// <summary>Version number (unique within the page)</summary>
    [Column("version_no")]
    public int VersionNo { get; set; }

    /// <summary>Page content</summary>
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>Author</summary>
    [Column("author_id")]
    public int AuthorId { get; set; }

    /// <summary>Approver</summary>
    [Column("approved_by")]
    public int? ApprovedBy { get; set; }

    /// <summary>Time of the change</summary>
    [Column("changed_at")]
    public DateTime ChangedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(PageId))]
    public virtual WebPage Page { get; set; } = null!;
    [ForeignKey(nameof(AuthorId))]
    public virtual UserAccount Author { get; set; } = null!;
    [ForeignKey(nameof(ApprovedBy))]
    public virtual UserAccount? ApprovedByUser { get; set; }
}
