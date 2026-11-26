using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.Interface.Admission;
using UA.Facade.Interface.Integration;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Admission;
public sealed class AdmissionMaintenance(IRepository<AdmissionCircular> circulars,IRepository<Application> applications,IRepository<Applicant> applicants,IRepository<UserAccount> users,IRepository<UserRole> userRoles,IRepository<Role> roles,IRepository<UA.Entities.Student> students,IRepository<UA.Entities.Faculty> faculty,IRepository<UploadedDocument> docs,IRepository<VerificationToken> tokens,IRepository<Notification> notifications,IRepository<NotificationTemplate> templates,IApplicantStorage storage,IPasswordHasher passwords,IUnitOfWork work,IClock clock):IAdmissionMaintenance
{
 public async Task RemindAsync(CancellationToken ct)
 {
  var today=clock.Today;var dates=new[]{today.AddDays(7),today.AddDays(3),today.AddDays(1)};
  var cs=await circulars.ListAsync(c=>dates.Contains(c.EnrollmentDeadline)&&(c.Status=="Published"||c.Status=="Closed"),ct);
  foreach(var c in cs)await work.ExecuteTransactionAsync(async token=>{
   var template=await templates.FirstAsync(t=>t.Code=="admission.deadline"&&t.IsActive,token);if(template==null)return;
   var aa=await applications.ListAsync(a=>a.CircularId==c.CircularId&&a.ApplicationStatus=="Result published"&&(a.SelectionStatus=="Selected"||a.SelectionStatus=="Replacement"),token);
   foreach(var a in aa){var p=await applicants.FirstAsync(x=>x.ApplicantId==a.ApplicantId,token);if(p==null)continue;var u=await users.FirstAsync(x=>x.UserId==p.UserId&&x.Status=="Active",token);if(u==null)continue;
    var key=$"admission.deadline:{a.ApplicationId}:{c.EnrollmentDeadline:yyyyMMdd}:{(c.EnrollmentDeadline-today).Days}";
    if(await notifications.FirstAsync(n=>n.RecipientUserId==u.UserId&&n.EventKey==key,token)==null)await notifications.AddAsync(new Notification{RecipientUserId=u.UserId,TemplateId=template.TemplateId,EventKey=key,Status="Queued",QueuedAt=clock.Now},token);
   }
  },ct);
 }
 public async Task AnonymizeAsync(CancellationToken ct)
 {
  var cutoff=clock.Today.AddYears(-2);var ids=(await applications.ListAsync(a=>a.SubmittedAt<cutoff,ct)).Select(a=>a.ApplicantId).Distinct().ToArray();
  foreach(var id in ids)await work.ExecuteTransactionAsync(async token=>{
   var p=await applicants.FirstAsync(x=>x.ApplicantId==id,token);if(p==null||p.SscBoard=="ANONYMIZED")return;
   var aa=await applications.ListAsync(a=>a.ApplicantId==id,token);if(aa.Count==0||aa.Any(a=>a.SubmittedAt>=cutoff||a.ApplicationStatus=="Enrolled"))return;
   var cids=aa.Select(a=>a.CircularId).ToArray();var cs=await circulars.ListAsync(c=>cids.Contains(c.CircularId),token);if(cs.Count!=cids.Distinct().Count()||cs.Any(c=>c.EnrollmentDeadline>=cutoff))return;
   var aids=aa.Select(a=>a.ApplicationId).ToArray();if(await students.CountAsync(s=>s.UserId==p.UserId||(s.ApplicationId!=null&&aids.Contains(s.ApplicationId.Value)),token)>0||await faculty.CountAsync(f=>f.UserId==p.UserId,token)>0)return;
   var u=await users.FirstAsync(x=>x.UserId==p.UserId,token);if(u==null)return;
   var role=await roles.FirstAsync(r=>r.Name==UA.Framework.Constants.RoleNames.Applicant,token);var grants=await userRoles.ListAsync(r=>r.UserId==u.UserId,token);if(role==null||grants.Count==0||grants.Any(r=>r.RoleId!=role.RoleId))return;
   var records=await docs.ListAsync(d=>d.ApplicantId==id,token);if(records.Any(d=>d.StudentId!=null||!d.FilePath.StartsWith("applicant-files/",StringComparison.Ordinal))||(!string.IsNullOrEmpty(p.PhotoPath)&&!p.PhotoPath.StartsWith("applicant-files/",StringComparison.Ordinal)))return;
   // Delete only applicant-private files, never a shared student document. Retrying is safe if a file was already removed.
   foreach(var d in records){if(d.FilePath.StartsWith("applicant-files/",StringComparison.Ordinal))await storage.DeleteApplicantAsync(d.FilePath,token);else return;}
   if(!string.IsNullOrEmpty(p.PhotoPath)){if(!p.PhotoPath.StartsWith("applicant-files/",StringComparison.Ordinal))return;await storage.DeleteApplicantAsync(p.PhotoPath,token);}
   foreach(var d in records)docs.Remove(d);
   foreach(var t in await tokens.ListAsync(t=>t.UserId==u.UserId,token))tokens.Remove(t);
   p.FullName="Anonymized applicant";p.DateOfBirth=new DateTime(1900,1,1);p.Gender="Other";p.Address="Anonymized";p.GuardianName="Anonymized";p.GuardianPhone="Anonymized";p.SscBoard="ANONYMIZED";p.HscBoard="ANONYMIZED";p.SscRoll=id.ToString(System.Globalization.CultureInfo.InvariantCulture);p.HscRoll=p.SscRoll;p.SscResult=0;p.HscResult=0;p.PhotoPath="";
   u.Username="retired-applicant-"+Guid.NewGuid().ToString("N")[..24];u.Email=null;u.Mobile=null;u.PasswordHash=passwords.Hash(Guid.NewGuid().ToString("N")+"!Aa9");u.IsVerified=false;u.Status="Inactive";u.LockedUntil=null;u.FailedLoginCount=0;
  },ct);
 }
}
