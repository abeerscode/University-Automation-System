using Microsoft.EntityFrameworkCore;
using UA.Entities;
using UA.Framework.Constants;
using UA.Framework.Time;
namespace UA.DataAccess.Data.Seed;
/// <summary>Explicitly enabled test fixtures. Never resets a password or changes existing business rows.</summary>
public static class Phase5DemoSeeder
{
    public static async Task SeedAsync(UniversityDbContext db,IClock clock,Func<string,string> hash,CancellationToken ct=default)
    {
        await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
        var dept=await db.Departments.SingleAsync(d=>d.IdCode=="01",ct);
        var roleMap=await db.Roles.ToDictionaryAsync(r=>r.Name,ct);
        async Task<UserAccount> Account(string name,string password,string role,int? scope=null)
        {
            var a=await db.UserAccounts.SingleOrDefaultAsync(u=>u.Username==name,ct);
            int roleId=roleMap[role].RoleId;
            if(a is not null)
            {
                if(!await db.UserRoles.AnyAsync(r=>r.UserId==a.UserId && r.RoleId==roleId && r.DepartmentId==scope,ct)) throw new InvalidOperationException("Demo account name collision: "+name+". Existing account was not changed.");
                return a;
            }
            a=new UserAccount{Username=name,PasswordHash=hash(password),Status="Active",IsVerified=true};
            db.UserAccounts.Add(a);db.UserRoles.Add(new UserRole{User=a,RoleId=roleId,DepartmentId=scope});await db.SaveChangesAsync(ct);return a;
        }
        await Account("demo.controller","UasExam!2026Demo",RoleNames.Controller); // Phase 7, opt-in demo only
        var registrar=await Account("demo.registrar","UasReg!2026Demo",RoleNames.Registrar);
        var head=await Account("demo.head.cse","UasHead!2026Demo",RoleNames.Head,dept.DepartmentId);
        await Account("demo.coord.cse","UasCoord!2026Demo",RoleNames.Coordinator,dept.DepartmentId);
        var accountant=await Account("demo.accounts","UasFin!2026Demo",RoleNames.Accountant);
        var editor=await Account("demo.notice","UasNotice!2026Demo",RoleNames.WebsiteEditor);
        async Task<Faculty> Teacher(string name,string password,string fullName,string role,string type)
        {
            var account=await Account(name,password,role,dept.DepartmentId);
            var f=await db.Faculties.SingleOrDefaultAsync(x=>x.EmployeeRef==name,ct);
            if(f is not null) { if(f.UserId!=account.UserId) throw new InvalidOperationException("Demo faculty collision."); return f; }
            if(await db.Faculties.AnyAsync(x=>x.UserId==account.UserId,ct)) throw new InvalidOperationException("Demo faculty account already linked.");
            f=new Faculty{UserId=account.UserId,EmployeeRef=name,FullName=fullName,DepartmentId=dept.DepartmentId,Designation="Lecturer",FacultyType=type,Status="Active",Qualifications="MSc in Computer Science"};db.Faculties.Add(f);await db.SaveChangesAsync(ct);return f;
        }
        var teacher=await Teacher("DEMO-FAC-01","UasFac!2026Demo","Demo Lecturer Amina Rahman",RoleNames.Faculty,"Full-time");
        await Teacher("DEMO-GUEST-01","UasGuest!2026Demo","Demo Guest Lecturer Hasan Karim",RoleNames.GuestFaculty,"Guest");
        if(!await db.Rooms.AnyAsync(r=>r.Name=="DEMO-CSE-101",ct)) db.Rooms.Add(new Room{Name="DEMO-CSE-101",Capacity=40,RoomType="Classroom"});
        if(!await db.Notices.AnyAsync(n=>n.Title=="[DEMO] Academic services orientation",ct)) db.Notices.Add(new Notice{DepartmentId=dept.DepartmentId,Title="[DEMO] Academic services orientation",Body="Sample internal notice for testing the university automation system.",AuthorId=editor.UserId,ApprovedBy=head.UserId,IsPublic=false,PublishedAt=clock.Now});
        // Fixed fixture term/year makes repeated startup independent of current date.
        const short year=2026; var start=new DateTime(year,7,1);
        var term=await db.AcademicTerms.SingleOrDefaultAsync(t=>t.Year==year && t.TermNo==2,ct);
        if(term is null){term=new AcademicTerm{Year=year,TermNo=2,StartDate=start,EndDate=start.AddMonths(6).AddDays(-1),RegistrationDeadline=start.AddDays(14),DropDeadline=start.AddDays(9),RetakeDeadline=start.AddMonths(1).AddDays(14)};db.AcademicTerms.Add(term);await db.SaveChangesAsync(ct);}
        if(!await db.AcademicCalendarEvents.AnyAsync(e=>e.TermId==term.TermId && e.Title=="[DEMO] Fee deadline",ct)) db.AcademicCalendarEvents.Add(new AcademicCalendarEvent{TermId=term.TermId,EventType="Fee deadline",Title="[DEMO] Fee deadline",StartDate=term.RegistrationDeadline,EndDate=term.RegistrationDeadline});
        // Only placeholder rates defined by BR-22; document fees need approved amounts.
        foreach(var d in await db.Departments.ToListAsync(ct))
            foreach(var fee in new (string Name,string Unit,decimal Amount)[]{("Registration","Semester",3000),("Tuition","Credit",1000),("Retake exam","Course",1000),("Late fee","Semester",200),("Application","Application",500)})
                if(!await db.FeeTypes.AnyAsync(f=>f.DepartmentId==d.DepartmentId && f.Name==fee.Name,ct)) db.FeeTypes.Add(new FeeType{DepartmentId=d.DepartmentId,Name=fee.Name,Unit=fee.Unit,Amount=fee.Amount,EffectiveFrom=new DateTime(year,1,1)});
        await db.SaveChangesAsync(ct);
        var program=await db.AcademicPrograms.SingleOrDefaultAsync(p=>p.DepartmentId==dept.DepartmentId,ct);
        if(program is null){program=new AcademicProgram{DepartmentId=dept.DepartmentId,Name="BSc in Computer Science and Engineering",TotalCredits=120,SemesterCount=8};db.AcademicPrograms.Add(program);await db.SaveChangesAsync(ct);}
        var batch=await db.Batches.SingleOrDefaultAsync(b=>b.DepartmentId==dept.DepartmentId && b.AdmissionYear==year,ct);
        if(batch is null)
        {
            var version=await db.CurriculumVersions.Where(v=>v.ProgramId==program.ProgramId && v.EffectiveFromYear<=year && (v.Status=="Active" || v.Status=="Retired")).OrderByDescending(v=>v.EffectiveFromYear).ThenByDescending(v=>v.VersionNo).FirstOrDefaultAsync(ct);
            if(version is null)
            {
                if(await db.CurriculumVersions.AnyAsync(v=>v.ProgramId==program.ProgramId,ct) || program.TotalCredits!=120 || program.SemesterCount!=8) throw new InvalidOperationException("Demo student requires an approved 2026 CSE curriculum; existing academic data was not altered.");
                string reference="CSE-CUR-2026-9999";
                if(await db.ApprovalRecords.AnyAsync(a=>a.ReferenceNo==reference,ct)) throw new InvalidOperationException("Demo approval reference collision.");
                var approval=new ApprovalRecord{ReferenceNo=reference,ApprovalType="Curriculum",Status="Approved",ApproverId=registrar.UserId,ApprovalDate=new DateTime(year-1,12,1)};db.ApprovalRecords.Add(approval);
                version=new CurriculumVersion{ProgramId=program.ProgramId,VersionNo=1,EffectiveFromYear=year,Status="Active",Approval=approval};db.CurriculumVersions.Add(version);
                string[] subjects=["Programming","Mathematics","Computer Systems","Communication","Engineering Fundamentals","Data Structures","Discrete Mathematics","Digital Logic","Database Systems","Statistics","Algorithms","Computer Architecture","Operating Systems","Object-Oriented Design","Technical Writing","Computer Networks","Software Engineering","Numerical Methods","Web Development","Theory of Computation","Artificial Intelligence","Cybersecurity","Human Computer Interaction","Distributed Systems","Research Methods","Machine Learning","Cloud Computing","Information Systems","Computer Graphics","Professional Ethics","Data Mining","Mobile Computing","Embedded Systems","Parallel Computing","Elective Studies","Advanced Software Design","Computer Vision","Project Management","Capstone Project","Entrepreneurship"];
                for(int i=0;i<subjects.Length;i++){var code=$"DEMO-CSE-{i+1:D2}";if(await db.Courses.AnyAsync(c=>c.Code==code,ct)) throw new InvalidOperationException("Demo course collision: "+code);var course=new Course{DepartmentId=dept.DepartmentId,Code=code,Title=subjects[i],Credits=3,CourseType="Theory"};db.Courses.Add(course);db.CurriculumCourses.Add(new CurriculumCourse{Curriculum=version,Course=course,SemesterNo=(byte)(i/5+1)});}
                await db.SaveChangesAsync(ct);
            }
            batch=new Batch{DepartmentId=dept.DepartmentId,CurriculumId=version.CurriculumId,AdmissionYear=year,SeatLimit=120};db.Batches.Add(batch);await db.SaveChangesAsync(ct);
            foreach(var name in new[]{"A","B","C"}) db.Sections.Add(new Section{BatchId=batch.BatchId,Name=name,Capacity=40});await db.SaveChangesAsync(ct);
        }
        var section=await db.Sections.SingleOrDefaultAsync(s=>s.BatchId==batch.BatchId && s.Name=="C",ct) ?? throw new InvalidOperationException("Demo student needs section C in the 2026 CSE batch.");
        const string sid="26101120"; // YY + 1 + department 01 + roll 120
        var student=await db.Students.SingleOrDefaultAsync(s=>s.StudentId==sid,ct);
        if(student is null)
        {
            if(batch.SeatLimit<120 || await db.Students.AnyAsync(s=>s.AdmissionBatchId==batch.BatchId && s.RollNo==120,ct) || await db.Students.CountAsync(s=>s.SectionId==section.SectionId,ct)>=section.Capacity) throw new InvalidOperationException("Demo roll or section unavailable; existing students were not changed.");
            if(await db.UserAccounts.AnyAsync(u=>u.Username==sid,ct)) throw new InvalidOperationException("Demo student username collision.");
            var account=await Account(sid,"UasStu!2026Demo",RoleNames.Student);
            student=new Student{StudentId=sid,UserId=account.UserId,DepartmentId=dept.DepartmentId,AdmissionBatchId=batch.BatchId,CurrentBatchId=batch.BatchId,SectionId=section.SectionId,RollNo=120,FullName="Demo Student Samira Akter",DateOfBirth=new DateTime(2007,3,10),Gender="Female",GuardianName="Demo Guardian",GuardianPhone="01000000001",Phone="01000000002",Address="Demo address, Dhaka",PhotoPath="",Status="Active"};db.Students.Add(student);
            db.StudentStatusHistories.Add(new StudentStatusHistory{Student=student,Status="Active",EffectiveDate=new DateTime(year,1,1),Reason="Demo fixture enrollment",ChangedBy=registrar.UserId});db.StudentBatchHistories.Add(new StudentBatchHistory{Student=student,BatchId=batch.BatchId,SectionId=section.SectionId,FromDate=new DateTime(year,1,1),Reason="Demo fixture enrollment"});await db.SaveChangesAsync(ct);
            // Seed an outstanding semester tuition due and a paid registration due, only for the newly created fixture student.
            var feeRates=await db.FeeTypes.Where(f=>f.DepartmentId==dept.DepartmentId && f.EffectiveFrom<=term.StartDate).ToListAsync(ct);
            var credits=await (from link in db.CurriculumCourses join c in db.Courses on link.CourseId equals c.CourseId where link.CurriculumId==batch.CurriculumId && link.SemesterNo==2 select c.Credits).SumAsync(ct);
            if(credits<=0) throw new InvalidOperationException("Demo curriculum lacks semester 2 credits.");
            foreach(var name in new[]{"Registration","Tuition"})
            {
                var rate=feeRates.Where(r=>r.Name==name).OrderByDescending(r=>r.EffectiveFrom).ThenByDescending(r=>r.FeeTypeId).FirstOrDefault()??throw new InvalidOperationException("No demo effective rate: "+name);
                var amount=decimal.Round(rate.Amount*(name=="Tuition"?credits:1),2,MidpointRounding.AwayFromZero);
                var paid=name=="Registration" && amount>0;
                var due=new Due{StudentId=sid,FeeTypeId=rate.FeeTypeId,TermId=term.TermId,Amount=amount,DueDate=term.RegistrationDeadline,Status=paid||amount==0?"Paid":"Unpaid"};db.Dues.Add(due);
                if(paid){var payment=new Payment{StudentId=sid,Amount=amount,Method="Counter",Status="Successful",PaidAt=term.StartDate,RecordedBy=accountant.UserId};db.Payments.Add(payment);await db.SaveChangesAsync(ct);db.PaymentAllocations.Add(new PaymentAllocation{PaymentId=payment.PaymentId,DueId=due.DueId,Amount=amount});db.Receipts.Add(new Receipt{PaymentId=payment.PaymentId,ReceiptNo=$"UAS-{term.StartDate:yyyy}-{payment.PaymentId:D10}",IssuedAt=term.StartDate});}
            }
            if(!await db.MentorAssignments.AnyAsync(m=>m.BatchId==batch.BatchId && (m.SectionId==null || m.SectionId==section.SectionId) && m.EndedOn==null,ct)) db.MentorAssignments.Add(new MentorAssignment{FacultyId=teacher.FacultyId,BatchId=batch.BatchId,SectionId=section.SectionId,AssignedOn=term.StartDate});
        }
        else if(student.FullName!="Demo Student Samira Akter" || !await db.UserAccounts.AnyAsync(u=>u.UserId==student.UserId && u.Username==sid,ct)) throw new InvalidOperationException("Demo student ID collision; existing record was not changed.");
        var template=await db.NotificationTemplates.SingleOrDefaultAsync(t=>t.Code=="demo.phase5.welcome",ct);
        if(template is null){template=new NotificationTemplate{Code="demo.phase5.welcome",Channel="In-app",Subject="Demo finance account",Body="Your sample ledger is ready for testing.",IsActive=true};db.NotificationTemplates.Add(template);await db.SaveChangesAsync(ct);}
        if(!await db.Notifications.AnyAsync(n=>n.RecipientUserId==student.UserId && n.EventKey=="demo.phase5.welcome",ct)) db.Notifications.Add(new Notification{TemplateId=template.TemplateId,RecipientUserId=student.UserId,EventKey="demo.phase5.welcome",Status="Sent",QueuedAt=term.StartDate,SentAt=term.StartDate});
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
}
