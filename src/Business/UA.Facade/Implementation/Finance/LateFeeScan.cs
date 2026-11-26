using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.Interface.Finance;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Finance;
public sealed class LateFeeScan(IRepository<Due> dues,IRepository<FeeType> rates,IRepository<AcademicTerm> terms,IRepository<UA.Entities.Student> students,IDueGenerator generator,IUnitOfWork work,IClock clock):ILateFeeScan
{
 public async Task ScanAsync(CancellationToken ct){var today=clock.Today;var ts=(await terms.ListAsync(t=>t.StartDate<=today&&t.EndDate>=today,ct)).Select(t=>t.TermId).ToArray();var feeIds=(await rates.ListAsync(r=>r.Name=="Registration"||r.Name=="Tuition",ct)).Select(r=>r.FeeTypeId).ToArray();var dd=await dues.ListAsync(d=>d.StudentId!=null&&d.TermId!=null&&ts.Contains(d.TermId.Value)&&feeIds.Contains(d.FeeTypeId)&&d.DueDate<today&&(d.Status=="Unpaid"||d.Status=="Partially paid"),ct);var ids=dd.Select(d=>d.StudentId!).Distinct().ToArray();var active=(await students.ListAsync(s=>ids.Contains(s.StudentId)&&s.Status=="Active",ct)).Select(s=>s.StudentId).ToHashSet();foreach(var pair in dd.Where(d=>active.Contains(d.StudentId!)).Select(d=>(Student:d.StudentId!,Term:d.TermId!.Value)).Distinct())await work.ExecuteTransactionAsync(t=>generator.GenerateLateFeeAsync(pair.Student,pair.Term,t),ct);}
}
