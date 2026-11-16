namespace UA.Entities;

/// <summary>Public website pages.</summary>
/// <remarks>Requirements: FR-WEB-01, FR-WEB-02, FR-WEB-07</remarks>
[Table("Web_Page")]
[Index(nameof(Slug), IsUnique = true, Name = "UQ_Web_Page_slug")]
public class WebPage
{
    /// <summary>Page identifier</summary>
    [Key]
    [Column("page_id")]
    public int PageId { get; set; }

    /// <summary>Owning department (empty for central pages)</summary>
    [Column("department_id")]
    public int? DepartmentId { get; set; }

    /// <summary>Page address</summary>
    [Required, MaxLength(100)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    /// <summary>Title</summary>
    [Required, MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Draft, Pending or Published</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department? Department { get; set; }
    [InverseProperty(nameof(WebPageVersion.Page))]
    public virtual ICollection<WebPageVersion> WebPageVersions { get; set; } = new List<WebPageVersion>();
}
