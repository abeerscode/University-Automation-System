using System.Globalization;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Examination;
using UA.Facade.Interface.Examination;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Finance;
using UA.Facade.Rules.Academic;
using UA.Facade.Rules.Examination;
using UA.Facade.Rules.Faculty;
using UA.Framework.Constants;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Examination;
public sealed class ExaminationFacade(
    IRepository<OfferingTeacher> assignments,IRepository<UA.Entities.Faculty> faculty,IRepository<ExamEvent> events,IRepository<ExamSitting> sittings,IRepository<AcademicCalendarEvent> calendar,
    IRepository<AcademicTerm> terms,IRepository<Offering> offerings,IRepository<Batch> batches,IRepository<Course> courses,
    IRepository<Enrollment> enrollments,IRepository<UA.Entities.Student> students,IRepository<Room> rooms,
    IRepository<EligibilityHold> holds,IRepository<SeatAllocation> seats,IRepository<AttendanceRecord> attendance,
    IRepository<AttendanceScale> scales,IRepository<MarkComponent> components,IRepository<MarkSubmission> submissions,
    IRepository<MarkEntry> entries,IRepository<GradeScale> grades,IRepository<Result> results,IRepository<SemesterResult> semesterResults,
    IRepository<BackgroundJob> jobs,IRepository<ApprovalRecord> approvals,IRepository<AuditEntry> audits,IRepository<Department> departments,IRepository<NotificationTemplate> templates,IRepository<Notification> notifications,
    IUnitOfWork work,IUserAccessFacade access,ICurrentUser current,IClock clock,IConfigurationFacade config,
    IFeeClearanceChecker fees,ITeacherAssignmentGuard teacher,IAttendanceMarkConverter converter) : IExaminationFacade,IExamJobProcessor
{
    private int Actor=>current.UserId??throw new ForbiddenException();
    private async Task<bool> Role(int user,string role,int? dept,CancellationToken ct)=> (await access.ForUserAsync(user,ct)).Assignments.Any(a=>a.RoleName==role&&(!a.Scoped||(dept.HasValue&&a.DepartmentId==dept)));
    private async Task<bool> Controller(int user,string permission,CancellationToken ct)=>await Role(user,RoleNames.Controller,null,ct)&&await access.AllowedAsync(user,permission,null,ct);
    private async Task RequireController(int user,string permission,CancellationToken ct){if(!await Controller(user,permission,ct))throw new ForbiddenException();}
    private Task<bool> Allowed(string permission,int dept,CancellationToken ct)=>access.AllowedAsync(Actor,permission,dept,ct);
    private async Task<bool> ReadDept(int dept,CancellationToken ct)=>await Controller(Actor,"exams.manage",ct)||await Allowed("reports.view",dept,ct)||await Allowed("hold.release.dues",dept,ct)||await Allowed("hold.release.attendance",dept,ct);
    private async Task<bool> ReadResults(int dept,CancellationToken ct)=>await Controller(Actor,"results.publish",ct)||await Allowed("reports.view",dept,ct);
    private async Task<bool> CanRelease(EligibilityHold h,int dept,CancellationToken ct)=>h.HoldType=="Dues"?((await Role(Actor,RoleNames.Accountant,null,ct)||await Role(Actor,RoleNames.Controller,null,ct))&&await Allowed("hold.release.dues",dept,ct)):((await Role(Actor,RoleNames.Head,dept,ct)||await Role(Actor,RoleNames.Coordinator,dept,ct))&&await Allowed("hold.release.attendance",dept,ct));
    private async Task<ExamEvent> Event(int id,CancellationToken ct)=>await events.FirstAsync(x=>x.EventId==id,ct)??throw new NotFoundException();
    private async Task<Offering> Offering(int id,CancellationToken ct)=>await offerings.FirstAsync(x=>x.OfferingId==id,ct)??throw new NotFoundException();
    private async Task<Batch> Batch(int id,CancellationToken ct)=>await batches.FirstAsync(x=>x.BatchId==id,ct)??throw new NotFoundException();
    private async Task<Course> Course(int id,CancellationToken ct)=>await courses.FirstAsync(x=>x.CourseId==id,ct)??throw new NotFoundException();
    private async Task<AcademicTerm> Term(int id,CancellationToken ct)=>await terms.FirstAsync(x=>x.TermId==id,ct)??throw new NotFoundException();
    private async Task<UA.Entities.Student> Student(string id,CancellationToken ct)=>await students.FirstAsync(x=>x.StudentId==id,ct)??throw new NotFoundException();
    private Task Log(int actor,string action,string entity,int id,string detail,CancellationToken ct)=>audits.AddAsync(new AuditEntry{UserId=actor,Action=action,EntityName=entity,EntityId=id.ToString(),NewValue=detail,LoggedAt=clock.Now},ct);
    private async Task Notify(IEnumerable<string> studentIds,string templateCode,string key,CancellationToken ct)
    {var t=await templates.FirstAsync(x=>x.Code==templateCode&&x.IsActive,ct);if(t is null)return;var ids=studentIds.Distinct().ToArray();var users=await students.ListAsync(x=>ids.Contains(x.StudentId),ct);var old=await notifications.ListAsync(x=>x.EventKey==key,ct);var sent=old.Select(x=>x.RecipientUserId).ToHashSet();foreach(var st in users)if(sent.Add(st.UserId))await notifications.AddAsync(new Notification{TemplateId=t.TemplateId,RecipientUserId=st.UserId,EventKey=key,Status="Queued",QueuedAt=clock.Now},ct);}
    private static EventRow Row(ExamEvent e)=>new(e.EventId,e.TermId,e.EventType,e.StartDate,e.EndDate,e.Status);
    private async Task<bool> CanEnter(int id,int dept,CancellationToken ct)
    {if(!await access.AllowedAsync(Actor,"marks.enter",null,ct))return false;try{await teacher.RequireAssignedAsync(id,ct);return true;}catch(ForbiddenException){return false;}}
    private async Task<bool> CanEnterRetake(int id,CancellationToken ct){var f=await faculty.FirstAsync(x=>x.UserId==Actor&&x.Status=="Active",ct);if(f is null)return false;return await assignments.CountAsync(x=>x.OfferingId==id&&x.FacultyId==f.FacultyId&&x.IsRetakeMarker&&x.AssignedOn<=clock.Today&&(x.EndedOn==null||x.EndedOn>clock.Today),ct)>0;}
    private async Task<bool> CanVerify(int dept,CancellationToken ct)=>await Role(Actor,RoleNames.Head,dept,ct)&&await Allowed("marks.verify",dept,ct);
    public async Task<ExamHome> HomeAsync(CancellationToken ct=default)
    {
        var actor=Actor;var admin=await Controller(actor,"exams.manage",ct);var bs=await batches.ListAsync(ct:ct);var os=await offerings.ListAsync(ct:ct);var cs=await courses.ListAsync(ct:ct);var subs=await submissions.ListAsync(ct:ct);var list=new List<OfferingExamRow>();
        foreach(var o in os){var b=bs.Single(x=>x.BatchId==o.BatchId);var enter=await CanEnter(o.OfferingId,b.DepartmentId,ct);var verify=await CanVerify(b.DepartmentId,ct);if(enter||verify||admin)list.Add(new(o.OfferingId,$"{cs.Single(x=>x.CourseId==o.CourseId).Code} / Batch {b.AdmissionYear} / Section {o.SectionId} / Term {o.TermId}{(o.IsSpecialRetake?" / Special retake":"")}",subs.Where(x=>x.OfferingId==o.OfferingId).OrderByDescending(x=>x.SubmissionId).FirstOrDefault()?.Status??"Not started",enter,verify,admin));}
        bool read=admin;foreach(var dept in bs.Select(b=>b.DepartmentId).Distinct())if(await ReadDept(dept,ct)){read=true;break;}
        if(!read&&list.Count==0&&!await access.AllowedAsync(actor,"marks.enter",null,ct))throw new ForbiddenException();
        var ts=await terms.ListAsync(ct:ct);return new(ts.OrderByDescending(t=>t.Year).ThenByDescending(t=>t.TermNo).Select(t=>new ExamOption(t.TermId,$"{t.Year} / {t.TermNo}")).ToArray(),read?(await events.ListAsync(ct:ct)).Select(Row).ToArray():[],list.ToArray(),admin);
    }
    public async Task<int> CreateEventAsync(int termId,string type,CancellationToken ct=default)
    {
        await RequireController(Actor,"exams.manage",ct);if(type is not ("Midterm" or "Final"))throw new BusinessRuleException("Invalid exam type.");var term=await Term(termId,ct);var period=await calendar.FirstAsync(x=>x.TermId==termId&&x.EventType==(type+" period"),ct)??throw new BusinessRuleException("Set the examination period in Academic Calendar first.");
        if(period.StartDate<term.StartDate||period.EndDate>term.EndDate||period.EndDate<period.StartDate)throw new BusinessRuleException("Invalid calendar period.");var e=new ExamEvent{TermId=termId,EventType=type,StartDate=period.StartDate,EndDate=period.EndDate,Status="Draft"};
        await work.ExecuteTransactionAsync(async token=>{if(await events.CountAsync(x=>x.TermId==termId&&x.EventType==type,token)>0)throw new BusinessRuleException("This examination event already exists.");await events.AddAsync(e,token);},ct);return e.EventId;
    }
    public async Task<int> CreateSpecialEventAsync(int termId,DateTime date,CancellationToken ct=default)
    {await RequireController(Actor,"exams.manage",ct);var t=await Term(termId,ct);var day=date.Date;if(day<=clock.Today||day<t.StartDate.Date||day>t.EndDate.Date)throw new BusinessRuleException("Choose a future special retake date within the selected term.");var item=new ExamEvent{TermId=termId,EventType="Special",StartDate=day,EndDate=day,Status="Draft"};await work.ExecuteTransactionAsync(async token=>{var final=await events.FirstAsync(x=>x.TermId==termId&&x.EventType=="Final",token);if(final is not null&&final.StartDate<=day&&final.EndDate>=day)throw new BusinessRuleException("Choose a date outside the main final period.");if(await events.CountAsync(x=>x.TermId==termId&&x.EventType=="Special",token)>0)throw new BusinessRuleException("A special event already exists for this term.");await events.AddAsync(item,token);await work.SaveChangesAsync(token);await Log(Actor,"Create special retake event","ExamEvent",item.EventId,day.ToString("yyyy-MM-dd"),token);},ct);return item.EventId;}
    private static void Times(TimeSpan start,TimeSpan end){if(start<TimeSpan.Zero||end>=TimeSpan.FromDays(1)||end<=start)throw new BusinessRuleException("Enter valid exam times.");}
    private static ExamSlot Slot(ExamSitting sitting) => new(sitting.ExamDate, sitting.StartTime, sitting.EndTime);
    private async Task<ExamHall[]> ExamHalls(CancellationToken ct) =>
        (await rooms.ListAsync(x => x.RoomType == "Exam hall" && x.Capacity > 0, ct))
        .Select(x => new ExamHall(x.RoomId, x.Capacity)).ToArray();
    private async Task<List<ExamDemand>> ScheduleDemands(ExamEvent currentEvent, IReadOnlyList<ExamSitting> scheduled,
        IReadOnlyList<SeatAllocation> allocations, CancellationToken ct)
    {
        var eventIds = scheduled.Select(x => x.EventId).Append(currentEvent.EventId).Distinct().ToArray();
        var eventRows = await events.ListAsync(x => eventIds.Contains(x.EventId), ct);
        var eventTerms = eventRows.ToDictionary(x => x.EventId, x => x.TermId);
        eventTerms[currentEvent.EventId] = currentEvent.TermId;
        var termIds = eventTerms.Values.Distinct().ToArray();
        var os = await offerings.ListAsync(x => termIds.Contains(x.TermId), ct);
        var offeringIds = os.Select(x => x.OfferingId).ToArray();
        var es = await enrollments.ListAsync(x => offeringIds.Contains(x.OfferingId) && (x.EnrollmentType == "Regular" || x.EnrollmentType == "Retake") &&
            (x.Status == "Registered" || x.Status == "Confirmed" || x.Status == "Completed"), ct);
        var active = await students.ListAsync(x => x.Status == "Active", ct);
        var activeIds = active.Select(x => x.StudentId).ToHashSet();
        var enrolled = es.ToLookup(x => x.OfferingId);
        var cohort = active.ToLookup(x => (x.CurrentBatchId, x.SectionId));
        var byCourse = os.ToLookup(x => (x.TermId, x.CourseId, x.IsSpecialRetake));
        var bySitting = allocations.ToLookup(x => x.SittingId);
        var demands = new List<ExamDemand>();
        foreach (var sitting in scheduled)
        {
            // Phase 8 owns make-up/retake scheduling. Preserve existing regular allocations, and
            // reserve any already allocated non-regular halls without inferring retake clashes.
            var fixedRows = bySitting[sitting.SittingId].ToArray();
            if (fixedRows.Any(x => x.MakeupDay.HasValue))
                throw new BusinessRuleException("Existing make-up allocations must be preserved; use manual seat adjustments.");
            var regular = sitting.SittingType == "Regular";
            var special=eventRows.Any(x=>x.EventId==sitting.EventId&&x.EventType=="Special");
            var courseOfferings = regular ? byCourse[(eventTerms[sitting.EventId], sitting.CourseId, special)].ToArray() : Array.Empty<Offering>();
            var studentIds = courseOfferings.SelectMany(o => enrolled[o.OfferingId].Where(x => x.EnrollmentType=="Regular" &&
                (activeIds.Contains(x.StudentId) || x.Status == "Completed")).Select(x => x.StudentId)
                .Concat(!special && o.Status != "Closed" ? cohort[(o.BatchId, o.SectionId)].Select(x => x.StudentId) : Enumerable.Empty<string>()))
                .Distinct().ToArray();
            demands.Add(new ExamDemand(sitting.SittingId, Slot(sitting), regular,
                special?Array.Empty<int>():courseOfferings.Select(x => x.BatchId).Distinct().ToArray(), special?Array.Empty<int>():courseOfferings.Select(x => x.SectionId).Distinct().ToArray(),
                studentIds, fixedRows.Select(x => new ReservedExamSeat(x.StudentId, x.RoomId, x.SeatNo)).ToArray(),
                eventRows.Any(x=>x.EventId==sitting.EventId&&(x.EventType=="Final"||x.EventType=="Special")) ? courseOfferings.SelectMany(o=>enrolled[o.OfferingId].Where(x=>x.EnrollmentType=="Retake"&&(activeIds.Contains(x.StudentId)||x.Status=="Completed")).Select(x=>x.StudentId)).Distinct().ToArray() : [],special));
        }
        return demands;
    }
    private async Task<List<ExamSitting>> PeriodSittings(ExamEvent e, CancellationToken ct) =>
        await sittings.ListAsync(x => x.ExamDate >= e.StartDate && x.ExamDate <= e.EndDate, ct);
    private async Task<List<SeatAllocation>> PeriodSeats(IReadOnlyList<ExamSitting> scheduled, CancellationToken ct)
    {
        var keys = scheduled.Select(x => x.SittingId).Where(x => x > 0).ToArray();
        return await seats.ListAsync(x => keys.Contains(x.SittingId), ct);
    }
    public async Task GenerateRoutineAsync(int id, TimeSpan start, TimeSpan end, int minutes, CancellationToken ct = default)
    {
        await RequireController(Actor, "exams.manage", ct); Times(start, end);
        if (minutes < 30 || minutes > 360) throw new BusinessRuleException("Invalid slot duration.");
        var duration = TimeSpan.FromMinutes(minutes);
        await work.ExecuteTransactionAsync(async token =>
        {
            var e = await Event(id, token);
            if (e.Status != "Draft" || await sittings.CountAsync(s => s.EventId == id, token) > 0)
                throw new BusinessRuleException("Routine already exists.");
            var os = await offerings.ListAsync(x => x.TermId == e.TermId && x.IsSpecialRetake == (e.EventType=="Special") && x.Status != "Closed", token);
            var cs = await courses.ListAsync(ct: token);
            var courseIds = os.Select(x => x.CourseId).Distinct().Where(c => e.EventType != "Midterm" ||
                cs.Single(x => x.CourseId == c).CourseType == "Theory").ToArray();
            if (courseIds.Length == 0) throw new BusinessRuleException("No applicable offerings.");
            var halls = await ExamHalls(token);
            if (halls.Length == 0) throw new BusinessRuleException("Configure exam halls first.");
            var breaks = await calendar.ListAsync(x => x.TermId == e.TermId && x.EventType == "Break", token);
            var reservedMakeups=(await seats.ListAsync(a=>a.MakeupDay>=e.StartDate&&a.MakeupDay<=e.EndDate,token)).Select(a=>a.MakeupDay!.Value.Date).ToHashSet();
            var existing = await PeriodSittings(e, token);
            var fixedSeats = await PeriodSeats(existing, token);
            var slots = new List<ExamSlot>();
            for (var d = e.StartDate.Date; d <= e.EndDate.Date; d = d.AddDays(1))
                if (!reservedMakeups.Contains(d)&&!breaks.Any(b => b.StartDate <= d && b.EndDate >= d))
                    for (var time = start; time + duration <= end; time += duration) slots.Add(new(d, time, time + duration));
            if (slots.Count == 0) throw new BusinessRuleException("No exam slots fit the calendar and daily window.");
            var pending = courseIds.OrderBy(x => x).Select((course, i) => new ExamSitting { SittingId = -i - 1,
                EventId = id, CourseId = course, SittingType = "Regular", ExamDate = slots[0].Date,
                StartTime = slots[0].Start, EndTime = slots[0].End }).ToArray();
            var demands = await ScheduleDemands(e, existing.Concat(pending).ToArray(), fixedSeats, token);
            var placed = demands.Where(x => x.Key > 0).ToList();
            // Larger and multi-batch courses first; earliest feasible slot wins. Check all sittings on that day.
            foreach (var exam in demands.Where(x => x.Key < 0).OrderByDescending(x => x.BatchIds.Length)
                         .ThenByDescending(x => x.StudentIds.Length).ThenByDescending(x => x.Key))
            {
                ExamDemand? chosen = null;
                foreach (var slot in slots)
                {
                    var candidate = exam with { Slot = slot };
                    var day = placed.Where(x => x.Slot.Date == slot.Date).Append(candidate).ToArray();
                    if (ExamScheduleRules.TryPlan(day, halls, out _, out _)) { chosen = candidate; break; }
                }
                if (chosen is null) throw new BusinessRuleException("Insufficient non-conflicting exam slots or hall capacity. Extend the exam window or configure more halls.");
                placed.Add(chosen);
            }
            foreach (var exam in placed.Where(x => x.Key < 0))
                await sittings.AddAsync(new ExamSitting { EventId = id, CourseId = pending.Single(x => x.SittingId == exam.Key).CourseId,
                    SittingType = "Regular", ExamDate = exam.Slot.Date, StartTime = exam.Slot.Start, EndTime = exam.Slot.End }, token);
            e.Status = "Scheduled";
            await Log(Actor, "Generate exam routine", "ExamEvent", id, "Parallel routine generated with cohort and hall feasibility checks", token);
        }, ct);
    }
    public async Task EditSittingAsync(int id, int sittingId, DateTime date, TimeSpan start, TimeSpan end, CancellationToken ct = default)
    {
        await RequireController(Actor, "exams.manage", ct); Times(start, end);
        await work.ExecuteTransactionAsync(async token =>
        {
            var e = await Event(id, token);
            var s = await sittings.FirstAsync(x => x.SittingId == sittingId && x.EventId == id, token) ?? throw new NotFoundException();
            if (e.Status != "Scheduled" || s.SittingType != "Regular" || date.Date < e.StartDate || date.Date > e.EndDate ||
                await seats.CountAsync(x => x.SittingId == sittingId, token) > 0)
                throw new BusinessRuleException("Sitting cannot be changed after seating or outside the exam period.");
            if(await seats.CountAsync(x=>x.MakeupDay==date.Date,token)>0)throw new BusinessRuleException("Date reserved for assigned make-up exams.");
            if (await calendar.CountAsync(x => x.TermId == e.TermId && x.EventType == "Break" && x.StartDate <= date.Date && x.EndDate >= date.Date, token) > 0)
                throw new BusinessRuleException("Exam slot clashes with a calendar break.");
            var sameDay = await sittings.ListAsync(x => x.ExamDate == date.Date && x.SittingId != sittingId, token);
            sameDay.Add(s);
            var demands = await ScheduleDemands(e, sameDay, await PeriodSeats(sameDay, token), token);
            var revised = demands.Select(x => x.Key == sittingId ? x with { Slot = new ExamSlot(date.Date, start, end) } : x).ToArray();
            var halls = await ExamHalls(token);
            if (halls.Length == 0) throw new BusinessRuleException("Configure exam halls first.");
            if (!ExamScheduleRules.TryPlan(revised, halls, out _, out var reason)) throw new BusinessRuleException(reason);
            var old = $"{s.ExamDate:yyyy-MM-dd} {s.StartTime}-{s.EndTime}";
            s.ExamDate = date.Date; s.StartTime = start; s.EndTime = end;
            await Log(Actor, "Adjust exam routine", "ExamSitting", s.SittingId, $"{old} -> {date:yyyy-MM-dd} {start}-{end}", token);
        }, ct);
    }
    private async Task<HoldRow> HoldDto(EligibilityHold h,CancellationToken ct)
    {var s=await Student(h.StudentId,ct);var can=await CanRelease(h,s.DepartmentId,ct);return new(h.HoldId,h.StudentId,h.CourseId.HasValue?(await Course(h.CourseId.Value,ct)).Code:"All courses",h.HoldType,h.Reason,h.ReleasedAt.HasValue,can);}
    private async Task<bool> IsRetake(SeatAllocation a,ExamSitting sitting,CancellationToken ct){var e=await Event(sitting.EventId,ct);if(e.EventType is not ("Final" or "Special"))return false;var ids=(await offerings.ListAsync(x=>x.TermId==e.TermId&&x.IsSpecialRetake==(e.EventType=="Special")&&x.CourseId==sitting.CourseId,ct)).Select(x=>x.OfferingId).ToArray();return await enrollments.CountAsync(x=>x.StudentId==a.StudentId&&ids.Contains(x.OfferingId)&&x.EnrollmentType=="Retake"&&(x.Status=="Confirmed"||x.Status=="Completed"),ct)>0;}
    private async Task<SeatRow> SeatDto(SeatAllocation a,ExamSitting s,CancellationToken ct){var st=await Student(a.StudentId,ct);var retake=await IsRetake(a,s,ct);return new(s.EventId,s.SittingId,a.StudentId,(await Course(s.CourseId,ct)).Code,a.MakeupDay??s.ExamDate,s.StartTime,s.EndTime,(await rooms.FirstAsync(x=>x.RoomId==a.RoomId,ct))?.Name??"Unknown",a.SeatNo,retake,a.ClashConfirmedBy.HasValue,a.MakeupDay.HasValue,retake&&await Role(Actor,RoleNames.Coordinator,st.DepartmentId,ct)&&await Allowed("retake.enroll",st.DepartmentId,ct));}
    public async Task ConfirmClashAsync(int eventId,int sittingId,string studentId,string reason,CancellationToken ct=default){if(string.IsNullOrWhiteSpace(reason)||reason.Length>255)throw new BusinessRuleException("Provide the department-confirmed clash reason.");await work.ExecuteTransactionAsync(async token=>{var e=await Event(eventId,token);var s=await sittings.FirstAsync(x=>x.EventId==eventId&&x.SittingId==sittingId,token)??throw new NotFoundException();var st=await Student(studentId,token);if(!await Role(Actor,RoleNames.Coordinator,st.DepartmentId,token)||!await Allowed("retake.enroll",st.DepartmentId,token))throw new ForbiddenException();var a=await seats.FirstAsync(x=>x.SittingId==sittingId&&x.StudentId==studentId,token)??throw new NotFoundException();if(e.Status!="Scheduled"||!await IsRetake(a,s,token)||a.ClashConfirmedBy.HasValue)throw new BusinessRuleException("A scheduled retake seat without prior confirmation is required.");a.ClashConfirmedBy=Actor;await Log(Actor,"Confirm retake clash","SeatAllocation",a.SeatId,reason,token);},ct);}
    public async Task AssignMakeupAsync(int eventId,int sittingId,string studentId,DateTime date,int roomId,int seatNo,CancellationToken ct=default){await RequireController(Actor,"makeup.manage",ct);await work.ExecuteTransactionAsync(async token=>{var e=await Event(eventId,token);var s=await sittings.FirstAsync(x=>x.EventId==eventId&&x.SittingId==sittingId,token)??throw new NotFoundException();var a=await seats.FirstAsync(x=>x.SittingId==sittingId&&x.StudentId==studentId,token)??throw new NotFoundException();if(e.Status!="Scheduled"||!a.ClashConfirmedBy.HasValue||a.ClashConfirmedBy==Actor||!await IsRetake(a,s,token)||date.Date<=e.EndDate.Date||date.Date<clock.Today)throw new BusinessRuleException("A department-confirmed retake clash and future make-up date after the main exam period are required.");var windows=await calendar.ListAsync(x=>x.TermId==e.TermId&&x.EventType=="Make-up days"&&x.StartDate>e.EndDate,token);var days=windows.SelectMany(x=>Enumerable.Range(0,Math.Max(0,(x.EndDate.Date-x.StartDate.Date).Days+1)).Select(n=>x.StartDate.Date.AddDays(n))).Distinct().ToArray();if(days.Length is <2 or >3||!days.Contains(date.Date))throw new BusinessRuleException("Configure exactly two or three make-up days in the academic calendar.");if(await holds.CountAsync(x=>x.EventId==eventId&&x.StudentId==studentId&&x.ReleasedAt==null&&(x.CourseId==null||x.CourseId==s.CourseId),token)>0)throw new BusinessRuleException("Student has an active examination hold.");var room=await rooms.FirstAsync(x=>x.RoomId==roomId&&x.RoomType=="Exam hall",token)??throw new BusinessRuleException("Select an exam hall.");if(seatNo<1||seatNo>room.Capacity)throw new BusinessRuleException("Seat outside hall capacity.");var number=seatNo.ToString(CultureInfo.InvariantCulture);var dayRows=await sittings.ListAsync(x=>x.ExamDate==date.Date&&x.StartTime<s.EndTime&&s.StartTime<x.EndTime,token);var keys=dayRows.Select(x=>x.SittingId).ToArray();if(keys.Length>0)throw new BusinessRuleException("Make-up dates must not overlap another scheduled main examination.");var makeup=await seats.ListAsync(x=>x.MakeupDay==date.Date&&x.SeatId!=a.SeatId,token);var makeupIds=makeup.Select(x=>x.SittingId).Distinct().ToArray();var makeupSittings=await sittings.ListAsync(x=>makeupIds.Contains(x.SittingId),token);foreach(var existing in makeup){var other=makeupSittings.Single(x=>x.SittingId==existing.SittingId);if(other.StartTime>=s.EndTime||s.StartTime>=other.EndTime)continue;if(existing.RoomId==roomId&&(existing.SittingId!=sittingId||existing.SeatNo==number)||existing.StudentId==studentId)throw new BusinessRuleException("Make-up room, seat or examinee conflicts with another manually assigned make-up slot.");}a.MakeupDay=date.Date;a.RoomId=roomId;a.SeatNo=number;await Log(Actor,"Assign make-up seat","SeatAllocation",a.SeatId,$"{date:yyyy-MM-dd}; room {roomId}; seat {number}",token);},ct);}
    private async Task EnsureMakeupsComplete(int eventId,CancellationToken ct){var ids=(await sittings.ListAsync(x=>x.EventId==eventId,ct)).Select(x=>x.SittingId).ToArray();if(await seats.CountAsync(x=>ids.Contains(x.SittingId)&&x.ClashConfirmedBy!=null&&(x.MakeupDay==null||x.MakeupDay>=clock.Today),ct)>0)throw new BusinessRuleException("All confirmed make-up exams must be assigned and completed before marks submission/publication.");}
    public async Task<EventPage> EventAsync(int id,CancellationToken ct=default)
    {
        var e=await Event(id,ct);var admin=await Controller(Actor,"exams.manage",ct);var bs=await batches.ListAsync(ct:ct);var allowed=new HashSet<int>();foreach(var d in bs.Select(x=>x.DepartmentId).Distinct())if(await ReadDept(d,ct))allowed.Add(d);if(!admin&&allowed.Count==0)throw new ForbiddenException();
        var ss=await sittings.ListAsync(x=>x.EventId==id,ct);var os=await offerings.ListAsync(x=>x.TermId==e.TermId&&x.IsSpecialRetake==(e.EventType=="Special"),ct);var batchIds=bs.Where(b=>allowed.Contains(b.DepartmentId)).Select(b=>b.BatchId).ToHashSet();var visible=ss.Where(s=>admin||os.Any(o=>o.CourseId==s.CourseId&&batchIds.Contains(o.BatchId))).ToArray();var sr=new List<SittingRow>();foreach(var s in visible.OrderBy(x=>x.ExamDate).ThenBy(x=>x.StartTime).ThenBy(x=>x.CourseId))sr.Add(new(s.SittingId,(await Course(s.CourseId,ct)).Code,s.ExamDate,s.StartTime,s.EndTime));
        var allStudents=await students.ListAsync(x=>allowed.Contains(x.DepartmentId),ct);var ids=allStudents.Select(x=>x.StudentId).ToHashSet();var hs=await holds.ListAsync(x=>x.EventId==id,ct);var hr=new List<HoldRow>();foreach(var h in hs.Where(h=>admin||ids.Contains(h.StudentId))){var student=allStudents.FirstOrDefault(x=>x.StudentId==h.StudentId)??await Student(h.StudentId,ct);if(admin||await ReadResults(student.DepartmentId,ct)||await CanRelease(h,student.DepartmentId,ct))hr.Add(await HoldDto(h,ct));}var keys=visible.Select(x=>x.SittingId).ToArray();var alloc=await seats.ListAsync(x=>keys.Contains(x.SittingId),ct);var ar=new List<SeatRow>();foreach(var a in alloc.Where(a=>admin||ids.Contains(a.StudentId))){var student=allStudents.FirstOrDefault(x=>x.StudentId==a.StudentId)??await Student(a.StudentId,ct);if(admin||await ReadResults(student.DepartmentId,ct))ar.Add(await SeatDto(a,ss.Single(s=>s.SittingId==a.SittingId),ct));}var hallOptions=admin?(await rooms.ListAsync(x=>x.RoomType=="Exam hall",ct)).Select(x=>new ExamOption(x.RoomId,$"{x.Name} ({x.Capacity} seats)")).ToArray():Array.Empty<ExamOption>();return new(Row(e),sr.ToArray(),hr.ToArray(),ar.ToArray(),admin,hallOptions);
    }
    public async Task ReleaseAsync(int id,string reason,CancellationToken ct=default)
    {
        if(string.IsNullOrWhiteSpace(reason)||reason.Length>255)throw new BusinessRuleException("A reason of up to 255 characters is required.");await work.ExecuteTransactionAsync(async token=>{var h=await holds.FirstAsync(x=>x.HoldId==id,token)??throw new NotFoundException();var s=await Student(h.StudentId,token);if(!await CanRelease(h,s.DepartmentId,token))throw new ForbiddenException();if(h.ReleasedAt.HasValue)throw new BusinessRuleException("Hold already released.");h.ReleasedBy=Actor;h.ReleasedAt=clock.Now;h.ReleaseReason=reason;await Log(Actor,"Release exam hold","EligibilityHold",id,reason,token);},ct);
    }
    public async Task<int> QueueAsync(int id,string kind,CancellationToken ct=default)
    {
        if(kind is not ("Seat" or "Publish"))throw new BusinessRuleException("Invalid job.");await RequireController(Actor,kind=="Seat"?"seatplan.manage":"results.publish",ct);var e=await Event(id,ct);if(e.Status!="Scheduled"||(kind=="Publish"&&(e.EventType is not ("Final" or "Special")||clock.Today<=e.EndDate)))throw new BusinessRuleException("A scheduled event is required; final publication must follow the final examination.");var type=$"Exam{kind}:{id}";BackgroundJob? job=null;
        await work.ExecuteTransactionAsync(async token=>{job=await jobs.FirstAsync(x=>x.JobType==type&&(x.Status=="Queued"||x.Status=="Running"),token);if(job is null){job=new BackgroundJob{JobType=type,RequestedBy=Actor,Status="Queued",CreatedAt=clock.Now};await jobs.AddAsync(job,token);}},ct);return job!.JobId;
    }
    public async Task<ExamJob> JobAsync(int id,CancellationToken ct=default){await RequireController(Actor,"exams.manage",ct);var j=await jobs.FirstAsync(x=>x.JobId==id&&x.JobType.StartsWith("Exam"),ct)??throw new NotFoundException();return new(j.JobId,j.JobType,j.Status,j.ProgressPercent,j.ErrorReportPath);}
    public async Task<int?> NextAsync(CancellationToken ct){var rows=await jobs.ListAsync(x=>(x.Status=="Queued"||x.Status=="Running")&&(x.JobType.StartsWith("ExamSeat:")||x.JobType.StartsWith("ExamPublish:")),ct);return rows.OrderBy(x=>x.JobId).FirstOrDefault()?.JobId;}
    public async Task FailAsync(int id,string reason,CancellationToken ct){await work.ExecuteTransactionAsync(async token=>{var j=await jobs.FirstAsync(x=>x.JobId==id&&x.JobType.StartsWith("Exam")&&(x.Status=="Queued"||x.Status=="Running"),token);if(j is null)return;j.Status="Failed";j.ErrorReportPath=reason.Length>255?reason[..255]:reason;j.FinishedAt=clock.Now;},ct);}
    public async Task ProcessAsync(int id,CancellationToken ct)
    {
        var j=await jobs.FirstAsync(x=>x.JobId==id,ct)??throw new NotFoundException();var parts=j.JobType.Split(':');if(parts.Length!=2||!int.TryParse(parts[1],out var eventId)||parts[0] is not ("ExamSeat" or "ExamPublish"))throw new BusinessRuleException("Invalid exam job.");var seat=parts[0]=="ExamSeat";await RequireController(j.RequestedBy,seat?"seatplan.manage":"results.publish",ct);
        // Same committed running stage and restart recovery convention as Phase 6.
        await work.ExecuteTransactionAsync(async token=>{var pending=await jobs.FirstAsync(x=>x.JobId==id&&x.Status=="Queued",token);if(pending is not null){pending.Status="Running";pending.ProgressPercent=10;}},ct);
        await work.ExecuteTransactionAsync(async token=>{var fresh=await jobs.FirstAsync(x=>x.JobId==id&&x.Status=="Running",token);if(fresh is null)return;var e=await Event(eventId,token);if(e.Status!="Scheduled")throw new BusinessRuleException("Event is no longer scheduled.");if(seat)await Allocate(e,j.RequestedBy,token);else await Publish(e,j.RequestedBy,token);fresh.Status="Completed";fresh.ProgressPercent=100;fresh.FinishedAt=clock.Now;},ct);
    }
    private async Task<decimal?> Attendance(Enrollment en,DateTime through,CancellationToken ct){var a=await attendance.ListAsync(x=>x.EnrollmentId==en.EnrollmentId&&x.ClassDate<=through,ct);return AcademicOperationRules.Percentage(a.Count(x=>x.Status=="Present"),a.Count);}
    private async Task EnsureHold(ExamEvent e,string student,int? course,string type,string? reason,int actor,List<EligibilityHold> existing,CancellationToken ct)
    {
        var h=existing.Where(x=>x.StudentId==student&&x.CourseId==course&&x.HoldType==type).OrderByDescending(x=>x.HoldId).FirstOrDefault();if(reason is null){if(h is not null&&!h.ReleasedAt.HasValue){h.ReleasedAt=clock.Now;h.ReleasedBy=actor;h.ReleaseReason="Condition cleared on recheck";}return;}
        if(h is not null&&(!h.ReleasedAt.HasValue||h.ReleaseReason!="Condition cleared on recheck"))return;
        h=new EligibilityHold{EventId=e.EventId,StudentId=student,CourseId=course,HoldType=type,Reason=reason};existing.Add(h);await holds.AddAsync(h,ct);
    }
    private async Task Allocate(ExamEvent e,int actor,CancellationToken ct)
    {
        var ss=await sittings.ListAsync(x=>x.EventId==e.EventId,ct);if(ss.Count==0)throw new BusinessRuleException("Generate routine first.");var os=await offerings.ListAsync(x=>x.TermId==e.TermId&&x.IsSpecialRetake==(e.EventType=="Special"),ct);var ids=os.Select(x=>x.OfferingId).ToArray();var es=await enrollments.ListAsync(x=>ids.Contains(x.OfferingId)&&x.Status=="Confirmed"&&(x.EnrollmentType=="Regular"||((e.EventType=="Final"||e.EventType=="Special")&&x.EnrollmentType=="Retake")),ct);var activeIds=(await students.ListAsync(x=>x.Status=="Active",ct)).Select(x=>x.StudentId).ToHashSet();es=es.Where(x=>activeIds.Contains(x.StudentId)).ToList();var bs=await batches.ListAsync(ct:ct);var hs=await holds.ListAsync(x=>x.EventId==e.EventId,ct);var halls=(await rooms.ListAsync(x=>x.RoomType=="Exam hall"&&x.Capacity>0,ct)).OrderBy(x=>x.RoomId).ToArray();if(halls.Length==0)throw new BusinessRuleException("Configure exam halls first.");
        foreach(var group in es.GroupBy(x=>x.StudentId)){var st=await Student(group.Key,ct);var clear=await fees.IsClearAsync(st.StudentId,e.TermId,ct);await EnsureHold(e,st.StudentId,null,"Dues",clear?null:"Outstanding semester dues",actor,hs,ct);foreach(var en in group){var o=os.Single(x=>x.OfferingId==en.OfferingId);if(!ss.Any(s=>s.CourseId==o.CourseId)||en.EnrollmentType=="Retake")continue;var dept=bs.Single(b=>b.BatchId==o.BatchId).DepartmentId;var cfg=await config.ResolveAsync(ConfigKeys.AttendanceMinimum,dept,e.TermId,ct);var limit=decimal.TryParse(cfg,NumberStyles.Number,CultureInfo.InvariantCulture,out var n)?Math.Clamp(n,75,100):75;var pct=await Attendance(en,e.StartDate,ct);await EnsureHold(e,st.StudentId,o.CourseId,"Attendance",pct is null?"Attendance not recorded":pct<limit?$"Attendance {pct:F2}% below {limit}%":null,actor,hs,ct);}}
        var scheduled = await PeriodSittings(e, ct);
        var allSeats = await PeriodSeats(scheduled, ct);
        var byOffering = os.ToDictionary(x => x.OfferingId);
        var eligibility = ss.ToDictionary(s => s.SittingId, s => es.Where(x => byOffering[x.OfferingId].CourseId == s.CourseId &&
            !hs.Any(h => h.StudentId == x.StudentId && !h.ReleasedAt.HasValue && (h.CourseId == null || h.CourseId == s.CourseId)))
            .Select(x => x.StudentId).Distinct().OrderBy(x => x).ToArray());
        if (ss.Any(s => s.SittingType != "Regular") || allSeats.Any(a => a.MakeupDay.HasValue || a.ClashConfirmedBy.HasValue))
            throw new BusinessRuleException("Existing make-up allocations must be preserved; use manual seat adjustments.");
        // Keep valid existing assignments (including manual changes); release seats of newly ineligible students.
        var removed = allSeats.Where(a => eligibility.TryGetValue(a.SittingId, out var idsForSitting) && !idsForSitting.Contains(a.StudentId)).ToArray();
        var kept = allSeats.Except(removed).ToList();
        var demands = await ScheduleDemands(e, scheduled, kept, ct);
        var hallDefinitions = halls.Select(h => new ExamHall(h.RoomId, h.Capacity)).ToArray();
        var plan = new Dictionary<int, HashSet<int>>();
        foreach (var day in demands.GroupBy(x => x.Slot.Date))
        {
            if (!ExamScheduleRules.TryPlan(day.ToArray(), hallDefinitions, out var dayPlan, out var reason)) throw new BusinessRuleException(reason);
            foreach (var reservation in dayPlan) plan.Add(reservation.Key, reservation.Value);
        }
        foreach (var a in removed) seats.Remove(a);
        foreach (var sitting in ss.OrderBy(x => x.ExamDate).ThenBy(x => x.StartTime).ThenBy(x => x.SittingId))
        {
            var existingSeats = kept.Where(a => a.SittingId == sitting.SittingId).ToArray();
            var occupied = existingSeats.Select(a => (a.RoomId, a.SeatNo)).ToHashSet();
            var alreadySeated = existingSeats.Select(a => a.StudentId).ToHashSet();
            var available = new Queue<(int RoomId, string SeatNo)>(halls.Where(h => plan[sitting.SittingId].Contains(h.RoomId))
                .SelectMany(h => Enumerable.Range(1, h.Capacity).Select(n => (h.RoomId, SeatNo: n.ToString(CultureInfo.InvariantCulture))))
                .Where(x => !occupied.Contains(x)));
            foreach (var student in eligibility[sitting.SittingId].Where(x => !alreadySeated.Contains(x)))
            {
                if (available.Count == 0) throw new BusinessRuleException("Insufficient exam hall capacity for concurrent sittings.");
                var seat = available.Dequeue();
                await seats.AddAsync(new SeatAllocation { SittingId = sitting.SittingId, StudentId = student,
                    RoomId = seat.RoomId, SeatNo = seat.SeatNo }, ct);
            }
        }
        await Notify(es.Select(x=>x.StudentId),"exam.routine",$"exam.routine:{e.EventId}",ct);await Log(actor,"Generate exam seats","ExamEvent",e.EventId,"Eligibility checked and seats allocated",ct);
    }
    public async Task MoveSeatAsync(int eventId,int sittingId,string studentId,int roomId,int seatNo,CancellationToken ct=default)
    {
        await RequireController(Actor,"seatplan.manage",ct);await work.ExecuteTransactionAsync(async token=>{
            var e=await Event(eventId,token);if(e.Status!="Scheduled")throw new BusinessRuleException("Only scheduled events can be adjusted.");var s=await sittings.FirstAsync(x=>x.SittingId==sittingId&&x.EventId==eventId,token)??throw new NotFoundException();var a=await seats.FirstAsync(x=>x.SittingId==sittingId&&x.StudentId==studentId,token)??throw new NotFoundException();
            if(a.MakeupDay.HasValue||a.ClashConfirmedBy.HasValue)throw new BusinessRuleException("Use the make-up seat assignment for a confirmed clash.");var room=await rooms.FirstAsync(x=>x.RoomId==roomId&&x.RoomType=="Exam hall",token)??throw new BusinessRuleException("Select an exam hall.");if(seatNo<1||seatNo>room.Capacity)throw new BusinessRuleException("Seat number is outside room capacity.");var number=seatNo.ToString(CultureInfo.InvariantCulture);if(await seats.CountAsync(x=>x.SittingId==sittingId&&x.RoomId==roomId&&x.SeatNo==number&&x.SeatId!=a.SeatId,token)>0)throw new BusinessRuleException("Seat already allocated.");var overlaps=await sittings.ListAsync(x=>x.SittingId!=sittingId&&x.ExamDate==s.ExamDate&&x.StartTime<s.EndTime&&s.StartTime<x.EndTime,token);var overlapIds=overlaps.Select(x=>x.SittingId).ToArray();if(await seats.CountAsync(x=>overlapIds.Contains(x.SittingId)&&x.RoomId==roomId,token)>0)throw new BusinessRuleException("Room already allocated to a concurrent sitting.");
            if(await holds.CountAsync(x=>x.EventId==eventId&&x.StudentId==studentId&&x.ReleasedAt==null&&(x.CourseId==null||x.CourseId==s.CourseId),token)>0)throw new BusinessRuleException("Student has an active hold.");
            // A manual move must not consume halls needed by other (including not-yet-seated) exams.
            var sameDay = await sittings.ListAsync(x => x.ExamDate == s.ExamDate, token);
            var allocations = await PeriodSeats(sameDay, token);
            var demands = await ScheduleDemands(e, sameDay, allocations, token);
            var updated = demands.Select(d => d.Key != sittingId ? d : d with { FixedSeats = d.FixedSeats.Select(f =>
                f.StudentId == studentId ? new ReservedExamSeat(studentId, roomId, number) : f).ToArray() }).ToArray();
            if (!ExamScheduleRules.TryPlan(updated, await ExamHalls(token), out _, out var reason)) throw new BusinessRuleException(reason);
            a.RoomId=roomId;a.SeatNo=number;await Log(Actor,"Adjust exam seat","SeatAllocation",a.SeatId,$"Room {roomId}, seat {number}",token);
        },ct);
    }
    private async Task<MarkComponent[]> Parts(Offering o,CancellationToken ct){var b=await Batch(o.BatchId,ct);var c=await Course(o.CourseId,ct);var t=await Term(o.TermId,ct);var rows=await components.ListAsync(x=>x.DepartmentId==b.DepartmentId&&x.CourseType==c.CourseType&&x.EffectiveFrom<=t.StartDate,ct);var v=rows.OrderByDescending(x=>x.EffectiveFrom).ThenByDescending(x=>x.VersionNo).FirstOrDefault()?.VersionNo;var parts=rows.Where(x=>x.VersionNo==v).OrderBy(x=>x.ComponentId).ToArray();if(parts.Length==0||parts.Sum(x=>x.MaxMarks)!=100)throw new BusinessRuleException("Configure a valid 100-mark scheme for the offering first.");return parts;}
    private Task<List<Enrollment>> Examinees(int id,CancellationToken ct)=>enrollments.ListAsync(x=>x.OfferingId==id&&((x.Status=="Confirmed"&&x.EnrollmentType=="Regular")||((x.Status=="Confirmed"||x.Status=="Completed")&&x.EnrollmentType=="Retake")),ct);
    private async Task<MarkComponent[]> RetakeParts(Offering o,CancellationToken ct)
    {var dept=(await Batch(o.BatchId,ct)).DepartmentId;var t=await Term(o.TermId,ct);var type=(await Course(o.CourseId,ct)).CourseType;var key=RetakeSchemeRules.Key(type);return RetakeSchemeRules.Resolve(await components.ListAsync(x=>x.DepartmentId==dept&&x.CourseType==key,ct),t.StartDate,type);}
    private async Task<MarkComponent[]> ApplicableParts(Offering o,List<Enrollment> es,CancellationToken ct)
    {var list=new List<MarkComponent>();if(es.Any(x=>x.EnrollmentType=="Regular")||es.Count==0)list.AddRange(await Parts(o,ct));if(es.Any(x=>x.EnrollmentType=="Retake"))list.AddRange(await RetakeParts(o,ct));return list.ToArray();}
    private static bool Applies(MarkComponent p,Enrollment en)=>(RetakeSchemeRules.IsRetake(p.CourseType))== (en.EnrollmentType=="Retake");
    private async Task<MarkSubmission?> Submission(int id,CancellationToken ct)=>(await submissions.ListAsync(x=>x.OfferingId==id,ct)).OrderByDescending(x=>x.SubmissionId).FirstOrDefault();
    public async Task<MarksPage> MarksAsync(int id,CancellationToken ct=default)
    {
        var o=await Offering(id,ct);var dept=(await Batch(o.BatchId,ct)).DepartmentId;var enter=await CanEnter(id,dept,ct);var verify=await CanVerify(dept,ct);var approve=await Controller(Actor,"marks.approve",ct);if(!enter&&!verify&&!approve)throw new ForbiddenException();var sub=await Submission(id,ct);var es=await Examinees(id,ct);var ps=await ApplicableParts(o,es,ct);var examinees=new List<Examinee>();var enrollmentIds=es.Select(x=>x.EnrollmentId).ToArray();var dispositions=approve?await results.ListAsync(x=>enrollmentIds.Contains(x.EnrollmentId)&&x.PublishedAt==null,ct):new List<Result>();foreach(var e in es)examinees.Add(new(e.EnrollmentId,e.StudentId,(await Student(e.StudentId,ct)).FullName,dispositions.Where(x=>x.EnrollmentId==e.EnrollmentId).OrderByDescending(x=>x.VersionNo).FirstOrDefault()?.ResultStatus??"Normal",e.EnrollmentType));var existing=sub is null?new List<MarkEntry>():await entries.ListAsync(x=>x.SubmissionId==sub.SubmissionId,ct);return new(id,(await Course(o.CourseId,ct)).Title,sub?.Status??"Draft",examinees.ToArray(),ps.Select(p=>new MarkPart(p.ComponentId,p.Name,p.MaxMarks,p.IsAttendanceBased,RetakeSchemeRules.IsRetake(p.CourseType))).ToArray(),existing.Select(x=>new MarkCell(x.EnrollmentId,x.ComponentId,x.Marks,x.IsAbsent)).ToArray(),enter,verify,approve,enter&&await CanEnterRetake(id,ct));
    }
    public async Task SaveMarksAsync(int id,IReadOnlyList<MarkInput> input,CancellationToken ct=default)
    {
        var o=await Offering(id,ct);if(!await CanEnter(id,(await Batch(o.BatchId,ct)).DepartmentId,ct))throw new ForbiddenException();if(input.Count==0||input.Select(x=>(x.EnrollmentId,x.ComponentId)).Distinct().Count()!=input.Count)throw new BusinessRuleException("Provide unique mark cells.");
        await work.ExecuteTransactionAsync(async token=>{var sub=await Submission(id,token);if(sub is not null&&sub.Status is not ("Draft" or "Returned"))throw new BusinessRuleException("Marks are locked for review or publication.");var es=await Examinees(id,token);var ps=await ApplicableParts(o,es,token);if(sub is null){sub=new MarkSubmission{OfferingId=id,Status="Draft",SubmittedBy=Actor};await submissions.AddAsync(sub,token);await work.SaveChangesAsync(token);}var old=await entries.ListAsync(x=>x.SubmissionId==sub.SubmissionId,token);
            var retakeMarker=await CanEnterRetake(id,token);foreach(var m in input){var p=ps.FirstOrDefault(x=>x.ComponentId==m.ComponentId);if(p is null||p.IsAttendanceBased||(RetakeSchemeRules.IsRetake(p.CourseType)&&!retakeMarker)||!es.Any(x=>x.EnrollmentId==m.EnrollmentId&&Applies(p,x))||(!m.Absent&&m.Marks.HasValue&&(m.Marks.Value<0||m.Marks.Value>p.MaxMarks||ExamRules.Round(m.Marks.Value)!=m.Marks.Value)))throw new BusinessRuleException("Invalid enrollment, component or mark range.");var row=old.FirstOrDefault(x=>x.EnrollmentId==m.EnrollmentId&&x.ComponentId==m.ComponentId);if(row is null){row=new MarkEntry{EnrollmentId=m.EnrollmentId,ComponentId=m.ComponentId,SubmissionId=sub.SubmissionId};await entries.AddAsync(row,token);}row.Marks=m.Absent?0:m.Marks.HasValue?ExamRules.Round(m.Marks.Value):null;row.IsAbsent=m.Absent;row.EnteredBy=Actor;}
        },ct);
    }
    private async Task<(decimal Marks,int Version)> AttendanceMarks(Offering o,Enrollment en,decimal maximum,CancellationToken ct)
    {var dept=(await Batch(o.BatchId,ct)).DepartmentId;var t=await Term(o.TermId,ct);var final=await events.FirstAsync(x=>x.TermId==o.TermId&&x.EventType=="Final",ct)??throw new BusinessRuleException("Create final exam event first.");var pct=await Attendance(en,final.StartDate,ct)??throw new BusinessRuleException("Attendance must be recorded before submission.");var rows=await scales.ListAsync(x=>x.DepartmentId==dept&&x.EffectiveFrom<=t.StartDate,ct);var version=rows.OrderByDescending(x=>x.EffectiveFrom).ThenByDescending(x=>x.VersionNo).FirstOrDefault()?.VersionNo??throw new BusinessRuleException("Configure attendance scale.");return(converter.Convert(pct,maximum,rows.Where(x=>x.VersionNo==version).Select(x=>(x.MinPercent,x.MarkPercent))),version);}
    private async Task<int> AttendanceVersion(Offering o,CancellationToken ct){var dept=(await Batch(o.BatchId,ct)).DepartmentId;var t=await Term(o.TermId,ct);var rows=await scales.ListAsync(x=>x.DepartmentId==dept&&x.EffectiveFrom<=t.StartDate,ct);return rows.OrderByDescending(x=>x.EffectiveFrom).ThenByDescending(x=>x.VersionNo).FirstOrDefault()?.VersionNo??throw new BusinessRuleException("Configure attendance scale.");}
    public async Task SetDispositionAsync(int offeringId,int enrollmentId,string status,string reason,CancellationToken ct=default)
    {
        await RequireController(Actor,"results.publish",ct);if(status is not ("Normal" or "Withheld" or "Expelled")||string.IsNullOrWhiteSpace(reason)||reason.Length>255)throw new BusinessRuleException("Select a valid disposition and provide a reason.");
        await work.ExecuteTransactionAsync(async token=>{var o=await Offering(offeringId,token);var sub=await Submission(offeringId,token);if(sub?.Status!="Approved")throw new BusinessRuleException("Only approved, unpublished marks may receive a disposition.");var en=await enrollments.FirstAsync(x=>x.EnrollmentId==enrollmentId&&x.OfferingId==offeringId&&x.Status=="Confirmed",token)??throw new NotFoundException();var history=await results.ListAsync(x=>x.EnrollmentId==enrollmentId,token);if(history.Any(x=>x.PublishedAt.HasValue))throw new BusinessRuleException("Use the result correction workflow after publication.");var old=history.OrderByDescending(x=>x.VersionNo).FirstOrDefault();var grade=await Grade(o,0,null,token);var ps=en.EnrollmentType=="Retake"?await RetakeParts(o,token):await Parts(o,token);var av=ps.Any(x=>x.IsAttendanceBased)?await AttendanceVersion(o,token):0;await results.AddAsync(new Result{EnrollmentId=en.EnrollmentId,VersionNo=(old?.VersionNo??0)+1,TotalMarks=0,LetterGrade="-",GradePoint=0,ResultStatus=status,GradeScaleVersion=grade.VersionNo,AttendanceScaleVersion=av,SupersedesResultId=old?.ResultId,CorrectionReason=reason},token);await Log(Actor,"Set result disposition","Enrollment",enrollmentId,status+": "+reason,token);},ct);
    }
    public async Task TransitionAsync(int id,string action,string? comment,CancellationToken ct=default)
    {
        var o=await Offering(id,ct);var dept=(await Batch(o.BatchId,ct)).DepartmentId;var actor=Actor;
        await work.ExecuteTransactionAsync(async token=>{var sub=await Submission(id,token)??throw new BusinessRuleException("Save marks first.");var rows=await entries.ListAsync(x=>x.SubmissionId==sub.SubmissionId,token);
            if(action=="Submit") {if(!await CanEnter(id,dept,token))throw new ForbiddenException();if(sub.Status is not ("Draft" or "Returned"))throw new BusinessRuleException("Invalid submission state.");var final=await events.FirstAsync(x=>x.TermId==o.TermId&&x.EventType==(o.IsSpecialRetake?"Special":"Final"),token)??throw new BusinessRuleException("Create the final exam event first.");if(clock.Today<=final.EndDate)throw new BusinessRuleException("Final marks may be submitted after the final examination period.");await EnsureMakeupsComplete(final.EventId,token);var es=await Examinees(id,token);if(o.IsSpecialRetake&&await sittings.CountAsync(x=>x.EventId==final.EventId&&x.CourseId==o.CourseId,token)==0)throw new BusinessRuleException("Schedule the special retake first.");var ps=await ApplicableParts(o,es,token);if(es.Count==0)throw new BusinessRuleException("No confirmed enrollments.");foreach(var en in es)foreach(var p in ps.Where(p=>Applies(p,en))){var m=rows.FirstOrDefault(x=>x.EnrollmentId==en.EnrollmentId&&x.ComponentId==p.ComponentId);if(p.IsAttendanceBased){var value=await AttendanceMarks(o,en,p.MaxMarks,token);if(m is null){m=new MarkEntry{EnrollmentId=en.EnrollmentId,ComponentId=p.ComponentId,SubmissionId=sub.SubmissionId};await entries.AddAsync(m,token);}m.Marks=value.Marks;m.IsAbsent=false;m.EnteredBy=actor;}else if(m?.Marks is null)throw new BusinessRuleException("Every required mark must be entered or marked absent.");}sub.SubmittedBy=actor;sub.SubmittedAt=clock.Now;sub.VerifiedBy=null;sub.VerifiedAt=null;sub.ApprovedBy=null;sub.ApprovedAt=null;sub.Status="Submitted";}
            else if(action=="Verify"||action=="ReturnHead"){if(!await CanVerify(dept,token)||sub.SubmittedBy==actor||rows.Any(x=>x.EnteredBy==actor))throw new ForbiddenException();if(sub.Status!="Submitted")throw new BusinessRuleException("Only submitted marks can be reviewed.");if(action=="Verify"){sub.VerifiedBy=actor;sub.VerifiedAt=clock.Now;sub.Status="Verified";}else Return(sub,comment);}
            else if(action=="Approve"||action=="ReturnController"){await RequireController(actor,"marks.approve",token);if(sub.SubmittedBy==actor||sub.VerifiedBy==actor||rows.Any(x=>x.EnteredBy==actor))throw new ForbiddenException();if(sub.Status!="Verified")throw new BusinessRuleException("Only verified marks can be reviewed.");if(action=="Approve"){sub.ApprovedBy=actor;sub.ApprovedAt=clock.Now;sub.Status="Approved";}else Return(sub,comment);}
            else throw new BusinessRuleException("Unknown transition.");await Log(actor,"Mark submission "+action,"MarkSubmission",sub.SubmissionId,comment??sub.Status,token);
        },ct);
    }
    private static void Return(MarkSubmission sub,string? comment){if(string.IsNullOrWhiteSpace(comment)||comment.Length>255)throw new BusinessRuleException("Return comment required (maximum 255 characters).");sub.ReturnComment=comment;sub.Status="Returned";}
    private async Task<GradeScale> Grade(Offering o,decimal total,int? pinned,CancellationToken ct){var dept=(await Batch(o.BatchId,ct)).DepartmentId;var t=await Term(o.TermId,ct);var all=await grades.ListAsync(x=>x.DepartmentId==dept&&x.EffectiveFrom<=t.StartDate,ct);var ver=pinned??all.OrderByDescending(x=>x.EffectiveFrom).ThenByDescending(x=>x.VersionNo).FirstOrDefault()?.VersionNo??throw new BusinessRuleException("Configure grade scale.");var g=all.SingleOrDefault(x=>x.VersionNo==ver&&ExamRules.InBand(total,x.MinMark,x.MaxMark))??throw new BusinessRuleException("Grade scale has a gap or is invalid.");if(total<40&&(g.LetterGrade!="F"||g.GradePoint!=0)||total>=40&&(g.LetterGrade=="F"||g.GradePoint<=0))throw new BusinessRuleException("The grade scale must preserve the 40-mark pass boundary.");return g;}
    private async Task Publish(ExamEvent e,int actor,CancellationToken ct)
    {
        await EnsureMakeupsComplete(e.EventId,ct);
        if(e.EventType is not ("Final" or "Special")||clock.Today<=e.EndDate)throw new BusinessRuleException("Final examination must finish before publication.");var os=await offerings.ListAsync(x=>x.TermId==e.TermId&&x.IsSpecialRetake==(e.EventType=="Special"),ct);var affected=new HashSet<string>();
        foreach(var o in os){var es=await Examinees(o.OfferingId,ct);if(es.Count==0)continue;var sub=await Submission(o.OfferingId,ct);if(sub?.Status!="Approved")throw new BusinessRuleException("Every offering with confirmed enrollments must have approved marks.");var allParts=await ApplicableParts(o,es,ct);var ms=await entries.ListAsync(x=>x.SubmissionId==sub.SubmissionId,ct);foreach(var en in es){var ps=allParts.Where(p=>Applies(p,en)).ToArray();var history=await results.ListAsync(x=>x.EnrollmentId==en.EnrollmentId,ct);if(history.Any(x=>x.PublishedAt.HasValue))throw new BusinessRuleException("A published result already exists for an enrollment.");var draft=history.OrderByDescending(x=>x.VersionNo).FirstOrDefault();var cells=ps.Select(p=>ms.SingleOrDefault(x=>x.EnrollmentId==en.EnrollmentId&&x.ComponentId==p.ComponentId)).ToArray();if(cells.Any(x=>x?.Marks is null))throw new BusinessRuleException("Incomplete approved marks.");var total=ExamRules.Total(cells.Select(x=>x!.Marks!.Value));var finals=ps.Where(x=>en.EnrollmentType=="Retake"||x.Name.StartsWith("Final",StringComparison.OrdinalIgnoreCase)).Select(x=>x.ComponentId).ToArray();var absent=finals.Length>0&&cells.Where(x=>finals.Contains(x!.ComponentId)).All(x=>x!.IsAbsent);var disposition=draft?.ResultStatus is "Withheld" or "Expelled"?draft.ResultStatus:absent?"Absent":"Normal";var nonNumeric=disposition is "Withheld" or "Expelled";var grade=await Grade(o,absent?0:total,null,ct);var att=ps.FirstOrDefault(x=>x.IsAttendanceBased);var av=att is null?0:await AttendanceVersion(o,ct);await results.AddAsync(new Result{EnrollmentId=en.EnrollmentId,VersionNo=(draft?.VersionNo??0)+1,TotalMarks=total,LetterGrade=nonNumeric?"-":absent?"F":grade.LetterGrade,GradePoint=nonNumeric||absent?0:grade.GradePoint,ResultStatus=disposition,GradeScaleVersion=grade.VersionNo,AttendanceScaleVersion=av,PublishedAt=clock.Now,SupersedesResultId=draft?.ResultId},ct);if(en.EnrollmentType=="Retake"&&(disposition is "Normal" or "Absent")){
            var studentEnrollments=await enrollments.ListAsync(x=>x.StudentId==en.StudentId&&x.EnrollmentId!=en.EnrollmentId,ct);var previousOffers=studentEnrollments.Select(x=>x.OfferingId).ToArray();var sameCourse=(await offerings.ListAsync(x=>previousOffers.Contains(x.OfferingId)&&x.CourseId==o.CourseId,ct)).Select(x=>x.OfferingId).ToHashSet();var previousIds=studentEnrollments.Where(x=>sameCourse.Contains(x.OfferingId)&&x.AttemptNo<en.AttemptNo).Select(x=>x.EnrollmentId).ToArray();foreach(var previous in await results.ListAsync(x=>previousIds.Contains(x.EnrollmentId)&&x.PublishedAt!=null,ct))previous.IsReplaced=true;
            }if(en.EnrollmentType=="Retake")en.Status="Completed";affected.Add(en.StudentId);}sub.Status="Published";}
        if(affected.Count==0)throw new BusinessRuleException("No approved enrollments to publish.");await work.SaveChangesAsync(ct);foreach(var id in affected)await Recalculate(id,e.TermId,ct);e.Status="Completed";await Notify(affected,"exam.results",$"exam.results:{e.EventId}",ct);await Log(actor,"Publish results","ExamEvent",e.EventId,$"Published for {affected.Count} students",ct);
    }
    private async Task<ResultRow[]> History(string student,CancellationToken ct)
    {var en=await enrollments.ListAsync(x=>x.StudentId==student,ct);var ids=en.Select(x=>x.EnrollmentId).ToArray();var rs=await results.ListAsync(x=>ids.Contains(x.EnrollmentId)&&x.PublishedAt!=null,ct);var latest=rs.GroupBy(x=>x.EnrollmentId).Select(g=>g.OrderByDescending(x=>x.VersionNo).First()).ToArray();var os=await offerings.ListAsync(ct:ct);var cs=await courses.ListAsync(ct:ct);var st=await Student(student,ct);var rows=(from r in latest join n in en on r.EnrollmentId equals n.EnrollmentId join o in os on n.OfferingId equals o.OfferingId join c in cs on o.CourseId equals c.CourseId select new {r,n,o,c}).ToArray();return rows.Select(x=>new ResultRow(x.r.ResultId,student,x.c.Code,x.o.TermId,x.o.SemesterNo,x.c.Credits,x.r.TotalMarks,x.r.LetterGrade,x.r.GradePoint,x.r.ResultStatus,x.r.VersionNo,x.r.PublishedAt!.Value,x.r.IsReplaced||rows.Any(y=>y.o.CourseId==x.o.CourseId&&y.n.AttemptNo>x.n.AttemptNo),st.AdmissionBatchId,x.n.AttemptNo)).ToArray();}
    private async Task Recalculate(string student,int termId,CancellationToken ct)
    {
        var history=await History(student,ct);var term=await Term(termId,ct);var previous=await terms.ListAsync(t=>t.StartDate<=term.StartDate,ct);var termIds=previous.Select(t=>t.TermId).ToHashSet();var semester=history.Where(r=>r.TermId==termId).ToArray();if(semester.Length==0)return;
        var numeric=semester.Where(r=>r.Attempt==1&&(r.Status is "Normal" or "Absent")).ToArray();var currentRows=history.Where(r=>termIds.Contains(r.TermId)).GroupBy(r=>r.Course).Select(g=>g.OrderByDescending(r=>r.Attempt).ThenByDescending(r=>previous.Single(t=>t.TermId==r.TermId).StartDate).ThenByDescending(r=>r.Version).First()).Where(r=>r.Status is "Normal" or "Absent").ToArray();
        var old=(await semesterResults.ListAsync(x=>x.StudentId==student&&x.TermId==termId,ct)).OrderByDescending(x=>x.VersionNo).FirstOrDefault();await semesterResults.AddAsync(new SemesterResult{StudentId=student,TermId=termId,SemesterNo=(byte)(semester.Where(x=>x.Attempt==1).Select(x=>x.Semester).DefaultIfEmpty(semester.Max(x=>x.Semester)).Max()),CreditsEarned=numeric.Where(x=>x.Points>0).Sum(x=>x.Credits),SemesterGpa=ExamRules.Gpa(numeric.Select(x=>(x.Credits,x.Points))),Cgpa=ExamRules.Gpa(currentRows.Select(x=>(x.Credits,x.Points))),VersionNo=(old?.VersionNo??0)+1,SupersedesId=old?.SemesterResultId,PublishedAt=clock.Now},ct);
    }
    public async Task CorrectAsync(int id,decimal total,string status,string reason,int approvalId,CancellationToken ct=default)
    {
        await RequireController(Actor,"results.correct",ct);if(total<0||total>100||status is not ("Normal" or "Absent" or "Withheld" or "Expelled")||string.IsNullOrWhiteSpace(reason)||reason.Length>255)throw new BusinessRuleException("Invalid correction values.");await work.ExecuteTransactionAsync(async token=>{var old=await results.FirstAsync(x=>x.ResultId==id&&x.PublishedAt!=null,token)??throw new NotFoundException();if(await results.CountAsync(x=>x.EnrollmentId==old.EnrollmentId&&x.VersionNo>old.VersionNo,token)>0)throw new BusinessRuleException("Correct the latest result only.");var approval=await approvals.FirstAsync(x=>x.ApprovalId==approvalId&&x.Status=="Approved"&&x.ApprovalType=="ResultCorrection",token)??throw new BusinessRuleException("Record an approved physical reference first.");var en=await enrollments.FirstAsync(x=>x.EnrollmentId==old.EnrollmentId,token)??throw new NotFoundException();var o=await Offering(en.OfferingId,token);var deptId=(await Batch(o.BatchId,token)).DepartmentId;var dept=await departments.FirstAsync(x=>x.DepartmentId==deptId,token)??throw new NotFoundException();if(!approval.ReferenceNo.StartsWith(dept.Code+"-",StringComparison.OrdinalIgnoreCase)&&!approval.ReferenceNo.StartsWith("UNI-",StringComparison.OrdinalIgnoreCase))throw new BusinessRuleException("Approval reference belongs to another department.");if(await approvals.CountAsync(x=>x.SupersedesId==approvalId,token)>0)throw new BusinessRuleException("Approval reference was superseded.");var g=await Grade(o,status=="Absent"?0:ExamRules.Round(total),old.GradeScaleVersion,token);await results.AddAsync(new Result{EnrollmentId=en.EnrollmentId,VersionNo=old.VersionNo+1,TotalMarks=ExamRules.Round(total),LetterGrade=status is "Withheld" or "Expelled"?"-":status=="Absent"?"F":g.LetterGrade,GradePoint=status is "Withheld" or "Expelled" or "Absent"?0:g.GradePoint,ResultStatus=status,GradeScaleVersion=g.VersionNo,AttendanceScaleVersion=old.AttendanceScaleVersion,PublishedAt=clock.Now,SupersedesResultId=id,CorrectionReason=reason,ApprovalId=approval.ApprovalId,IsReplaced=old.IsReplaced},token);await work.SaveChangesAsync(token);await Recalculate(en.StudentId,o.TermId,token);var correctedTerm=await Term(o.TermId,token);var laterTerms=await terms.ListAsync(x=>x.StartDate>correctedTerm.StartDate,token);var previousSnapshots=await semesterResults.ListAsync(x=>x.StudentId==en.StudentId,token);foreach(var later in laterTerms.Where(t=>previousSnapshots.Any(x=>x.TermId==t.TermId)).OrderBy(t=>t.StartDate))await Recalculate(en.StudentId,later.TermId,token);await Log(Actor,"Correct published result","Result",id,reason,token);},ct);
    }
    public async Task<SeatRow[]> AdmitSeatsAsync(int eventId,CancellationToken ct=default) => (await StudentAsync(ct:ct)).Seats.Where(x=>x.EventId==eventId).ToArray();
    public async Task<StudentExamPage> StudentAsync(string? studentId=null,CancellationToken ct=default)
    {
        var actor=Actor;var st=studentId is null?await students.FirstAsync(x=>x.UserId==actor,ct):await Student(studentId,ct);if(st is null)throw new ForbiddenException();if(st.UserId!=actor&&!await ReadResults(st.DepartmentId,ct))throw new ForbiddenException();var hs=await holds.ListAsync(x=>x.StudentId==st.StudentId,ct);var hr=new List<HoldRow>();foreach(var h in hs)hr.Add(new(h.HoldId,h.StudentId,h.CourseId.HasValue?(await Course(h.CourseId.Value,ct)).Code:"All courses",h.HoldType,h.Reason,h.ReleasedAt.HasValue,false));var alloc=await seats.ListAsync(x=>x.StudentId==st.StudentId,ct);var sr=new List<SeatRow>();foreach(var a in alloc){if(a.ClashConfirmedBy.HasValue&&!a.MakeupDay.HasValue)continue;var s=await sittings.FirstAsync(x=>x.SittingId==a.SittingId,ct);if(s is null||hs.Any(h=>h.EventId==s.EventId&&!h.ReleasedAt.HasValue&&(h.CourseId==null||h.CourseId==s.CourseId)))continue;var e=await Event(s.EventId,ct);if(e.Status is not ("Scheduled" or "Completed"))continue;if(e.Status=="Scheduled"){
            if(!hs.Any(h=>h.EventId==e.EventId&&h.HoldType=="Dues"&&h.ReleasedAt.HasValue&&h.ReleaseReason!="Condition cleared on recheck")&&!await fees.IsClearAsync(st.StudentId,e.TermId,ct)){if(!hr.Any(h=>h.Type=="Dues"&&!h.Released))hr.Add(new(0,st.StudentId,"All courses","Dues","Outstanding semester dues; ask the Controller to refresh eligibility.",false,false));continue;}
            var currentOffers=await offerings.ListAsync(o=>o.TermId==e.TermId&&o.IsSpecialRetake==(e.EventType=="Special")&&o.CourseId==s.CourseId,ct);var currentIds=currentOffers.Select(o=>o.OfferingId).ToArray();var enrollment=await enrollments.FirstAsync(x=>x.StudentId==st.StudentId&&currentIds.Contains(x.OfferingId)&&x.Status=="Confirmed"&&(x.EnrollmentType=="Regular"||e.EventType=="Final"||e.EventType=="Special"),ct);if(enrollment is null)continue;
            if(enrollment.EnrollmentType=="Regular"&&!hs.Any(h=>h.EventId==e.EventId&&h.CourseId==s.CourseId&&h.HoldType=="Attendance"&&h.ReleasedAt.HasValue&&h.ReleaseReason!="Condition cleared on recheck")){var offer=currentOffers.Single(o=>o.OfferingId==enrollment.OfferingId);var dept=(await Batch(offer.BatchId,ct)).DepartmentId;var cfg=await config.ResolveAsync(ConfigKeys.AttendanceMinimum,dept,e.TermId,ct);var limit=decimal.TryParse(cfg,NumberStyles.Number,CultureInfo.InvariantCulture,out var n)?Math.Clamp(n,75,100):75;var pct=await Attendance(enrollment,e.StartDate,ct);if(pct is null||pct<limit){hr.Add(new(0,st.StudentId,(await Course(s.CourseId,ct)).Code,"Attendance","Attendance below threshold or not recorded; ask the Controller to refresh eligibility.",false,false));continue;}}
        }sr.Add(await SeatDto(a,s,ct));}var g=await semesterResults.ListAsync(x=>x.StudentId==st.StudentId,ct);var specialEnrollments=await enrollments.ListAsync(x=>x.StudentId==st.StudentId&&x.EnrollmentType=="Retake"&&(x.Status=="Confirmed"||x.Status=="Completed"),ct);var specialIds=specialEnrollments.Select(x=>x.OfferingId).ToArray();var specialTerms=(await offerings.ListAsync(x=>specialIds.Contains(x.OfferingId)&&x.IsSpecialRetake,ct)).Select(x=>x.TermId).ToArray();var specialEvents=(await events.ListAsync(x=>specialTerms.Contains(x.TermId)&&x.EventType=="Special"&&x.Status!="Draft",ct)).Select(Row).ToArray();return new(st.StudentId,sr.ToArray(),hr.ToArray(),await History(st.StudentId,ct),g.GroupBy(x=>x.TermId).Select(x=>x.OrderByDescending(y=>y.VersionNo).First()).Select(x=>new GpaRow(x.TermId,x.SemesterNo,x.CreditsEarned,x.SemesterGpa,x.Cgpa,x.VersionNo)).ToArray(),st.UserId==actor,specialEvents);
    }
    public async Task AuditExportAsync(string document,string identifier,CancellationToken ct=default){await Log(Actor,"Export examination document","Examination",0,document+" / "+identifier,ct);await work.SaveChangesAsync(ct);}
    public async Task<ResultRow[]> ReportAsync(int termId,int? batchId,CancellationToken ct=default)
    {var bs=await batches.ListAsync(ct:ct);var allowed=new List<int>();foreach(var b in bs)if(await ReadResults(b.DepartmentId,ct))allowed.Add(b.BatchId);if(allowed.Count==0||batchId.HasValue&&!allowed.Contains(batchId.Value))throw new ForbiddenException();var ss=await students.ListAsync(x=>allowed.Contains(x.AdmissionBatchId)&&(!batchId.HasValue||x.AdmissionBatchId==batchId),ct);var list=new List<ResultRow>();foreach(var s in ss)list.AddRange((await History(s.StudentId,ct)).Where(x=>x.TermId==termId));return list.ToArray();}
}
