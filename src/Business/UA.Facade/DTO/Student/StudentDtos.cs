namespace UA.Facade.DTO.Student;

public sealed record StudentSearchInput(string? Query, int? DepartmentId, int? BatchId, int? SectionId,
    string? Status, int Page = 1, int PageSize = 25);
public sealed record StudentListItem(string StudentId, string FullName, string DepartmentCode,
    int DepartmentId, int AdmissionYear, int CurrentBatchYear, string Section, string Status, short RollNo);
public sealed record StudentStatusItem(string Status, DateTime EffectiveDate, string Reason,
    string? ApprovalReference, int ChangedBy);
public sealed record StudentPlacementItem(int BatchId, int AdmissionYear, int SectionId, string Section,
    DateTime From, DateTime? To, string Reason, string? ApprovalReference);
public sealed record StudentPreviousEducationDto(string SscBoard, string SscRoll,
    decimal SscResult, string HscBoard, string HscRoll, decimal HscResult);
public sealed record StudentDocumentItem(int Id, string Type, string MimeType, DateTime UploadedAt);
public sealed record StudentDetailDto(string StudentId, int UserId, string FullName, int DepartmentId,
    string DepartmentCode, int AdmissionBatchId, int CurrentBatchId, int SectionId,
    string Section, short RollNo, DateTime DateOfBirth, string Gender, string GuardianName,
    string GuardianPhone, string Phone, string Address, bool HasPhoto, string Status,
    IReadOnlyList<StudentStatusItem> StatusHistory, IReadOnlyList<StudentPlacementItem> BatchHistory,
    IReadOnlyList<StudentDocumentItem> Documents, StudentPreviousEducationDto? PreviousEducation);
public sealed record StudentEditInput(string StudentId, string FullName, DateTime DateOfBirth,
    string Gender, string GuardianName, string GuardianPhone, string Phone, string Address,
    string? ApprovalReference);
public sealed record StudentStatusChangeInput(string StudentId, string Status, DateTime EffectiveDate,
    string Reason, string ApprovalReference);
public sealed record SectionChangeInput(string StudentId, int SectionId, string Reason,
    string ApprovalReference);
public sealed record StudentRegistrationInput(int? ApplicationId, int DepartmentId, int AdmissionBatchId,
    int SectionId, short RollNo, string FullName, DateTime DateOfBirth, string Gender,
    string GuardianName, string GuardianPhone, string Phone, string Address, string? Email);
public sealed record StudentCreatedCredential(string StudentId, string TemporaryPassword);
public sealed record StudentImportRow(int Line, StudentRegistrationInput Data);
public sealed record StudentImportIssue(int Line, string Message);
public sealed record StudentImportPreview(int ValidRows, IReadOnlyList<StudentImportIssue> Issues);
public sealed record StudentImportResult(int Imported, IReadOnlyList<StudentCreatedCredential> Credentials);
public sealed record StudentImportSheet(IReadOnlyList<string> Headers, IReadOnlyList<StudentImportRawRow> Rows);
public sealed record StudentImportRawRow(int Line, IReadOnlyDictionary<string, string> Values);
public sealed record StudentPhotoDto(byte[] Data, string ContentType);
