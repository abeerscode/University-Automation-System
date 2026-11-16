namespace UA.Entities;

/// <summary>Prerequisite relation between courses.</summary>
/// <remarks>Requirements: FR-ACA-02, FR-ACA-14, BR-17</remarks>
[Table("Course_Prerequisite")]
[PrimaryKey(nameof(CourseId), nameof(PrerequisiteCourseId))]
public class CoursePrerequisite
{
    /// <summary>Course that has the prerequisite</summary>
    [Column("course_id")]
    public int CourseId { get; set; }

    /// <summary>Required course</summary>
    [Column("prerequisite_course_id")]
    public int PrerequisiteCourseId { get; set; }

    // Navigation
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;
    [ForeignKey(nameof(PrerequisiteCourseId))]
    public virtual Course PrerequisiteCourse { get; set; } = null!;
}
