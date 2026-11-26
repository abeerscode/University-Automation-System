namespace UA.Facade.DTO.Faculty;
public sealed record FacultySearchInput(string? Query = null, int? DepartmentId = null,
    string? Type = null, string? Status = null, int Page = 1, int PageSize = 20);
public sealed record FacultySummaryDto(int Id, string EmployeeRef, string FullName, int DepartmentId,
    string Department, string Designation, string Type, string Status);
public sealed record FacultyDetailDto(int Id, int UserId, string Username, string EmployeeRef, string FullName,
    int DepartmentId, string Department, string Designation, string Type, string Status,
    string? Qualifications, string? Phone, string? Email, bool HasPhoto);
public sealed record FacultySaveDto(int DepartmentId, string EmployeeRef, string FullName, string Designation,
    string Type, string Status, string? Qualifications, string? Phone, string? Email);
public sealed record FacultyCreatedDto(int Id, string Username, string TemporaryPassword);
public sealed record FacultyPhotoDto(byte[] Data, string ContentType);
public sealed record TeachingAssignmentDto(int OfferingId, int TermId, string TermLabel, string CourseCode,
    string CourseTitle, string Section, decimal Credits, string Status, DateTime AssignedOn, DateTime? EndedOn);
public sealed record FacultyDashboardDto(FacultyDetailDto Profile, IReadOnlyList<TeachingAssignmentDto> Courses,
    int ActiveMentorAssignments);
public sealed record FacultyWorkloadRowDto(int FacultyId, string EmployeeRef, string FacultyName,
    int TermId, string TermLabel, decimal Credits, int OfferingCount);
public sealed record MentorAssignmentDto(int Id, int FacultyId, string FacultyName, int BatchId,
    short AdmissionYear, int? SectionId, string? Section, DateTime AssignedOn, DateTime? EndedOn);
public sealed record MentorSaveDto(int FacultyId, int BatchId, int? SectionId);
public sealed record MenteeSummaryDto(string StudentId, string FullName, short RollNo, string Section, string Status);
public sealed record MentorBatchOptionDto(int Id, string Label, IReadOnlyList<MentorSectionOptionDto> Sections);
public sealed record MentorSectionOptionDto(int Id, string Name);
public sealed record MentorFacultyOptionDto(int Id, string Label);
public sealed record MentorOptionsDto(IReadOnlyList<MentorBatchOptionDto> Batches,
    IReadOnlyList<MentorFacultyOptionDto> Faculty);
