namespace UA.Entities;

/// <summary>Semester of the academic calendar with its main deadlines.</summary>
/// <remarks>Requirements: FR-ACA-04, SR-05, BR-02</remarks>
[Table("Academic_Term")]
[Index(nameof(Year), nameof(TermNo), IsUnique = true, Name = "UQ_Academic_Term_year_term_no")]
public class AcademicTerm
{
    /// <summary>Term identifier</summary>
    [Key]
    [Column("term_id")]
    public int TermId { get; set; }

    /// <summary>Calendar year</summary>
    [Column("year")]
    public short Year { get; set; }

    /// <summary>1 for January to June, 2 for July to December</summary>
    [Column("term_no")]
    public byte TermNo { get; set; }

    /// <summary>First day of the term</summary>
    [Column("start_date", TypeName = "date")]
    public DateTime StartDate { get; set; }

    /// <summary>Last day of the term</summary>
    [Column("end_date", TypeName = "date")]
    public DateTime EndDate { get; set; }

    /// <summary>Registration deadline</summary>
    [Column("registration_deadline", TypeName = "date")]
    public DateTime RegistrationDeadline { get; set; }

    /// <summary>Semester-drop deadline</summary>
    [Column("drop_deadline", TypeName = "date")]
    public DateTime DropDeadline { get; set; }

    /// <summary>Retake application deadline</summary>
    [Column("retake_deadline", TypeName = "date")]
    public DateTime RetakeDeadline { get; set; }

    // Navigation
    [InverseProperty(nameof(AcademicCalendarEvent.Term))]
    public virtual ICollection<AcademicCalendarEvent> AcademicCalendarEvents { get; set; } = new List<AcademicCalendarEvent>();
    [InverseProperty(nameof(Offering.Term))]
    public virtual ICollection<Offering> Offerings { get; set; } = new List<Offering>();
    [InverseProperty(nameof(ExamEvent.Term))]
    public virtual ICollection<ExamEvent> ExamEvents { get; set; } = new List<ExamEvent>();
    [InverseProperty(nameof(SemesterResult.Term))]
    public virtual ICollection<SemesterResult> SemesterResults { get; set; } = new List<SemesterResult>();
    [InverseProperty(nameof(Due.Term))]
    public virtual ICollection<Due> Dues { get; set; } = new List<Due>();
    [InverseProperty(nameof(FeeWaiver.Term))]
    public virtual ICollection<FeeWaiver> FeeWaivers { get; set; } = new List<FeeWaiver>();
    [InverseProperty(nameof(ConfigParameter.EffectiveTerm))]
    public virtual ICollection<ConfigParameter> EffectiveConfigParameters { get; set; } = new List<ConfigParameter>();
}
