namespace UniversityAutomation.Models;

/// <summary>Verification and password-reset codes.</summary>
/// <remarks>Requirements: FR-ADM-02, FR-SEC-01, NFR-INT-10</remarks>
[Table("Verification_Token")]
public class VerificationToken
{
    /// <summary>Token identifier</summary>
    [Key]
    [Column("token_id")]
    public int TokenId { get; set; }

    /// <summary>User</summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Email, Mobile or Password reset</summary>
    [Required, MaxLength(15)]
    [Column("purpose")]
    public string Purpose { get; set; } = string.Empty;

    /// <summary>Hash of the code or link token</summary>
    [Required, MaxLength(255)]
    [Column("code_hash")]
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>Expiry time</summary>
    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>Attempts used</summary>
    [Column("attempts")]
    public byte Attempts { get; set; }

    /// <summary>Time of use</summary>
    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(UserId))]
    public virtual UserAccount User { get; set; } = null!;
}
