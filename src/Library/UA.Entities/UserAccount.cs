namespace UniversityAutomation.Models;

/// <summary>Login accounts of all users.</summary>
/// <remarks>Requirements: FR-SEC-01, FR-SEC-04, UR-43</remarks>
[Table("User_Account")]
[Index(nameof(Username), IsUnique = true, Name = "UQ_User_Account_username")]
[Index(nameof(Email), IsUnique = true, Name = "UQ_User_Account_email")]
public class UserAccount
{
    /// <summary>User identifier</summary>
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Login name</summary>
    [Required, MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Salted password hash</summary>
    [Required, MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Email address</summary>
    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    /// <summary>Mobile number</summary>
    [MaxLength(20)]
    [Column("mobile")]
    public string? Mobile { get; set; }

    /// <summary>Email or mobile verified</summary>
    [Column("is_verified")]
    public bool IsVerified { get; set; }

    /// <summary>Active, Inactive or Locked</summary>
    [Required, MaxLength(10)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Multi-factor authentication enabled</summary>
    [Column("mfa_enabled")]
    public bool MfaEnabled { get; set; }

    /// <summary>Consecutive failed logins</summary>
    [Column("failed_login_count")]
    public short FailedLoginCount { get; set; }

    /// <summary>End of the account lock</summary>
    [Column("locked_until")]
    public DateTime? LockedUntil { get; set; }

    /// <summary>Last successful login</summary>
    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    [InverseProperty(nameof(Applicant.User))]
    public virtual Applicant? Applicant { get; set; }
    [InverseProperty(nameof(Student.User))]
    public virtual Student? Student { get; set; }
    [InverseProperty(nameof(StudentStatusHistory.ChangedByUser))]
    public virtual ICollection<StudentStatusHistory> ChangedStudentStatusHistories { get; set; } = new List<StudentStatusHistory>();
    [InverseProperty(nameof(SemesterDrop.RecommendedByUser))]
    public virtual ICollection<SemesterDrop> RecommendedSemesterDrops { get; set; } = new List<SemesterDrop>();
    [InverseProperty(nameof(AttendanceRecord.RecordedByUser))]
    public virtual ICollection<AttendanceRecord> RecordedAttendanceRecords { get; set; } = new List<AttendanceRecord>();
    [InverseProperty(nameof(Faculty.User))]
    public virtual Faculty? Faculty { get; set; }
    [InverseProperty(nameof(EligibilityHold.ReleasedByUser))]
    public virtual ICollection<EligibilityHold> ReleasedEligibilityHolds { get; set; } = new List<EligibilityHold>();
    [InverseProperty(nameof(SeatAllocation.ClashConfirmedByUser))]
    public virtual ICollection<SeatAllocation> ClashConfirmedSeatAllocations { get; set; } = new List<SeatAllocation>();
    [InverseProperty(nameof(MarkSubmission.SubmittedByUser))]
    public virtual ICollection<MarkSubmission> SubmittedMarkSubmissions { get; set; } = new List<MarkSubmission>();
    [InverseProperty(nameof(MarkSubmission.VerifiedByUser))]
    public virtual ICollection<MarkSubmission> VerifiedMarkSubmissions { get; set; } = new List<MarkSubmission>();
    [InverseProperty(nameof(MarkSubmission.ApprovedByUser))]
    public virtual ICollection<MarkSubmission> ApprovedMarkSubmissions { get; set; } = new List<MarkSubmission>();
    [InverseProperty(nameof(MarkEntry.EnteredByUser))]
    public virtual ICollection<MarkEntry> EnteredMarkEntries { get; set; } = new List<MarkEntry>();
    [InverseProperty(nameof(Payment.RecordedByUser))]
    public virtual ICollection<Payment> RecordedPayments { get; set; } = new List<Payment>();
    [InverseProperty(nameof(UserRole.User))]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    [InverseProperty(nameof(VerificationToken.User))]
    public virtual ICollection<VerificationToken> VerificationTokens { get; set; } = new List<VerificationToken>();
    [InverseProperty(nameof(ApprovalRecord.Approver))]
    public virtual ICollection<ApprovalRecord> RecordedApprovalRecords { get; set; } = new List<ApprovalRecord>();
    [InverseProperty(nameof(AuditEntry.User))]
    public virtual ICollection<AuditEntry> AuditEntries { get; set; } = new List<AuditEntry>();
    [InverseProperty(nameof(ConfigParameter.ChangedByUser))]
    public virtual ICollection<ConfigParameter> ChangedConfigParameters { get; set; } = new List<ConfigParameter>();
    [InverseProperty(nameof(Notification.RecipientUser))]
    public virtual ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
    [InverseProperty(nameof(BackgroundJob.RequestedByUser))]
    public virtual ICollection<BackgroundJob> RequestedBackgroundJobs { get; set; } = new List<BackgroundJob>();
    [InverseProperty(nameof(Notice.Author))]
    public virtual ICollection<Notice> AuthoredNotices { get; set; } = new List<Notice>();
    [InverseProperty(nameof(Notice.ApprovedByUser))]
    public virtual ICollection<Notice> ApprovedNotices { get; set; } = new List<Notice>();
    [InverseProperty(nameof(WebPageVersion.Author))]
    public virtual ICollection<WebPageVersion> AuthoredWebPageVersions { get; set; } = new List<WebPageVersion>();
    [InverseProperty(nameof(WebPageVersion.ApprovedByUser))]
    public virtual ICollection<WebPageVersion> ApprovedWebPageVersions { get; set; } = new List<WebPageVersion>();
}
