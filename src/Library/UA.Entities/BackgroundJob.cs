namespace UA.Entities;

/// <summary>State of long-running jobs.</summary>
/// <remarks>Requirements: SR-16, SR-19</remarks>
[Table("Background_Job")]
public class BackgroundJob
{
    /// <summary>Job identifier</summary>
    [Key]
    [Column("job_id")]
    public int JobId { get; set; }

    /// <summary>Seat plan, Result processing, Bulk registration, Import or Report</summary>
    [Required, MaxLength(30)]
    [Column("job_type")]
    public string JobType { get; set; } = string.Empty;

    /// <summary>Requesting user</summary>
    [Column("requested_by")]
    public int RequestedBy { get; set; }

    /// <summary>Queued, Running, Completed or Failed</summary>
    [Required, MaxLength(10)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Percentage complete</summary>
    [Column("progress_percent")]
    public byte ProgressPercent { get; set; }

    /// <summary>Creation time</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Finish time</summary>
    [Column("finished_at")]
    public DateTime? FinishedAt { get; set; }

    /// <summary>Path of the error report</summary>
    [MaxLength(255)]
    [Column("error_report_path")]
    public string? ErrorReportPath { get; set; }

    // Navigation
    [ForeignKey(nameof(RequestedBy))]
    public virtual UserAccount RequestedByUser { get; set; } = null!;
}
