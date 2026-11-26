using UA.DataAccess.Interface.Common;
using UA.Entities;
namespace UA.DataAccess.Interface.Finance;
public record DueProjection(int Id,string StudentId,string StudentName,int DepartmentId,int BatchId,int SectionId,int? TermId,string Fee,decimal Amount,decimal Paid,DateTime Deadline,string Status);
public interface IDueRepository : IRepository<Due>
{
    Task<IReadOnlyList<DueProjection>> RowsAsync(int[] allowedDepartments,string? studentId=null,int? departmentId=null,int? batchId=null,int? sectionId=null,string? status=null,CancellationToken ct=default);
}
