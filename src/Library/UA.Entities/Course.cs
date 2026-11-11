namespace UniversityAutomation.Models;

/// <summary>Course master data.</summary>
/// <remarks>Requirements: FR-ACA-02</remarks>
[Table("Course")]
[Index(nameof(Code), IsUnique = true, Name = "UQ_Course_code")]
public class Course
{
    /// <summary>Course identifier</summary>
    [Key]
    [Column("course_id")]
    public int CourseId { get; set; }

    /// <summary>Owning department</summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>Course code</summary>
    [Required, MaxLength(15)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>Course title</summary>
    [Required, MaxLength(150)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Credit value</summary>
    [Column("credits", TypeName = "decimal(3,1)")]
    public decimal Credits { get; set; }

    /// <summary>Theory, Lab, Project, Thesis or Internship</summary>
    [Required, MaxLength(15)]
    [Column("course_type")]
    public string CourseType { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;
    [InverseProperty(nameof(CourseEquivalence.OldCourse))]
    public virtual ICollection<CourseEquivalence> OldCourseEquivalences { get; set; } = new List<CourseEquivalence>();
    [InverseProperty(nameof(CourseEquivalence.NewCourse))]
    public virtual ICollection<CourseEquivalence> NewCourseEquivalences { get; set; } = new List<CourseEquivalence>();
    [InverseProperty(nameof(CurriculumCourse.Course))]
    public virtual ICollection<CurriculumCourse> CurriculumCourses { get; set; } = new List<CurriculumCourse>();
    [InverseProperty(nameof(CoursePrerequisite.Course))]
    public virtual ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();
    [InverseProperty(nameof(CoursePrerequisite.PrerequisiteCourse))]
    public virtual ICollection<CoursePrerequisite> RequiredByPrerequisites { get; set; } = new List<CoursePrerequisite>();
    [InverseProperty(nameof(Offering.Course))]
    public virtual ICollection<Offering> Offerings { get; set; } = new List<Offering>();
    [InverseProperty(nameof(ExamSitting.Course))]
    public virtual ICollection<ExamSitting> ExamSittings { get; set; } = new List<ExamSitting>();
    [InverseProperty(nameof(EligibilityHold.Course))]
    public virtual ICollection<EligibilityHold> EligibilityHolds { get; set; } = new List<EligibilityHold>();
}
