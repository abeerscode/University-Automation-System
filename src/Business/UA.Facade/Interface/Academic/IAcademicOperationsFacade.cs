using UA.Facade.DTO.Academic;
namespace UA.Facade.Interface.Academic;
public interface IAcademicOperationsFacade
{
    Task<OperationsPage> ListAsync(int? termId = null, int? batchId = null, CancellationToken ct = default);
    Task GenerateAsync(int batchId, int termId, CancellationToken ct = default);
    Task SetOfferingStatusAsync(int offeringId, string status, CancellationToken ct = default);
    Task AssignAsync(int offeringId, int facultyId, bool end, CancellationToken ct = default);
    Task DesignateRetakeMarkerAsync(int offeringId,int facultyId,CancellationToken ct=default);
    Task SaveRoutineAsync(int offeringId, int routineId, int roomId, byte day, TimeSpan start, TimeSpan end, CancellationToken ct = default);
    Task<int> QueueRegistrationAsync(int batchId, int termId, CancellationToken ct = default);
    Task ConfirmAsync(int batchId, int termId, CancellationToken ct = default);
    Task<IReadOnlyList<RegistrationJobListRow>> RegistrationJobsAsync(int batchId, CancellationToken ct = default);
    Task<RegistrationJobRow> JobAsync(int id, CancellationToken ct = default);
    Task<OfferingDetail> DetailAsync(int offeringId, bool facultyPortal = false, CancellationToken ct = default);
    Task<AttendanceSession> SessionAsync(int offeringId, int routineId, DateTime date, CancellationToken ct = default);
    Task AuthorizeExportAsync(int batchId, int? offeringId = null, CancellationToken ct = default, string format = "CSV");
    Task RecordAttendanceAsync(int offeringId, int routineId, DateTime date, int[] presentIds, int[] enrollmentIds, CancellationToken ct = default);
    Task<IReadOnlyList<SectionStudent>> SectionReportAsync(int batchId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentCourseRow>> OwnCoursesAsync(CancellationToken ct = default);
}
public interface IRegistrationJobProcessor
{
    Task ScheduleOpenWindowsAsync(CancellationToken ct = default);
    Task<int?> NextAsync(CancellationToken ct = default);
    Task ProcessAsync(int id, CancellationToken ct = default);
    Task FailAsync(int id, string reason, CancellationToken ct = default);
}
