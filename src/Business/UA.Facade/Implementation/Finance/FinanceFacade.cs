using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Finance;
using UA.Entities;
using UA.Facade.DTO.Finance;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Finance;
using UA.Facade.Rules.Finance;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Finance;
public sealed class FinanceFacade(IFeeTypeRepository rates, IDueRepository dues, IPaymentRepository payments,
    IRepository<PaymentAllocation> allocations, IRepository<Receipt> receipts, IRepository<UA.Entities.Student> students,
    IRepository<Department> departments, IRepository<Batch> batches, IRepository<Section> sections,
    IRepository<AcademicTerm> terms, IRepository<CurriculumCourse> curriculum, IRepository<Course> courses,
    IRepository<SemesterDrop> drops, IRepository<Enrollment> enrollments, IRepository<Offering> offerings, IRepository<Result> results, IRepository<CourseEquivalence> equivalents, IRepository<AcademicCalendarEvent> calendar, IUnitOfWork work, IUserAccessFacade access, ICurrentUser current, IClock clock,
    IFeeCalculator calculator, ILateFeePolicy late, IPaymentAllocator allocator, IReceiptNumberGenerator numbering)
    : IFeeTypeFacade, IDueFacade, IDueGenerator, IFeeClearanceChecker, ILedgerFacade, IPaymentFacade
{
    private async Task<UA.Entities.Student> Student(string id, CancellationToken ct) => await students.FirstAsync(s => s.StudentId == id,ct) ?? throw new NotFoundException("Student not found.");
    private async Task<UA.Entities.Student> Own(CancellationToken ct) { var actor = current.UserId ?? throw new ForbiddenException(); return await students.FirstAsync(s => s.UserId == actor,ct) ?? throw new ForbiddenException("Student account required."); }
    private async Task<int[]> Allowed(string permission, CancellationToken ct) { await access.RequireAsync(permission,ct:ct); var id = current.UserId ?? throw new ForbiddenException(); var list = new List<int>(); foreach(var d in await departments.ListAsync(ct:ct)) if(await access.AllowedAsync(id,permission,d.DepartmentId,ct)) list.Add(d.DepartmentId); return list.ToArray(); }
    public async Task<FinanceLookup> LookupAsync(string permission, CancellationToken ct = default)
    { if(permission is not ("fees.manage" or "dues.view" or "ledger.view" or "payments.record")) throw new ForbiddenException(); var allowed = await Allowed(permission,ct); var bs = await batches.ListAsync(b=>allowed.Contains(b.DepartmentId),ct); var ids=bs.Select(b=>b.BatchId).ToArray(); return new((await departments.ListAsync(d=>allowed.Contains(d.DepartmentId),ct)).Select(d=>KeyValuePair.Create(d.DepartmentId,d.Code)).ToArray(),bs.Select(b=>KeyValuePair.Create(b.BatchId,$"{b.AdmissionYear} (department {b.DepartmentId}, batch {b.BatchId})")).ToArray(),(await sections.ListAsync(s=>ids.Contains(s.BatchId),ct)).Select(s=>KeyValuePair.Create(s.SectionId,$"{s.Name} (batch {s.BatchId})")).ToArray(),(await terms.ListAsync(ct:ct)).OrderByDescending(t=>t.StartDate).Select(t=>KeyValuePair.Create(t.TermId,$"{t.Year} / {t.TermNo}")).ToArray()); }
    public async Task<IReadOnlyList<FeeRateDto>> RatesAsync(CancellationToken ct = default) { var allowed=await Allowed("fees.manage",ct); return (await rates.ListAsync(r=>allowed.Contains(r.DepartmentId),ct)).OrderBy(r=>r.DepartmentId).ThenBy(r=>r.Name).ThenByDescending(r=>r.EffectiveFrom).Select(r=>new FeeRateDto(r.FeeTypeId,r.DepartmentId,r.Name,r.Unit,r.Amount,r.EffectiveFrom)).ToArray(); }
    public async Task AddRateAsync(int departmentId,string name,decimal amount,DateTime effectiveFrom,CancellationToken ct=default)
    {
        await access.RequireAsync("fees.manage",departmentId,ct);
        if(!FeeNames.Units.TryGetValue(name ?? "",out var unit) || amount < 0 || amount > 99999999.99m || decimal.Round(amount,2)!=amount || effectiveFrom.Year < 2000 || effectiveFrom.Year > 2100) throw new BusinessRuleException("Invalid fee rate, type or date.");
        if(await departments.FirstAsync(d=>d.DepartmentId==departmentId && d.Status=="Active",ct) is null) throw new BusinessRuleException("Active department required.");
        await work.ExecuteTransactionAsync(async token=> { if(await rates.FirstAsync(r=>r.DepartmentId==departmentId && r.Name==name && r.EffectiveFrom==effectiveFrom.Date,token) is not null) throw new BusinessRuleException("A rate already exists for that date. Add a new effective date."); await rates.AddAsync(new FeeType {DepartmentId=departmentId,Name=name!,Unit=unit,Amount=amount,EffectiveFrom=effectiveFrom.Date},token); },ct);
    }
    public async Task GenerateAsync(string studentId,int termId,CancellationToken ct=default) { var student=await Student(studentId,ct); await access.RequireAsync("fees.manage",student.DepartmentId,ct); await work.ExecuteTransactionAsync(token=>GenerateSemesterAsync(studentId,termId,token),ct); }
    // Transaction-participating entry point for the later registration workflow. Caller owns serializable transaction and save.
    public async Task GenerateSemesterAsync(string studentId,int termId,CancellationToken ct=default)
    {
        var s=await Student(studentId,ct); var t=await terms.FirstAsync(x=>x.TermId==termId,ct) ?? throw new NotFoundException("Term not found.");
        if(s.Status!="Active") return;
        var admission=await batches.FirstAsync(x=>x.BatchId==s.AdmissionBatchId,ct)??throw new BusinessRuleException("Missing admission batch.");if(clock.Today.Year>admission.AdmissionYear+6||t.Year>admission.AdmissionYear+6)return;
        var b=await batches.FirstAsync(x=>x.BatchId==s.CurrentBatchId,ct) ?? throw new BusinessRuleException("Missing batch.");
        int semester=(t.Year-b.AdmissionYear)*2+t.TermNo; if(await drops.FirstAsync(d=>d.StudentId==studentId && d.DroppedSemesterNo==semester && (d.Status=="Approved"||d.Status=="Requested"||d.Status=="Recommended"),ct) is not null) return; if(semester<1 || semester>8) throw new BusinessRuleException("Term is outside the student's current curriculum.");
        var links=await curriculum.ListAsync(c=>c.CurriculumId==b.CurriculumId && c.SemesterNo==semester,ct); var ids=links.Select(c=>c.CourseId).ToArray(); if(ids.Length==0)throw new BusinessRuleException("No curriculum courses for the attended semester.");var recognized=s.AdmissionBatchId==s.CurrentBatchId?new HashSet<int>():await UA.Facade.Rules.Student.RejoinCreditRecognition.PassedAsync(studentId,enrollments,offerings,results,equivalents,termId,ct);var credits=(await courses.ListAsync(c=>ids.Contains(c.CourseId),ct)).Where(c=>!recognized.Contains(c.CourseId)).Sum(c=>c.Credits);
        var available=await rates.ListAsync(r=>r.DepartmentId==s.DepartmentId,ct); var old=await dues.ListAsync(d=>d.StudentId==studentId && d.TermId==termId && d.EnrollmentId==null,ct); var oldIds=old.Select(d=>d.FeeTypeId).ToHashSet();
        var previousNames=available.Where(r=>oldIds.Contains(r.FeeTypeId)).Select(r=>r.Name).ToHashSet();
        foreach(var item in new[]{(FeeNames.Registration,1m),(FeeNames.Tuition,credits)}) if(DueGenerationRules.ShouldCreate(s.Status, previousNames, item.Item1)) { var rate=calculator.Rate(available,item.Item1,t.StartDate); var amount=calculator.Calculate(rate.Amount,item.Item2); await dues.AddAsync(new Due{StudentId=studentId,FeeTypeId=rate.FeeTypeId,TermId=termId,Amount=amount,DueDate=await FeeDeadline(t,ct),Status=amount==0?"Paid":"Unpaid"},ct); }
        await work.SaveChangesAsync(ct);
    }
    private async Task<DateTime> FeeDeadline(AcademicTerm t,CancellationToken ct)
    {
        var dates=await calendar.ListAsync(e=>e.TermId==t.TermId && e.EventType=="Fee deadline",ct);
        return dates.Count==0 ? t.RegistrationDeadline : dates.Min(e=>e.EndDate);
    }
    // Foundation for the Phase 9 scheduled job; no scheduler is introduced in Phase 5.
    // Caller owns the transaction, as with semester generation.
    public async Task GenerateLateFeeAsync(string studentId,int termId,CancellationToken ct=default)
    {
        var s=await Student(studentId,ct); if(s.Status!="Active") return;
        var admission=await batches.FirstAsync(x=>x.BatchId==s.AdmissionBatchId,ct)??throw new BusinessRuleException("Missing admission batch.");if(clock.Today.Year>admission.AdmissionYear+6)return;
        var t=await terms.FirstAsync(x=>x.TermId==termId,ct) ?? throw new NotFoundException("Term not found.");
        var available=await rates.ListAsync(r=>r.DepartmentId==s.DepartmentId,ct);
        var rows=(await Rows(s,ct)).Where(d=>d.TermId==termId).ToArray();
        if(rows.Any(d=>d.Fee==FeeNames.Late) || !rows.Any(d=>(d.Fee==FeeNames.Registration || d.Fee==FeeNames.Tuition) && d.Balance>0 && (d.Status is "Unpaid" or "Partially paid") && late.IsLate(clock.Today,d.Deadline))) return;
        var rate=calculator.Rate(available,FeeNames.Late,t.StartDate); var amount=calculator.Calculate(rate.Amount,1);
        await dues.AddAsync(new Due{StudentId=studentId,FeeTypeId=rate.FeeTypeId,TermId=termId,Amount=amount,DueDate=clock.Today,Status=amount==0?"Paid":"Unpaid"},ct);
        await work.SaveChangesAsync(ct);
    }
    private static DueRow Map(DueProjection d) => new(d.Id,d.StudentId,d.StudentName,d.DepartmentId,d.BatchId,d.SectionId,d.TermId,d.Fee,d.Amount,d.Paid,d.Deadline,d.Status);
    private async Task<List<DueRow>> Rows(UA.Entities.Student student,CancellationToken ct) => (await dues.RowsAsync([student.DepartmentId],student.StudentId,ct:ct)).Select(Map).ToList();
    public async Task<IReadOnlyList<DueRow>> DuesAsync(int? departmentId=null,int? batchId=null,int? sectionId=null,string? status=null,CancellationToken ct=default)
    {
        var allowed=await Allowed("dues.view",ct);
        if(departmentId.HasValue && !allowed.Contains(departmentId.Value)) throw new ForbiddenException();
        return (await dues.RowsAsync(allowed,departmentId:departmentId,batchId:batchId,sectionId:sectionId,status:status,ct:ct)).Select(Map).ToArray();
    }
    private async Task<LedgerDto> Ledger(UA.Entities.Student s,CancellationToken ct)
    {
        var rows=await Rows(s,ct);
        var history=await payments.HistoryAsync(s.StudentId,ct);
        return new(s.StudentId,rows,history.Select(p=>new PaymentRow(p.Id,p.Amount,p.PaidAt,p.Method,p.ReceiptNo)).ToArray());
    }
    public async Task<LedgerDto> StudentAsync(string studentId,CancellationToken ct=default) { var s=await Student(studentId,ct); await access.RequireAsync("ledger.view",s.DepartmentId,ct); return await Ledger(s,ct); }
    public async Task<LedgerDto> OwnAsync(CancellationToken ct=default) => await Ledger(await Own(ct),ct);
    public async Task<bool> IsClearAsync(string studentId,int? termId=null,CancellationToken ct=default) { var s=await Student(studentId,ct); return FeeClearanceRules.IsClear(await Rows(s,ct),termId); }
    public async Task<int> CounterAsync(string studentId,decimal amount,int[]? dueIds,CancellationToken ct=default)
    {
        var s=await Student(studentId,ct); await access.RequireAsync("payments.record",s.DepartmentId,ct); int id=0;
        await work.ExecuteTransactionAsync(async token=>{
            var rows=(await Rows(s,token)).Where(d=>d.Balance>0 && (d.Status is "Unpaid" or "Partially paid")).ToList();
            if(dueIds is {Length:>0}) { if(dueIds.Distinct().Count()!=dueIds.Length || dueIds.Any(k=>rows.All(d=>d.Id!=k))) throw new BusinessRuleException("Selected dues are not outstanding dues for this student."); rows=rows.Where(d=>dueIds.Contains(d.Id)).ToList(); }
            var parts=allocator.Allocate(amount,rows.Select(d=>(d.Id,d.Balance))); var p=new Payment{StudentId=studentId,Method="Counter",Amount=amount,Status="Successful",PaidAt=clock.Now,RecordedBy=current.UserId}; await payments.AddAsync(p,token); await work.SaveChangesAsync(token); id=p.PaymentId;
            foreach(var part in parts) { var d=await dues.FirstAsync(d=>d.DueId==part.DueId,token) ?? throw new NotFoundException(); await allocations.AddAsync(new PaymentAllocation{PaymentId=id,DueId=d.DueId,Amount=part.Amount},token); d.Status=rows.Single(r=>r.Id==d.DueId).Balance==part.Amount?"Paid":"Partially paid"; }
            await receipts.AddAsync(new Receipt{PaymentId=id,ReceiptNo=numbering.Generate(id,clock.Now),IssuedAt=clock.Now},token);
        },ct); return id;
    }
    public async Task<ReceiptDto> ReceiptAsync(int paymentId,bool own,CancellationToken ct=default)
    { var p=await payments.FirstAsync(p=>p.PaymentId==paymentId && p.StudentId!=null && p.Status=="Successful",ct) ?? throw new NotFoundException(); var s=await Student(p.StudentId!,ct); if(own) { if((await Own(ct)).StudentId!=s.StudentId) throw new ForbiddenException(); } else { var actor=current.UserId??throw new ForbiddenException(); if(!await access.AllowedAsync(actor,"ledger.view",s.DepartmentId,ct) && !await access.AllowedAsync(actor,"payments.record",s.DepartmentId,ct)) throw new ForbiddenException(); } var r=await receipts.FirstAsync(r=>r.PaymentId==paymentId,ct) ?? throw new NotFoundException(); var a=await allocations.ListAsync(a=>a.PaymentId==paymentId,ct); return new(r.ReceiptNo,s.StudentId,r.IssuedAt,p.Amount,a.Select(x=>$"Due {x.DueId}: BDT {x.Amount:F2}").ToArray()); }
}
