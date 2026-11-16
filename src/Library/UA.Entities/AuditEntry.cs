namespace UA.Entities;

/// <summary>Append-only log of sensitive actions.</summary>
/// <remarks>Requirements: FR-SEC-06, SR-14</remarks>
[Table("Audit_Entry")]
public class AuditEntry
{
    /// <summary>Audit identifier</summary>
    [Key]
    [Column("audit_id")]
    public long AuditId { get; set; }

    /// <summary>User (empty for system actions)</summary>
    [Column("user_id")]
    public int? UserId { get; set; }

    /// <summary>Action performed</summary>
    [Required, MaxLength(50)]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>Table affected</summary>
    [Required, MaxLength(50)]
    [Column("entity_name")]
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Key of the affected row</summary>
    [Required, MaxLength(30)]
    [Column("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Value before the change</summary>
    [Column("old_value")]
    public string? OldValue { get; set; }

    /// <summary>Value after the change</summary>
    [Column("new_value")]
    public string? NewValue { get; set; }

    /// <summary>Time of the action</summary>
    [Column("logged_at")]
    public DateTime LoggedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(UserId))]
    public virtual UserAccount? User { get; set; }
}
