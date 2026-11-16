namespace UA.Entities;

/// <summary>Personal, guardian and education data of a person who applies for admission.</summary>
/// <remarks>Requirements: FR-ADM-02, FR-ADM-03, FR-ADM-05</remarks>
[Table("Applicant")]
[Index(nameof(UserId), IsUnique = true, Name = "UQ_Applicant_user_id")]
[Index(nameof(SscBoard), nameof(SscRoll), nameof(HscBoard), nameof(HscRoll), IsUnique = true, Name = "UQ_Applicant_ssc_board_ssc_roll_hsc_board_hsc_roll")]
public class Applicant
{
    /// <summary>Applicant identifier (shown as applicant ID)</summary>
    [Key]
    [Column("applicant_id")]
    public int ApplicantId { get; set; }

    /// <summary>Login account of the applicant</summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Full name</summary>
    [Required, MaxLength(100)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Date of birth</summary>
    [Column("date_of_birth", TypeName = "date")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>Gender</summary>
    [Required, MaxLength(10)]
    [Column("gender")]
    public string Gender { get; set; } = string.Empty;

    /// <summary>Postal address</summary>
    [Required, MaxLength(255)]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>Guardian name</summary>
    [Required, MaxLength(100)]
    [Column("guardian_name")]
    public string GuardianName { get; set; } = string.Empty;

    /// <summary>Guardian phone number</summary>
    [Required, MaxLength(20)]
    [Column("guardian_phone")]
    public string GuardianPhone { get; set; } = string.Empty;

    /// <summary>SSC (or equivalent) board</summary>
    [Required, MaxLength(30)]
    [Column("ssc_board")]
    public string SscBoard { get; set; } = string.Empty;

    /// <summary>SSC (or equivalent) roll number</summary>
    [Required, MaxLength(20)]
    [Column("ssc_roll")]
    public string SscRoll { get; set; } = string.Empty;

    /// <summary>SSC (or equivalent) result</summary>
    [Column("ssc_result", TypeName = "decimal(3,2)")]
    public decimal SscResult { get; set; }

    /// <summary>HSC (or equivalent) board</summary>
    [Required, MaxLength(30)]
    [Column("hsc_board")]
    public string HscBoard { get; set; } = string.Empty;

    /// <summary>HSC (or equivalent) roll number</summary>
    [Required, MaxLength(20)]
    [Column("hsc_roll")]
    public string HscRoll { get; set; } = string.Empty;

    /// <summary>HSC (or equivalent) result</summary>
    [Column("hsc_result", TypeName = "decimal(3,2)")]
    public decimal HscResult { get; set; }

    /// <summary>Path of the applicant photo</summary>
    [Required, MaxLength(255)]
    [Column("photo_path")]
    public string PhotoPath { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(UserId))]
    public virtual UserAccount User { get; set; } = null!;
    [InverseProperty(nameof(Application.Applicant))]
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    [InverseProperty(nameof(Due.Applicant))]
    public virtual ICollection<Due> Dues { get; set; } = new List<Due>();
    [InverseProperty(nameof(Payment.Applicant))]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    [InverseProperty(nameof(UploadedDocument.Applicant))]
    public virtual ICollection<UploadedDocument> UploadedDocuments { get; set; } = new List<UploadedDocument>();
}
