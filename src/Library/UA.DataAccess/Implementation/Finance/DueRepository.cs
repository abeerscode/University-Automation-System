using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Interface.Finance;
using UA.Entities;
namespace UA.DataAccess.Implementation.Finance;
public sealed class DueRepository(UniversityDbContext db) : Repository<Due>(db), IDueRepository
{
    public async Task<IReadOnlyList<DueProjection>> RowsAsync(int[] allowedDepartments,string? studentId=null,int? departmentId=null,int? batchId=null,int? sectionId=null,string? status=null,CancellationToken ct=default)
    {
        var query=db.Dues.AsNoTracking().Where(d=>d.Student!=null && allowedDepartments.Contains(d.Student.DepartmentId));
        if(studentId is not null) query=query.Where(d=>d.StudentId==studentId);
        if(departmentId.HasValue) query=query.Where(d=>d.Student!.DepartmentId==departmentId.Value);
        if(batchId.HasValue) query=query.Where(d=>d.Student!.CurrentBatchId==batchId.Value);
        if(sectionId.HasValue) query=query.Where(d=>d.Student!.SectionId==sectionId.Value);
        if(!string.IsNullOrWhiteSpace(status)) query=query.Where(d=>d.Status==status);
        return await query.OrderBy(d=>d.DueDate).ThenBy(d=>d.DueId).Select(d=>new DueProjection(d.DueId,d.StudentId!,d.Student!.FullName,d.Student.DepartmentId,d.Student.CurrentBatchId,d.Student.SectionId,d.TermId,d.FeeType.Name,d.Amount,d.PaymentAllocations.Where(a=>a.Payment.Status=="Successful").Sum(a=>(decimal?)a.Amount)??0m,d.DueDate,d.Status)).ToListAsync(ct);
    }
}
