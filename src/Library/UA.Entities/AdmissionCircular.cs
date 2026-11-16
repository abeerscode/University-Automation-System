namespace UA.Entities;

/// <summary>Admission circular of one intake: application window, test details and enrollment deadline.</summary>
/// <remarks>Requirements: FR-ADM-01, FR-WEB-04</remarks>
[Table("Admission_Circular")]
[Index(nameof(IntakeYear), IsUnique = true, Name = "UQ_Admission_Circular_intake_year")]
public class AdmissionCircular
{
    /// <summary>Circular identifier</summary>
    [Key]
    [Column("circular_id")]
    public int CircularId { get; set; }

    /// <summary>Intake (admission) year</summary>
    [Column("intake_year")]
    public short IntakeYear { get; set; }

    /// <summary>First day of the application window</summary>
    [Column("application_open", TypeName = "date")]
    public DateTime ApplicationOpen { get; set; }

    /// <summary>Last day of the application window</summary>
    [Column("application_close", TypeName = "date")]
    public DateTime ApplicationClose { get; set; }

    /// <summary>Date of the physical admission test</summary>
    [Column("test_date", TypeName = "date")]
    public DateTime TestDate { get; set; }

    /// <summary>Venue of the admission test</summary>
    [Required, MaxLength(150)]
    [Column("test_venue")]
    public string TestVenue { get; set; } = string.Empty;

    /// <summary>Date of result publication</summary>
    [Column("result_date", TypeName = "date")]
    public DateTime ResultDate { get; set; }

    /// <summary>Last day to confirm enrollment</summary>
    [Column("enrollment_deadline", TypeName = "date")]
    public DateTime EnrollmentDeadline { get; set; }

    /// <summary>Draft, Published or Closed</summary>
    [Required, MaxLength(15)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [InverseProperty(nameof(CircularSeat.Circular))]
    public virtual ICollection<CircularSeat> CircularSeats { get; set; } = new List<CircularSeat>();
    [InverseProperty(nameof(Application.Circular))]
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
}
