namespace UA.Entities;

/// <summary>Course equivalence decided for a rejoining student when curriculum versions differ.</summary>
/// <remarks>Requirements: FR-STU-17, A-19</remarks>
[Table("Course_Equivalence")]
public class CourseEquivalence
{
    /// <summary>Equivalence identifier</summary>
    [Key]
    [Column("equivalence_id")]
    public int EquivalenceId { get; set; }

    /// <summary>Rejoining student</summary>
    [Required, MaxLength(8)]
    [Column("student_id", TypeName = "char(8)")]
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Course of the original curriculum version</summary>
    [Column("old_course_id")]
    public int OldCourseId { get; set; }

    /// <summary>Equivalent course of the junior curriculum version</summary>
    [Column("new_course_id")]
    public int NewCourseId { get; set; }

    /// <summary>Department Head approval</summary>
    [Column("approval_id")]
    public int ApprovalId { get; set; }

    // Navigation
    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; } = null!;
    [ForeignKey(nameof(OldCourseId))]
    public virtual Course OldCourse { get; set; } = null!;
    [ForeignKey(nameof(NewCourseId))]
    public virtual Course NewCourse { get; set; } = null!;
    [ForeignKey(nameof(ApprovalId))]
    public virtual ApprovalRecord Approval { get; set; } = null!;
}
