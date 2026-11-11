namespace UniversityAutomation.Models;

/// <summary>Message templates.</summary>
/// <remarks>Requirements: SR-17, FR-SEC-07</remarks>
[Table("Notification_Template")]
[Index(nameof(Code), IsUnique = true, Name = "UQ_Notification_Template_code")]
public class NotificationTemplate
{
    /// <summary>Template identifier</summary>
    [Key]
    [Column("template_id")]
    public int TemplateId { get; set; }

    /// <summary>Event code</summary>
    [Required, MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Email, SMS or In-app</summary>
    [Required, MaxLength(10)]
    [Column("channel")]
    public string Channel { get; set; } = string.Empty;

    /// <summary>Subject</summary>
    [MaxLength(150)]
    [Column("subject")]
    public string? Subject { get; set; }

    /// <summary>Message text with placeholders</summary>
    [Column("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>Template enabled</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

    // Navigation
    [InverseProperty(nameof(Notification.Template))]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
