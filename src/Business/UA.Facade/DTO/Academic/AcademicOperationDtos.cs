namespace UA.Facade.DTO.Academic;
public sealed record AcademicOption(int Id, string Label);
public sealed record OperationBatch(int Id, int DepartmentId, string Label, bool CanGenerate, bool CanRegister);
public sealed record OfferingRow(int Id, int TermId, int BatchId, int SectionId, string Course, string Section, byte Semester, string Status, bool CanManage, bool CanAssign, bool CanSchedule);
public sealed record AssignmentRow(int FacultyId, string Teacher, DateTime Assigned, DateTime? Ended,bool RetakeMarker=false);
public sealed record RoutineRow(int Id, string Room, byte Day, TimeSpan Start, TimeSpan End);
public sealed record EnrollmentRow(int Id, string StudentId, string Name, string Type, string Status, bool PrerequisiteWarning, int Present, int Classes, decimal? Percent, bool Shortage, decimal? Marks, int? ScaleVersion);
public sealed record SectionStudent(string StudentId, string Name, string Section, string Status, decimal EarnedCredits);
public sealed record OperationsPage(IReadOnlyList<AcademicOption> Terms, IReadOnlyList<OperationBatch> Batches, IReadOnlyList<OfferingRow> Offerings);
public sealed record OfferingDetail(OfferingRow Offering, IReadOnlyList<AssignmentRow> Teachers, IReadOnlyList<RoutineRow> Routine, IReadOnlyList<EnrollmentRow> Students, IReadOnlyList<AcademicOption> Faculty, IReadOnlyList<AcademicOption> Rooms, IReadOnlyList<AssignmentHistoryRow> History);
public sealed record StudentCourseRow(string Course, string Term, string Status, IReadOnlyList<RoutineRow> Routine, decimal? Attendance, bool Shortage);
public sealed record RegistrationJobRow(int Id, string Status, byte Progress, string? FailureReason, DateTime? FinishedAt, int BatchId, int TermId);

public sealed record AttendanceSession(OfferingDetail Detail, int RoutineId, DateTime Date, IReadOnlyList<int> PresentIds, bool HasRecords);

public sealed record RegistrationJobListRow(int Id, string Type, string Status, byte Progress, DateTime Created);

public sealed record AcademicPrintReport(string Title, string[] Headers, string[][] Rows);

public sealed record AssignmentHistoryRow(string Teacher, string PreviousInterval, DateTime LoggedAt);
