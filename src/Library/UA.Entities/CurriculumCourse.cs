namespace UniversityAutomation.Models;

/// <summary>Courses of a curriculum version and the semester of each.</summary>
/// <remarks>Requirements: FR-ACA-02, BR-05</remarks>
[Table("Curriculum_Course")]
[PrimaryKey(nameof(CurriculumId), nameof(CourseId))]
public class CurriculumCourse
{
    /// <summary>Curriculum version</summary>
    [Column("curriculum_id")]
    public int CurriculumId { get; set; }

    /// <summary>Course</summary>
    [Column("course_id")]
    public int CourseId { get; set; }

    /// <summary>Semester in which the course is taught</summary>
    [Column("semester_no")]
    public byte SemesterNo { get; set; }

    // Navigation
    [ForeignKey(nameof(CurriculumId))]
    public virtual CurriculumVersion Curriculum { get; set; } = null!;
    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;
}
