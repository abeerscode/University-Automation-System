using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.Interface.Finance;
using UA.Facade.Interface.Administration;
using UA.Facade.Rules.Finance;
using UA.Framework.Exceptions;
using UA.Framework.Security;
namespace UA.Facade.Implementation.Finance;
public sealed class FeeWaiverFacade(IRepository<UA.Entities.Student> students,IRepository<Department> departments,IRepository<FeeWaiver> waivers,IRepository<ApprovalRecord> approvals,IRepository<Due> dues,IRepository<FeeType> rates,IRepository<PaymentAllocation> allocations,IRepository<AcademicTerm> terms,IUnitOfWork work,IUserAccessFacade access,ICurrentUser current,IManualAuditService audit):IFeeWaiverFacade
{
 public async Task ApplyAsync(string studentId,int termId,decimal discount,int approvalId,CancellationToken ct=default)
 {
  if(discount<=0||discount>99999999.99m||decimal.Round(discount,2)!=discount)throw new BusinessRuleException("Enter a positive discount with up to two decimal places.");
  await work.ExecuteTransactionAsync(async token=>{
   var s=await students.FirstAsync(s=>s.StudentId==studentId,token)??throw new NotFoundException();await access.RequireAsync("waivers.manage",s.DepartmentId,token);
   if(s.Status!="Active"||await terms.FirstAsync(t=>t.TermId==termId,token)==null)throw new BusinessRuleException("Active student and valid term required.");
   var d=await departments.FirstAsync(x=>x.DepartmentId==s.DepartmentId,token)??throw new NotFoundException();var a=await approvals.FirstAsync(x=>x.ApprovalId==approvalId&&x.Status=="Approved"&&x.ApprovalType=="FeeWaiver",token)??throw new BusinessRuleException("Record an approved FeeWaiver reference first.");
   if(a.ApproverId==current.UserId||(!a.ReferenceNo.StartsWith(d.Code+"-",StringComparison.OrdinalIgnoreCase)&&!a.ReferenceNo.StartsWith("UNI-",StringComparison.OrdinalIgnoreCase))||await approvals.CountAsync(x=>x.SupersedesId==approvalId,token)>0)throw new BusinessRuleException("Use an independent, current approval for this department.");
   if(await waivers.CountAsync(x=>(x.StudentId==studentId&&x.TermId==termId)||x.ApprovalId==approvalId,token)>0)throw new BusinessRuleException("A waiver for this term or approval already exists.");
   var feeIds=(await rates.ListAsync(r=>r.DepartmentId==s.DepartmentId&&(r.Name==FeeNames.Registration||r.Name==FeeNames.Tuition),token)).Select(r=>r.FeeTypeId).ToArray();
   var lines=await dues.ListAsync(x=>x.StudentId==studentId&&x.TermId==termId&&feeIds.Contains(x.FeeTypeId)&&x.Status=="Unpaid",token);var keys=lines.Select(x=>x.DueId).ToArray();var allocated=(await allocations.ListAsync(x=>keys.Contains(x.DueId),token)).Select(x=>x.DueId).ToHashSet();
   var eligible=lines.Where(x=>!allocated.Contains(x.DueId)).OrderBy(x=>x.DueDate).ThenBy(x=>x.DueId).ToArray();if(discount>eligible.Sum(x=>x.Amount))throw new BusinessRuleException("Discount exceeds wholly unpaid registration and tuition charges; paid or allocated charges require a future reversal workflow.");
   var remaining=discount;foreach(var line in eligible){if(remaining==0)break;var amount=Math.Min(remaining,line.Amount);line.Amount-=amount;remaining-=amount;if(line.Amount==0)line.Status="Waived";}
   await waivers.AddAsync(new FeeWaiver{StudentId=studentId,TermId=termId,ApprovalId=approvalId,DiscountAmount=discount},token);await audit.LogAsync("Fee waiver","Student",studentId,$"Term {termId}; discount {discount:F2}; approval {approvalId}",token);
  },ct);
 }
}
