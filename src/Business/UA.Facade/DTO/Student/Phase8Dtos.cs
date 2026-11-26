namespace UA.Facade.DTO.Student;
public sealed record BacklogRow(int CourseId,string Course,string Grade,int Attempt,string RetakeStatus);
public sealed record LifecycleRow(string StudentId,string Name,int DepartmentId,string Status,DateTime Limit,string DropStatus,byte? DropSemester,int? TargetBatch);
public sealed record LifecycleOption(int Id,string Label);
public sealed record GraduationCheck(bool Eligible,decimal Earned,decimal Required,decimal Cgpa,string[] Reasons);
public sealed record DocumentRequestRow(int Id,string StudentId,string Type,string Status,DateTime Requested,int? DueId);
public sealed record LifecycleDetails(LifecycleRow Student,BacklogRow[] Backlog,GraduationCheck Graduation,LifecycleOption[] Terms,LifecycleOption[] RetakeOfferings,LifecycleOption[] OldCourses,LifecycleOption[] NewCourses,DocumentRequestRow[] Documents,bool Own,bool CanRequestDrop,bool CanRecommend,bool CanApprove,bool CanRejoin,bool CanEquivalence,bool CanGraduate,bool CanRetake,bool CanProcessDocuments);

public sealed record RetakeShortlistRow(string StudentId,string Name,int CourseId,string Course,int Attempts);

public sealed record RetakeSchemePage(LifecycleOption[] Departments,LifecycleOption[] Terms,string[] Types);
public sealed record RetakeComponentInput(string Name,decimal Marks);
