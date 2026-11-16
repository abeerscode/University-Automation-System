namespace UA.Entities;

/// <summary>Academic profile of a teacher.</summary>
/// <remarks>Requirements: FR-FAC-01 to FR-FAC-03, FR-FAC-07</remarks>
[Table("Faculty")]
[Index(nameof(UserId), IsUnique = true, Name = "UQ_Faculty_user_id")]
[Index(nameof(EmployeeRef), IsUnique = true, Name = "UQ_Faculty_employee_ref")]
public class Faculty
{
    /// <summary>Faculty identifier</summary>
    [Key]
    [Column("faculty_id")]
    public int FacultyId { get; set; }

    /// <summary>Login account</summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>Home department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Employee reference for the future HR module</summary>
    [Required, MaxLength(30)]
    [Column("employee_ref")]
    public string EmployeeRef { get; set; } = string.Empty;

    /// <summary>Full name</summary>
    [Required, MaxLength(100)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Designation</summary>
    [Required, MaxLength(50)]
    [Column("designation")]
    public string Designation { get; set; } = string.Empty;

    /// <summary>Full-time, Part-time or Guest</summary>
    [Required, MaxLength(15)]
    [Column("faculty_type")]
    public string FacultyType { get; set; } = string.Empty;

    /// <summary>Qualifications</summary>
    [MaxLength(255)]
    [Column("qualifications")]
    public string? Qualifications { get; set; }

    /// <summary>Phone number</summary>
    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    /// <summary>Path of the photo</summary>
    [MaxLength(255)]
    [Column("photo_path")]
    public string? PhotoPath { get; set; }

    /// <summary>Active, Inactive or Left</summary>
    [Required, MaxLength(10)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(UserId))]
    public virtual UserAccount User { get; set; } = null!;
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [InverseProperty(nameof(OfferingTeacher.Faculty))]
    public virtual ICollection<OfferingTeacher> OfferingTeachers { get; set; } = new List<OfferingTeacher>();
    [InverseProperty(nameof(MentorAssignment.Faculty))]
    public virtual ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
}
