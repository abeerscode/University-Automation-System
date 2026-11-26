using UA.Facade.DTO.Student;
namespace UA.Facade.Interface.Student;
public interface ILifecycleFacade
{
    Task<LifecycleRow[]> ListAsync(CancellationToken ct=default);
    Task<RetakeShortlistRow[]> ShortlistAsync(CancellationToken ct=default);
    Task AuditGraduationExportAsync(CancellationToken ct=default);
    Task<LifecycleDetails> DetailsAsync(string? id,CancellationToken ct=default);
    Task RequestDropAsync(string id,int termId,CancellationToken ct=default);
    Task RecommendDropAsync(string id,CancellationToken ct=default);
    Task DecideDropAsync(string id,int termId,string reference,CancellationToken ct=default);
    Task RejoinAsync(string id,int termId,CancellationToken ct=default);
    Task EquivalenceAsync(string id,int oldCourseId,int newCourseId,string reference,CancellationToken ct=default);
    Task GraduateAsync(string id,bool alumni,CancellationToken ct=default);
    Task DecideLimitAsync(string id,string status,string reference,CancellationToken ct=default);
    Task RetakeAsync(string id,int offeringId,string? reference,string? reason,CancellationToken ct=default);
    Task SpecialRetakeAsync(string id,int courseId,int termId,CancellationToken ct=default);
    Task<RetakeSchemePage> SchemesAsync(CancellationToken ct=default);
    Task SaveSchemeAsync(int departmentId,int termId,string type,RetakeComponentInput[] parts,CancellationToken ct=default);
    Task RequestDocumentAsync(string type,CancellationToken ct=default);
    Task ProcessDocumentAsync(int id,bool issue,CancellationToken ct=default);
}
public interface ILifecycleWarningScan { Task ScanAsync(CancellationToken ct=default); }
