using System.Globalization;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Finance;
using UA.Facade.Rules.Academic;
using UA.Facade.Rules.Faculty;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Academic;

public sealed class AcademicOperationsFacade(
    IRepository<Batch> batches, IRepository<Section> sections, IRepository<Department> departments,
    IRepository<AcademicTerm> terms, IRepository<CurriculumCourse> curriculum, IRepository<Course> courses,
    IRepository<Offering> offerings, IRepository<OfferingTeacher> assignments, IRepository<UA.Entities.Faculty> faculty,
    IRepository<Room> rooms, IRepository<ClassRoutine> routines, IRepository<Enrollment> enrollments,
    IRepository<UA.Entities.Student> students, IRepository<SemesterDrop> drops, IRepository<CourseEquivalence> equivalents, IRepository<AttendanceRecord> attendance,
    IRepository<AttendanceScale> scales, IRepository<MarkComponent> components, IRepository<Result> results,
    IRepository<CoursePrerequisite> prerequisites, IRepository<AcademicCalendarEvent> calendar,
    IRepository<BackgroundJob> jobs, IRepository<AuditEntry> audits, IUnitOfWork work, IUserAccessFacade access,
    IConfigurationFacade config, ICurrentUser current, IClock clock, ITeacherAssignmentGuard teacherGuard,
    IDueGenerator dues, IAttendanceMarkConverter converter, IRepository<UserRole> userRoles, IRepository<RolePermission> rolePermissions) : IAcademicOperationsFacade, IRegistrationJobProcessor
{
    private int Actor => current.UserId ?? throw new ForbiddenException();
    private async Task<Batch> BatchAsync(int id, CancellationToken ct) => await batches.FirstAsync(b => b.BatchId == id, ct) ?? throw new NotFoundException("Batch not found.");
    private async Task<AcademicTerm> TermAsync(int id, CancellationToken ct) => await terms.FirstAsync(t => t.TermId == id, ct) ?? throw new NotFoundException("Term not found.");
    private async Task<Offering> OfferingAsync(int id, CancellationToken ct) => await offerings.FirstAsync(o => o.OfferingId == id, ct) ?? throw new NotFoundException("Offering not found.");
    private Task<bool> Allowed(string permission, int dept, CancellationToken ct) => access.AllowedAsync(Actor, permission, dept, ct);
    private async Task<bool> CanRead(int dept, CancellationToken ct) => await Allowed("offerings.manage", dept, ct) || await Allowed("reports.view", dept, ct) || await Allowed("assignments.manage", dept, ct) || await Allowed("routine.manage", dept, ct);
    private async Task Log(string action, string entity, string id, string detail, int? actor, CancellationToken ct) => await audits.AddAsync(new AuditEntry { UserId=actor, Action=action, EntityName=entity, EntityId=id, NewValue=detail, LoggedAt=clock.Now }, ct);
    private static int Semester(Batch b, AcademicTerm t)
    {
        var n=AcademicOperationRules.Semester(b.AdmissionYear,t.Year,t.TermNo);
        if(n is <1 or >8) throw new BusinessRuleException("Term is outside the batch curriculum."); return n;
    }
    private void Window(AcademicTerm t)
    { if(clock.Today<t.StartDate.Date || clock.Today>t.RegistrationDeadline.Date) throw new BusinessRuleException("Registration is outside the term's registration window."); }
    private async Task<OfferingRow> Row(Offering o, Batch b, CancellationToken ct, bool portal=false)
    {
        var c=await courses.FirstAsync(x=>x.CourseId==o.CourseId,ct) ?? throw new NotFoundException();
        var s=await sections.FirstAsync(x=>x.SectionId==o.SectionId,ct) ?? throw new NotFoundException();
        return new(o.OfferingId,o.TermId,o.BatchId,o.SectionId,$"{c.Code} — {c.Title}{(o.IsSpecialRetake?" (Special retake)":"")}",s.Name,o.SemesterNo,o.Status,
            !portal && await Allowed("offerings.manage",b.DepartmentId,ct),!portal && await Allowed("assignments.manage",b.DepartmentId,ct),!portal && await Allowed("routine.manage",b.DepartmentId,ct));
    }
    public async Task<OperationsPage> ListAsync(int? termId=null,int? batchId=null,CancellationToken ct=default)
    {
        var actor=Actor; var ds=await departments.ListAsync(ct:ct); var allowed=new HashSet<int>();
        foreach(var d in ds) if(await CanRead(d.DepartmentId,ct)) allowed.Add(d.DepartmentId);
        if(allowed.Count==0) throw new ForbiddenException();
        var bs=await batches.ListAsync(b=>allowed.Contains(b.DepartmentId),ct);
        if(batchId.HasValue && !bs.Any(b=>b.BatchId==batchId)) throw new ForbiddenException();
        var choices=new List<OperationBatch>();
        foreach(var b in bs) choices.Add(new(b.BatchId,b.DepartmentId,$"{ds.First(d=>d.DepartmentId==b.DepartmentId).Code} {b.AdmissionYear}",await Allowed("offerings.manage",b.DepartmentId,ct),await Allowed("registration.run",b.DepartmentId,ct)));
        var ids=bs.Select(b=>b.BatchId).ToArray(); var os=await offerings.ListAsync(o=>ids.Contains(o.BatchId) && (!termId.HasValue || o.TermId==termId) && (!batchId.HasValue || o.BatchId==batchId),ct);
        var rows=new List<OfferingRow>(); foreach(var o in os) rows.Add(await Row(o,bs.First(b=>b.BatchId==o.BatchId),ct));
        var ts=await terms.ListAsync(ct:ct);
        return new(ts.OrderByDescending(t=>t.Year).ThenByDescending(t=>t.TermNo).Select(t=>new AcademicOption(t.TermId,$"{t.Year} / {t.TermNo}")).ToArray(),choices,rows);
    }
    public async Task GenerateAsync(int batchId,int termId,CancellationToken ct=default)
    {
        var b=await BatchAsync(batchId,ct); await access.RequireAsync("offerings.manage",b.DepartmentId,ct); var t=await TermAsync(termId,ct); int n=Semester(b,t);
        await work.ExecuteTransactionAsync(async token=>{
            var ss=await sections.ListAsync(s=>s.BatchId==batchId,token); var cs=await curriculum.ListAsync(c=>c.CurriculumId==b.CurriculumId && c.SemesterNo==n,token);
            if(ss.Count==0 || cs.Count==0) throw new BusinessRuleException("Create sections and curriculum courses first.");
            var existing=await offerings.ListAsync(o=>!o.IsSpecialRetake && o.BatchId==batchId && o.TermId==termId,token);
            foreach(var s in ss) foreach(var c in cs) if(!existing.Any(o=>o.SectionId==s.SectionId && o.CourseId==c.CourseId)) await offerings.AddAsync(new Offering { BatchId=batchId,TermId=termId,SectionId=s.SectionId,CourseId=c.CourseId,SemesterNo=(byte)n,Status="Planned" },token);
            await Log("Generate offerings","Batch",batchId.ToString(),$"Term {termId}",Actor,token);
        },ct);
    }
    public async Task SetOfferingStatusAsync(int offeringId,string status,CancellationToken ct=default)
    {
        var o=await OfferingAsync(offeringId,ct); var b=await BatchAsync(o.BatchId,ct); await access.RequireAsync("offerings.manage",b.DepartmentId,ct);
        if(status is not ("Planned" or "Active" or "Closed")) throw new BusinessRuleException("Invalid offering status.");
        await work.ExecuteTransactionAsync(async token=>{
            if(o.Status=="Closed" && status!="Closed") throw new BusinessRuleException("Closed offerings cannot be reopened.");
            if(status=="Planned" && await enrollments.CountAsync(e=>e.OfferingId==offeringId,token)>0) throw new BusinessRuleException("An enrolled offering cannot return to Planned.");
            o.Status=status;
        },ct);
    }
    public async Task AssignAsync(int offeringId,int facultyId,bool end,CancellationToken ct=default)
    {
        var o=await OfferingAsync(offeringId,ct); var b=await BatchAsync(o.BatchId,ct); await access.RequireAsync("assignments.manage",b.DepartmentId,ct);
        await work.ExecuteTransactionAsync(async token=>{
            if(o.Status=="Closed") throw new BusinessRuleException("Offering closed.");
            var f=await faculty.FirstAsync(f=>f.FacultyId==facultyId,token) ?? throw new NotFoundException();
            var a=await assignments.FirstAsync(a=>a.OfferingId==offeringId && a.FacultyId==facultyId,token);
            if(end){if(a is null || a.EndedOn.HasValue) throw new BusinessRuleException("No active assignment."); a.EndedOn=clock.Today; a.IsRetakeMarker=false; return;}
            if(f.Status!="Active") throw new BusinessRuleException("Only active faculty may be assigned.");
            if(a is not null && a.EndedOn is null) return;
            var own=await routines.ListAsync(r=>r.OfferingId==offeringId,token);
            var otherAssignments=await assignments.ListAsync(x=>x.FacultyId==facultyId && x.OfferingId!=offeringId && x.AssignedOn<=clock.Today && (x.EndedOn==null || x.EndedOn>clock.Today),token);
            var otherIds=otherAssignments.Select(x=>x.OfferingId).ToArray(); var otherOffers=await offerings.ListAsync(x=>otherIds.Contains(x.OfferingId) && x.TermId==o.TermId && x.Status!="Closed",token);
            var activeIds=otherOffers.Select(x=>x.OfferingId).ToArray(); var other=await routines.ListAsync(r=>activeIds.Contains(r.OfferingId),token);
            if(own.Any(r=>other.Any(q=>r.DayOfWeek==q.DayOfWeek && AcademicOperationRules.Overlaps(r.StartTime,r.EndTime,q.StartTime,q.EndTime)))) throw new BusinessRuleException("Teacher has a conflicting class in this term.");
            if(a is null) await assignments.AddAsync(new OfferingTeacher{OfferingId=offeringId,FacultyId=facultyId,AssignedOn=clock.Today},token);
            else { await Log("Reassign teacher","OfferingTeacher",$"{offeringId},{facultyId}",$"Previous interval {a.AssignedOn:yyyy-MM-dd} to {a.EndedOn:yyyy-MM-dd}",Actor,token); a.AssignedOn=clock.Today;a.EndedOn=null; }
        },ct);
    }
    public async Task DesignateRetakeMarkerAsync(int offeringId,int facultyId,CancellationToken ct=default)
    {
        var o=await OfferingAsync(offeringId,ct);var b=await BatchAsync(o.BatchId,ct);await access.RequireAsync("assignments.manage",b.DepartmentId,ct);
        await work.ExecuteTransactionAsync(async token=>{if(o.Status=="Closed")throw new BusinessRuleException("Offering closed.");var rows=await assignments.ListAsync(x=>x.OfferingId==offeringId,token);var a=rows.SingleOrDefault(x=>x.FacultyId==facultyId&&x.AssignedOn<=clock.Today&&(x.EndedOn==null||x.EndedOn>clock.Today))??throw new BusinessRuleException("Choose an actively assigned teacher.");var f=await faculty.FirstAsync(x=>x.FacultyId==facultyId&&x.Status=="Active",token)??throw new BusinessRuleException("Teacher is not active.");if(!await access.AllowedAsync(f.UserId,"marks.enter",null,token))throw new BusinessRuleException("Teacher lacks mark entry permission.");foreach(var r in rows)r.IsRetakeMarker=r.FacultyId==facultyId;await Log("Designate retake marker","OfferingTeacher",$"{offeringId},{facultyId}","Designated retake examiner",Actor,token);},ct);
    }
    public async Task SaveRoutineAsync(int offeringId,int routineId,int roomId,byte day,TimeSpan start,TimeSpan end,CancellationToken ct=default)
    {
        var o=await OfferingAsync(offeringId,ct); var b=await BatchAsync(o.BatchId,ct); if(o.IsSpecialRetake)throw new BusinessRuleException("Special retake offerings are exam-only."); await access.RequireAsync("routine.manage",b.DepartmentId,ct);
        if(day is <1 or >7 || start<TimeSpan.Zero || end>=TimeSpan.FromDays(1) || end<=start) throw new BusinessRuleException("Enter valid weekday and times.");
        await work.ExecuteTransactionAsync(async token=>{
            if(o.Status=="Closed") throw new BusinessRuleException("Offering closed.");
            var room=await rooms.FirstAsync(r=>r.RoomId==roomId,token) ?? throw new NotFoundException("Room not found.");
            var ss=await students.CountAsync(s=>s.SectionId==o.SectionId && s.Status=="Active",token); if(ss>room.Capacity) throw new BusinessRuleException("Room capacity is insufficient.");
            var peers=await offerings.ListAsync(x=>x.TermId==o.TermId && x.Status!="Closed",token);var ids=peers.Select(x=>x.OfferingId).ToArray();
            var aa=await assignments.ListAsync(x=>ids.Contains(x.OfferingId) && x.AssignedOn<=clock.Today && (x.EndedOn==null || x.EndedOn>clock.Today),token);
            var teachers=aa.Where(x=>x.OfferingId==offeringId).Select(x=>x.FacultyId).ToHashSet(); if(teachers.Count==0) throw new BusinessRuleException("Assign a teacher first.");
            var slots=await routines.ListAsync(x=>ids.Contains(x.OfferingId) && x.DayOfWeek==day && x.RoutineId!=routineId && x.StartTime<end && start<x.EndTime,token);
            foreach(var r in slots) if(r.RoomId==roomId || peers.First(x=>x.OfferingId==r.OfferingId).SectionId==o.SectionId || aa.Any(a=>a.OfferingId==r.OfferingId && teachers.Contains(a.FacultyId))) throw new BusinessRuleException("Clash detected: room, section or assigned teacher already has a class.");
            var row=routineId==0?new ClassRoutine{OfferingId=offeringId}:await routines.FirstAsync(r=>r.RoutineId==routineId && r.OfferingId==offeringId,token) ?? throw new NotFoundException();
            if(routineId!=0 && await attendance.CountAsync(a=>a.RoutineId==routineId,token)>0) throw new BusinessRuleException("A routine with attendance history cannot be changed.");
            row.RoomId=roomId;row.DayOfWeek=day;row.StartTime=start;row.EndTime=end;
            if(routineId==0) await routines.AddAsync(row,token);
        },ct);
    }
    public async Task<int> QueueRegistrationAsync(int batchId,int termId,CancellationToken ct=default)
    {
        var b=await BatchAsync(batchId,ct);await access.RequireAsync("registration.run",b.DepartmentId,ct);var t=await TermAsync(termId,ct);Semester(b,t);Window(t);
        var type=AcademicOperationRules.JobType(batchId,termId); var autoType=AcademicOperationRules.AutoJobType(batchId,termId); BackgroundJob? job=null;
        await work.ExecuteTransactionAsync(async token=>{
            job=await jobs.FirstAsync(j=>(j.JobType==type || j.JobType==autoType) && (j.Status=="Queued" || j.Status=="Running"),token);
            if(job is not null) return;
            job=new BackgroundJob{JobType=type,RequestedBy=Actor,Status="Queued",CreatedAt=clock.Now};await jobs.AddAsync(job,token);
        },ct); return job!.JobId;
    }
    // Called only by the machine-facing worker. RequestedBy is the responsible existing account,
    // not a claim that the account manually requested this automatic job.
    public async Task ScheduleOpenWindowsAsync(CancellationToken ct=default)
    {
        var today=clock.Today;
        var open=await terms.ListAsync(t=>t.StartDate<=today && t.RegistrationDeadline>=today,ct);
        if(open.Count==0)return;
        var roleIds=(await rolePermissions.ListAsync(p=>p.PermissionCode=="registration.run",ct)).Select(p=>p.RoleId).ToArray();
        var holders=await userRoles.ListAsync(u=>roleIds.Contains(u.RoleId),ct);
        var bs=await batches.ListAsync(ct:ct);
        foreach(var t in open) foreach(var b in bs)
        {
            int semester=AcademicOperationRules.Semester(b.AdmissionYear,t.Year,t.TermNo);
            if(semester is <1 or >8)continue;
            var manual=AcademicOperationRules.JobType(b.BatchId,t.TermId);var automatic=AcademicOperationRules.AutoJobType(b.BatchId,t.TermId);
            await work.ExecuteTransactionAsync(async token=>{
                if(await jobs.FirstAsync(j=>(j.JobType==manual || j.JobType==automatic) && (j.Status=="Queued" || j.Status=="Running" || j.Status=="Completed" || j.JobType==automatic),token) is not null)return;
                var ss=await students.ListAsync(s=>s.CurrentBatchId==b.BatchId && s.Status=="Active",token);
                if(ss.Count==0)return;
                var expected=(await curriculum.ListAsync(c=>c.CurriculumId==b.CurriculumId && c.SemesterNo==semester,token)).Select(c=>c.CourseId).ToHashSet();
                var os=await offerings.ListAsync(o=>!o.IsSpecialRetake && o.BatchId==b.BatchId && o.TermId==t.TermId && o.SemesterNo==semester && o.Status!="Closed",token);
                if(expected.Count==0 || ss.Select(s=>s.SectionId).Distinct().Any(section=>!os.Where(o=>o.SectionId==section).Select(o=>o.CourseId).ToHashSet().SetEquals(expected)))return;
                int? owner=null;
                foreach(var candidate in holders.OrderByDescending(h=>h.DepartmentId==b.DepartmentId).ThenBy(h=>h.UserId).Select(h=>h.UserId).Distinct())
                    if(await access.AllowedAsync(candidate,"registration.run",b.DepartmentId,token)){owner=candidate;break;}
                if(!owner.HasValue)return;
                await jobs.AddAsync(new BackgroundJob{JobType=automatic,RequestedBy=owner.Value,Status="Queued",CreatedAt=clock.Now},token);
                await Log("Auto queue registration","Batch",b.BatchId.ToString(),$"Term {t.TermId}; responsible account {owner.Value}",null,token);
            },ct);
        }
    }
    public async Task<int?> NextAsync(CancellationToken ct=default) => (await jobs.ListAsync(j=>(j.Status=="Queued" || j.Status=="Running") && (j.JobType.StartsWith("Reg:") || j.JobType.StartsWith("AutoReg:")),ct)).OrderBy(j=>j.JobId).FirstOrDefault()?.JobId;
    public async Task ProcessAsync(int id,CancellationToken ct=default)
    {
        // Commit a visible running stage before the batch transaction. A stopped worker can resume it.
        await work.ExecuteTransactionAsync(async token=>{
            var pending=await jobs.FirstAsync(x=>x.JobId==id && x.Status=="Queued" && (x.JobType.StartsWith("Reg:") || x.JobType.StartsWith("AutoReg:")),token);
            if(pending is not null){pending.Status="Running";pending.ProgressPercent=10;}
        },ct);
        await work.ExecuteTransactionAsync(async token=>{
            var j=await jobs.FirstAsync(x=>x.JobId==id && x.Status=="Running",token);if(j is null)return;
            if(!AcademicOperationRules.TryJob(j.JobType,out int batchId,out int termId)) throw new BusinessRuleException("Invalid registration job.");
            var b=await BatchAsync(batchId,token);if(!await access.AllowedAsync(j.RequestedBy,"registration.run",b.DepartmentId,token))throw new ForbiddenException();
            var t=await TermAsync(termId,token);int n=Semester(b,t);
            if(j.CreatedAt.Date<t.StartDate.Date || j.CreatedAt.Date>t.RegistrationDeadline.Date)throw new BusinessRuleException("Job requested outside registration window.");
            j.Status="Running";
            var ss=await students.ListAsync(s=>s.CurrentBatchId==batchId && s.Status=="Active",token);
            var os=await offerings.ListAsync(o=>!o.IsSpecialRetake && o.BatchId==batchId && o.TermId==termId && o.SemesterNo==n && o.Status!="Closed",token);
            var cs=await curriculum.ListAsync(c=>c.CurriculumId==b.CurriculumId && c.SemesterNo==n,token);var expected=cs.Select(c=>c.CourseId).ToHashSet();
            if(expected.Count==0)throw new BusinessRuleException("Curriculum is empty.");
            var ids=os.Select(o=>o.OfferingId).ToArray();var existing=await enrollments.ListAsync(e=>ids.Contains(e.OfferingId),token);
            var keys=existing.Select(e=>(e.StudentId,e.OfferingId)).ToHashSet();var studentIds=ss.Select(s=>s.StudentId).ToArray();
            var dropped=(await drops.ListAsync(d=>studentIds.Contains(d.StudentId) && d.DroppedSemesterNo==n && (d.Status=="Approved"||d.Status=="Requested"||d.Status=="Recommended"),token)).Select(d=>d.StudentId).ToHashSet();
            var admissionIds=ss.Select(s=>s.AdmissionBatchId).Distinct().ToArray();
            var admissions=await batches.ListAsync(x=>admissionIds.Contains(x.BatchId),token);
            int count=0;
            foreach(var s in ss.Where(s=>!dropped.Contains(s.StudentId))) {
                // A-10 / FR-STU-18: original admission batch determines the seven-year limit.
                var admission=admissions.Single(x=>x.BatchId==s.AdmissionBatchId);
                if(clock.Today.Year>admission.AdmissionYear+6 || t.Year>admission.AdmissionYear+6)continue;
                var own=os.Where(o=>o.SectionId==s.SectionId).ToArray();if(!own.Select(o=>o.CourseId).ToHashSet().SetEquals(expected))throw new BusinessRuleException("Generate complete section offerings before registration.");
                var recognized=s.AdmissionBatchId==s.CurrentBatchId?new HashSet<int>():await UA.Facade.Rules.Student.RejoinCreditRecognition.PassedAsync(s.StudentId,enrollments,offerings,results,equivalents,termId,token);
                foreach(var o in own.Where(o=>!recognized.Contains(o.CourseId))) if(keys.Add((s.StudentId,o.OfferingId))) await enrollments.AddAsync(new Enrollment{StudentId=s.StudentId,OfferingId=o.OfferingId,EnrollmentType="Regular",AttemptNo=1,Status="Registered"},token);
                await dues.GenerateSemesterAsync(s.StudentId,termId,token); count++;
            }
            j.Status="Completed";j.ProgressPercent=100;j.FinishedAt=clock.Now;
            await Log("Bulk registration","BackgroundJob",id.ToString(),$"Batch {batchId}, term {termId}, students {count}",j.JobType.StartsWith("AutoReg:")?null:j.RequestedBy,token);
        },ct);
    }
    public async Task FailAsync(int id,string reason,CancellationToken ct=default)
    {
        await work.ExecuteTransactionAsync(async token=>{
            var j=await jobs.FirstAsync(x=>x.JobId==id && (x.Status=="Queued" || x.Status=="Running") && (x.JobType.StartsWith("Reg:") || x.JobType.StartsWith("AutoReg:")),token);
            if(j is null)return;
            j.Status="Failed";j.FinishedAt=clock.Now;
            await Log("Registration failed","BackgroundJob",id.ToString(CultureInfo.InvariantCulture),reason,null,token);
        },ct);
    }
    public async Task<RegistrationJobRow> JobAsync(int id,CancellationToken ct=default)
    {
        var j=await jobs.FirstAsync(x=>x.JobId==id && (x.JobType.StartsWith("Reg:") || x.JobType.StartsWith("AutoReg:")),ct)??throw new NotFoundException();
        if(!AcademicOperationRules.TryJob(j.JobType,out var batchId,out var termId))throw new NotFoundException();
        var b=await BatchAsync(batchId,ct);await access.RequireAsync("registration.run",b.DepartmentId,ct);
        string? reason=null;
        if(j.Status=="Failed")
        {
            var key=id.ToString(CultureInfo.InvariantCulture);
            var logs=await audits.ListAsync(a=>a.EntityName=="BackgroundJob" && a.EntityId==key && a.Action=="Registration failed",ct);
            reason=logs.OrderByDescending(a=>a.AuditId).FirstOrDefault()?.NewValue
                ?? "No recorded failure summary is available. Ask an administrator to check the API logs.";
        }
        return new(j.JobId,j.Status,j.ProgressPercent,reason,j.FinishedAt,batchId,termId);
    }
    public async Task<IReadOnlyList<RegistrationJobListRow>> RegistrationJobsAsync(int batchId,CancellationToken ct=default)
    {
        var b=await BatchAsync(batchId,ct);await access.RequireAsync("registration.run",b.DepartmentId,ct);
        var manual=$"Reg:{batchId}:";var automatic=$"AutoReg:{batchId}:";
        return (await jobs.ListAsync(j=>j.JobType.StartsWith(manual)||j.JobType.StartsWith(automatic),ct))
            .OrderByDescending(j=>j.JobId).Take(100).Select(j=>new RegistrationJobListRow(j.JobId,j.JobType,j.Status,j.ProgressPercent,j.CreatedAt)).ToArray();
    }
    public async Task ConfirmAsync(int batchId,int termId,CancellationToken ct=default)
    {
        var b=await BatchAsync(batchId,ct);await access.RequireAsync("registration.run",b.DepartmentId,ct);var t=await TermAsync(termId,ct);Semester(b,t);
        await work.ExecuteTransactionAsync(async token=>{
            var manual=AcademicOperationRules.JobType(batchId,termId);var automatic=AcademicOperationRules.AutoJobType(batchId,termId);
            if(await jobs.FirstAsync(j=>(j.JobType==manual||j.JobType==automatic)&&(j.Status=="Queued"||j.Status=="Running"),token) is not null)throw new BusinessRuleException("Wait for registration to finish before confirming.");
            var os=await offerings.ListAsync(o=>o.BatchId==batchId && o.TermId==termId && o.Status!="Closed",token);var ids=os.Select(o=>o.OfferingId).ToArray();
            var active=(await students.ListAsync(s=>s.CurrentBatchId==batchId && s.Status=="Active",token)).Select(s=>s.StudentId).ToArray();
            var es=await enrollments.ListAsync(e=>ids.Contains(e.OfferingId)&&active.Contains(e.StudentId)&&e.EnrollmentType=="Regular"&&e.Status=="Registered",token);
            foreach(var e in es)e.Status="Confirmed"; foreach(var o in os.Where(o=>o.Status=="Planned" && es.Any(e=>e.OfferingId==o.OfferingId)))o.Status="Active";
        },ct);
    }
    private async Task<IReadOnlyList<RoutineRow>> RoutineRows(int offeringId,CancellationToken ct)
    {
        var rs=await routines.ListAsync(r=>r.OfferingId==offeringId,ct);var roomIds=rs.Select(r=>r.RoomId).ToArray();var rr=await rooms.ListAsync(r=>roomIds.Contains(r.RoomId),ct);
        return rs.OrderBy(r=>r.DayOfWeek).ThenBy(r=>r.StartTime).Select(r=>new RoutineRow(r.RoutineId,rr.First(x=>x.RoomId==r.RoomId).Name,r.DayOfWeek,r.StartTime,r.EndTime)).ToArray();
    }
    private async Task<IReadOnlyList<EnrollmentRow>> EnrollmentRows(Offering o,Batch b,string? studentId,CancellationToken ct)
    {
        var es=await enrollments.ListAsync(e=>e.OfferingId==o.OfferingId && (studentId==null || e.StudentId==studentId),ct);var ids=es.Select(e=>e.EnrollmentId).ToArray();var sids=es.Select(e=>e.StudentId).ToArray();
        var ss=await students.ListAsync(s=>sids.Contains(s.StudentId),ct);var records=await attendance.ListAsync(a=>ids.Contains(a.EnrollmentId),ct);
        var req=(await prerequisites.ListAsync(p=>p.CourseId==o.CourseId,ct)).Select(p=>p.PrerequisiteCourseId).ToHashSet();
        var previous=await enrollments.ListAsync(e=>sids.Contains(e.StudentId),ct);var poIds=previous.Select(e=>e.OfferingId).Distinct().ToArray();var po=await offerings.ListAsync(x=>poIds.Contains(x.OfferingId)&&req.Contains(x.CourseId),ct);
        var pe=previous.Where(e=>po.Any(p=>p.OfferingId==e.OfferingId)).ToArray();var peIds=pe.Select(e=>e.EnrollmentId).ToArray();var passed=await results.ListAsync(r=>peIds.Contains(r.EnrollmentId)&&r.PublishedAt!=null&&!r.IsReplaced,ct);
        var thresholdText=await config.ResolveAsync(ConfigKeys.AttendanceMinimum,b.DepartmentId,o.TermId,ct);var threshold=decimal.TryParse(thresholdText,NumberStyles.Number,CultureInfo.InvariantCulture,out var v)?Math.Clamp(v,75m,100m):75m;
        var term=await TermAsync(o.TermId,ct);var allBands=await scales.ListAsync(s=>s.DepartmentId==b.DepartmentId && s.EffectiveFrom<=term.StartDate,ct);var version=allBands.OrderByDescending(s=>s.EffectiveFrom).ThenByDescending(s=>s.VersionNo).FirstOrDefault()?.VersionNo;
        var bands=allBands.Where(s=>s.VersionNo==version).Select(s=>(s.MinPercent,s.MarkPercent)).ToArray();var c=await courses.FirstAsync(c=>c.CourseId==o.CourseId,ct)??throw new NotFoundException();
        var parts=await components.ListAsync(p=>p.DepartmentId==b.DepartmentId&&p.CourseType==c.CourseType&&p.EffectiveFrom<=term.StartDate,ct);var markVersion=parts.OrderByDescending(p=>p.EffectiveFrom).ThenByDescending(p=>p.VersionNo).FirstOrDefault()?.VersionNo;var maximum=parts.Where(p=>p.VersionNo==markVersion&&p.IsAttendanceBased).Sum(p=>p.MaxMarks);
        var rows=new List<EnrollmentRow>();foreach(var e in es){var ar=records.Where(a=>a.EnrollmentId==e.EnrollmentId).ToArray();int present=ar.Count(a=>a.Status=="Present");var pct=e.EnrollmentType=="Regular"?AcademicOperationRules.Percentage(present,ar.Length):null;
            var warning=req.Any(p=>!pe.Where(x=>x.StudentId==e.StudentId&&po.Any(q=>q.OfferingId==x.OfferingId&&q.CourseId==p)).Any(x=>passed.Where(r=>r.EnrollmentId==x.EnrollmentId).OrderByDescending(r=>r.VersionNo).FirstOrDefault() is { GradePoint: >0 }));
            rows.Add(new(e.EnrollmentId,e.StudentId,ss.First(s=>s.StudentId==e.StudentId).FullName,e.EnrollmentType,e.Status,warning,present,ar.Length,pct,AcademicOperationRules.IsShort(pct,threshold),pct.HasValue&&bands.Length>0&&markVersion.HasValue?converter.Convert(pct.Value,maximum,bands):null,version));}
        return rows.OrderBy(e=>e.Type).ThenBy(e=>e.StudentId).ToArray();
    }
    public async Task<OfferingDetail> DetailAsync(int offeringId,bool facultyPortal=false,CancellationToken ct=default)
    {
        var o=await OfferingAsync(offeringId,ct);var b=await BatchAsync(o.BatchId,ct);
        if(facultyPortal)await teacherGuard.RequireAssignedAsync(offeringId,ct);else if(!await CanRead(b.DepartmentId,ct))throw new ForbiddenException();
        var row=await Row(o,b,ct,facultyPortal);var aa=await assignments.ListAsync(a=>a.OfferingId==offeringId,ct);var fids=aa.Select(a=>a.FacultyId).ToArray();var ff=await faculty.ListAsync(f=>fids.Contains(f.FacultyId),ct);
        var studentsRows=await EnrollmentRows(o,b,null,ct);if(facultyPortal)studentsRows=studentsRows.Where(e=>e.Type=="Regular").ToArray();
        var history=new List<AssignmentHistoryRow>();
        if(!facultyPortal)
        {
            var prefix=$"{offeringId},";
            var prior=await audits.ListAsync(a=>a.EntityName=="OfferingTeacher" && a.Action=="Reassign teacher" && a.EntityId.StartsWith(prefix),ct);
            foreach(var entry in prior.OrderByDescending(a=>a.LoggedAt))
            {
                if(!int.TryParse(entry.EntityId[prefix.Length..],out var facultyId))continue;
                var teacher=ff.FirstOrDefault(f=>f.FacultyId==facultyId)?.FullName??$"Faculty {facultyId}";
                history.Add(new(teacher,entry.NewValue??"Previous assignment",entry.LoggedAt));
            }
        }
        return new(row,aa.Select(a=>new AssignmentRow(a.FacultyId,ff.First(f=>f.FacultyId==a.FacultyId).FullName,a.AssignedOn,a.EndedOn,a.IsRetakeMarker)).ToArray(),await RoutineRows(offeringId,ct),studentsRows,
            row.CanAssign?(await faculty.ListAsync(f=>f.Status=="Active",ct)).Select(f=>new AcademicOption(f.FacultyId,$"{f.EmployeeRef} — {f.FullName}")).ToArray():[],
            row.CanSchedule?(await rooms.ListAsync(ct:ct)).Select(r=>new AcademicOption(r.RoomId,$"{r.Name} ({r.Capacity})")).ToArray():[],history);
    }
    public async Task<AttendanceSession> SessionAsync(int offeringId,int routineId,DateTime date,CancellationToken ct=default)
    {
        var detail=await DetailAsync(offeringId,true,ct);
        if(!detail.Routine.Any(r=>r.Id==routineId))throw new NotFoundException();
        if(date==default || date.Date>clock.Today || detail.Routine.First(r=>r.Id==routineId).Day!=AcademicOperationRules.Weekday(date))throw new BusinessRuleException("Select a valid class date matching the routine weekday.");
        var ids=detail.Students.Where(s=>s.Status=="Confirmed").Select(s=>s.Id).ToArray();
        date=date.Date;var rows=await attendance.ListAsync(a=>ids.Contains(a.EnrollmentId)&&a.RoutineId==routineId&&a.ClassDate==date,ct);
        return new(detail,routineId,date,rows.Where(r=>r.Status=="Present").Select(r=>r.EnrollmentId).ToArray(),rows.Count>0);
    }
    public async Task AuthorizeExportAsync(int batchId,int? offeringId=null,CancellationToken ct=default,string format="CSV")
    {
        var b=await BatchAsync(batchId,ct);if(!await CanRead(b.DepartmentId,ct))throw new ForbiddenException();
        await access.RequireAsync("exports.sensitive",b.DepartmentId,ct);
        if(offeringId.HasValue && (await OfferingAsync(offeringId.Value,ct)).BatchId!=batchId)throw new ForbiddenException();
        await Log("Export academic report",offeringId.HasValue?"Offering":"Batch",(offeringId??batchId).ToString(),format,Actor,ct);await work.SaveChangesAsync(ct);
    }
    public async Task RecordAttendanceAsync(int offeringId,int routineId,DateTime date,int[] presentIds,int[] enrollmentIds,CancellationToken ct=default)
    {
        await teacherGuard.RequireAssignedAsync(offeringId,ct);var o=await OfferingAsync(offeringId,ct);var b=await BatchAsync(o.BatchId,ct);var t=await TermAsync(o.TermId,ct);date=date.Date;
        if(o.Status!="Active" || date<t.StartDate.Date || date>t.EndDate.Date || date>clock.Today)throw new BusinessRuleException("Attendance requires an active offering and a valid past or current class date.");
        var lockText=await config.ResolveAsync(ConfigKeys.AttendanceLockDate,b.DepartmentId,o.TermId,ct);
        if(!string.IsNullOrWhiteSpace(lockText)&&!DateTime.TryParseExact(lockText,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _))throw new BusinessRuleException("Invalid attendance lock configuration.");
        var lockDate=string.IsNullOrWhiteSpace(lockText)?t.EndDate.Date.AddDays(1):DateTime.ParseExact(lockText,"yyyy-MM-dd",CultureInfo.InvariantCulture);
        if(clock.Today>=lockDate)throw new BusinessRuleException("Attendance is locked.");
        var r=await routines.FirstAsync(r=>r.RoutineId==routineId&&r.OfferingId==offeringId,ct)??throw new NotFoundException();
        if(r.DayOfWeek!=AcademicOperationRules.Weekday(date))throw new BusinessRuleException("Class date does not match the routine weekday (Monday=1).");
        var events=await calendar.ListAsync(e=>e.TermId==o.TermId,ct);
        if(events.Any(e=>e.EventType=="Break"&&e.StartDate<=date&&e.EndDate>=date)|| (events.Any(e=>e.EventType=="Class period")&&!events.Any(e=>e.EventType=="Class period"&&e.StartDate<=date&&e.EndDate>=date)))throw new BusinessRuleException("Date is outside the class period or is a break.");
        await work.ExecuteTransactionAsync(async token=>{
            await teacherGuard.RequireAssignedAsync(offeringId,token);
            var es=await enrollments.ListAsync(e=>e.OfferingId==offeringId&&e.EnrollmentType=="Regular"&&e.Status=="Confirmed",token);var ids=es.Select(e=>e.EnrollmentId).ToHashSet();
            if(ids.Count==0||enrollmentIds.Length!=ids.Count||!ids.SetEquals(enrollmentIds)||presentIds.Any(i=>!ids.Contains(i)))throw new BusinessRuleException("The submitted class list is incomplete or has changed. Reload it.");
            var old=await attendance.ListAsync(a=>a.RoutineId==routineId&&a.ClassDate==date&&ids.Contains(a.EnrollmentId),token);
            foreach(var id in ids){var status=presentIds.Contains(id)?"Present":"Absent";var a=old.FirstOrDefault(x=>x.EnrollmentId==id);if(a is null)await attendance.AddAsync(new AttendanceRecord{EnrollmentId=id,RoutineId=routineId,ClassDate=date,Status=status,RecordedBy=Actor},token);else if(a.Status!=status){a.Status=status;a.RecordedBy=Actor;}}
        },ct);
    }
    public async Task<IReadOnlyList<SectionStudent>> SectionReportAsync(int batchId,CancellationToken ct=default)
    {
        var b=await BatchAsync(batchId,ct);if(!await CanRead(b.DepartmentId,ct))throw new ForbiddenException();var ss=await students.ListAsync(s=>s.CurrentBatchId==batchId,ct);var sec=await sections.ListAsync(s=>s.BatchId==batchId,ct);var sids=ss.Select(s=>s.StudentId).ToArray();
        var es=await enrollments.ListAsync(e=>sids.Contains(e.StudentId),ct);var eids=es.Select(e=>e.EnrollmentId).ToArray();var rs=await results.ListAsync(r=>eids.Contains(r.EnrollmentId)&&r.PublishedAt!=null&&!r.IsReplaced,ct);var oids=es.Select(e=>e.OfferingId).ToArray();var os=await offerings.ListAsync(o=>oids.Contains(o.OfferingId),ct);var cids=os.Select(o=>o.CourseId).ToArray();var cs=await courses.ListAsync(c=>cids.Contains(c.CourseId),ct);
        return ss.OrderBy(s=>s.SectionId).ThenBy(s=>s.RollNo).Select(s=>new SectionStudent(s.StudentId,s.FullName,sec.First(x=>x.SectionId==s.SectionId).Name,s.Status,
            es.Where(e=>e.StudentId==s.StudentId&&rs.Where(r=>r.EnrollmentId==e.EnrollmentId).OrderByDescending(r=>r.VersionNo).FirstOrDefault() is {GradePoint:>0}).Select(e=>os.First(o=>o.OfferingId==e.OfferingId).CourseId).Distinct().Sum(id=>cs.First(c=>c.CourseId==id).Credits))).ToArray();
    }
    public async Task<IReadOnlyList<StudentCourseRow>> OwnCoursesAsync(CancellationToken ct=default)
    {
        var actor=Actor;if(!(await access.ForUserAsync(actor,ct)).Assignments.Any(a=>a.RoleName==RoleNames.Student))throw new ForbiddenException();
        var s=await students.FirstAsync(s=>s.UserId==actor,ct)??throw new ForbiddenException();var es=await enrollments.ListAsync(e=>e.StudentId==s.StudentId,ct);var rows=new List<StudentCourseRow>();
        foreach(var e in es){var o=await OfferingAsync(e.OfferingId,ct);var b=await BatchAsync(o.BatchId,ct);var c=await courses.FirstAsync(c=>c.CourseId==o.CourseId,ct)??throw new NotFoundException();var t=await TermAsync(o.TermId,ct);var ar=(await EnrollmentRows(o,b,s.StudentId,ct)).First(x=>x.Id==e.EnrollmentId);rows.Add(new($"{c.Code} — {c.Title}",$"{t.Year} / {t.TermNo}",e.Status,e.EnrollmentType=="Regular"?await RoutineRows(o.OfferingId,ct):[],ar.Percent,ar.Shortage));}
        return rows;
    }
}
