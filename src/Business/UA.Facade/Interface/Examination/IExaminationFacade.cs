using UA.Facade.DTO.Examination;
namespace UA.Facade.Interface.Examination;
public interface IExaminationFacade
{
    Task<ExamHome> HomeAsync(CancellationToken ct=default);
    Task<EventPage> EventAsync(int id,CancellationToken ct=default);
    Task<int> CreateEventAsync(int termId,string type,CancellationToken ct=default);
    Task<int> CreateSpecialEventAsync(int termId,DateTime date,CancellationToken ct=default);
    Task GenerateRoutineAsync(int id,TimeSpan start,TimeSpan end,int minutes,CancellationToken ct=default);
    Task EditSittingAsync(int id,int sittingId,DateTime date,TimeSpan start,TimeSpan end,CancellationToken ct=default);
    Task MoveSeatAsync(int eventId,int sittingId,string studentId,int roomId,int seatNo,CancellationToken ct=default);
    Task ConfirmClashAsync(int eventId,int sittingId,string studentId,string reason,CancellationToken ct=default);
    Task AssignMakeupAsync(int eventId,int sittingId,string studentId,DateTime date,int roomId,int seatNo,CancellationToken ct=default);
    Task ReleaseAsync(int id,string reason,CancellationToken ct=default);
    Task<int> QueueAsync(int id,string kind,CancellationToken ct=default);
    Task<ExamJob> JobAsync(int id,CancellationToken ct=default);
    Task<MarksPage> MarksAsync(int id,CancellationToken ct=default);
    Task SaveMarksAsync(int id,IReadOnlyList<MarkInput> input,CancellationToken ct=default);
    Task SetDispositionAsync(int offeringId,int enrollmentId,string status,string reason,CancellationToken ct=default);
    Task TransitionAsync(int id,string action,string? comment,CancellationToken ct=default);
    Task<SeatRow[]> AdmitSeatsAsync(int eventId,CancellationToken ct=default);
    Task<StudentExamPage> StudentAsync(string? studentId=null,CancellationToken ct=default);
    Task AuditExportAsync(string document,string identifier,CancellationToken ct=default);
    Task<ResultRow[]> ReportAsync(int termId,int? batchId,CancellationToken ct=default);
    Task CorrectAsync(int id,decimal total,string status,string reason,int approvalId,CancellationToken ct=default);
}
public interface IExamJobProcessor
{
    Task<int?> NextAsync(CancellationToken ct);
    Task ProcessAsync(int id,CancellationToken ct);
    Task FailAsync(int id,string reason,CancellationToken ct);
}
