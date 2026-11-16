namespace UA.Entities;

/// <summary>Master data of the four departments.</summary>
/// <remarks>Requirements: SR-02, BR-01</remarks>
[Table("Department")]
[Index(nameof(Code), IsUnique = true, Name = "UQ_Department_code")]
[Index(nameof(Name), IsUnique = true, Name = "UQ_Department_name")]
[Index(nameof(IdCode), IsUnique = true, Name = "UQ_Department_id_code")]
public class Department
{
    /// <summary>Department identifier</summary>
    [Key]
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Short code (for example CSE)</summary>
    [Required, MaxLength(10)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Department name</summary>
    [Required, MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Department digits of the student ID (01 to 04)</summary>
    [Required, MaxLength(2)]
    [Column("id_code", TypeName = "char(2)")]
    public string IdCode { get; set; } = string.Empty;

    /// <summary>Active or Inactive</summary>
    [Required, MaxLength(10)]
    [Column("status")]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [InverseProperty(nameof(CircularSeat.Department))]
    public virtual ICollection<CircularSeat> CircularSeats { get; set; } = new List<CircularSeat>();
    [InverseProperty(nameof(Application.Department))]
    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();
    [InverseProperty(nameof(Student.Department))]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    [InverseProperty(nameof(AcademicProgram.Department))]
    public virtual AcademicProgram? Program { get; set; }
    [InverseProperty(nameof(Course.Department))]
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    [InverseProperty(nameof(Batch.Department))]
    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
    [InverseProperty(nameof(AttendanceScale.Department))]
    public virtual ICollection<AttendanceScale> AttendanceScales { get; set; } = new List<AttendanceScale>();
    [InverseProperty(nameof(Faculty.Department))]
    public virtual ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
    [InverseProperty(nameof(MarkComponent.Department))]
    public virtual ICollection<MarkComponent> MarkComponents { get; set; } = new List<MarkComponent>();
    [InverseProperty(nameof(GradeScale.Department))]
    public virtual ICollection<GradeScale> GradeScales { get; set; } = new List<GradeScale>();
    [InverseProperty(nameof(FeeType.Department))]
    public virtual ICollection<FeeType> FeeTypes { get; set; } = new List<FeeType>();
    [InverseProperty(nameof(UserRole.Department))]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    [InverseProperty(nameof(ConfigParameter.Department))]
    public virtual ICollection<ConfigParameter> ConfigParameters { get; set; } = new List<ConfigParameter>();
    [InverseProperty(nameof(Notice.Department))]
    public virtual ICollection<Notice> Notices { get; set; } = new List<Notice>();
    [InverseProperty(nameof(WebPage.Department))]
    public virtual ICollection<WebPage> WebPages { get; set; } = new List<WebPage>();
}
