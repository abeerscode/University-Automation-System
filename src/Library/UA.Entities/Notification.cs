namespace UniversityAutomation.Models;

/// <summary>Queue and delivery status of messages.</summary>
/// <remarks>Requirements: SR-17, NFR-INT-07 to NFR-INT-09</remarks>
[Table("Notification")]
[Index(nameof(EventKey), nameof(RecipientUserId), IsUnique = true, Name = "UQ_Notification_event_key_recipient_user_id")]
public class Notification
{
    /// <summary>Notification identifier</summary>
    [Key]
    [Column("notification_id")]
    public long NotificationId { get; set; }

    /// <summary>Template</summary>
    [Column("template_id")]
    public int TemplateId { get; set; }

    /// <summary>Recipient</summary>
    [Column("recipient_user_id")]
    public int RecipientUserId { get; set; }

    /// <summary>Event identifier (prevents duplicates)</summary>
    [Required, MaxLength(100)]
    [Column("event_key")]
    public string EventKey { get; set; } = string.Empty;

    /// <summary>Queued, Sent, Failed or Read</summary>
    [Required, MaxLength(10)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Delivery attempts</summary>
    [Column("attempts")]
    public byte Attempts { get; set; }

    /// <summary>Queue time</summary>
    [Column("queued_at")]
    public DateTime QueuedAt { get; set; }

    /// <summary>Delivery time</summary>
    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    // Navigation
    [ForeignKey(nameof(TemplateId))]
    public virtual NotificationTemplate Template { get; set; } = null!;
    [ForeignKey(nameof(RecipientUserId))]
    public virtual UserAccount RecipientUser { get; set; } = null!;
}
