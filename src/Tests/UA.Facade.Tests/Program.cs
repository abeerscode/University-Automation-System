using UA.Facade.Rules.Examination;
using UA.Facade.Rules.Administration;
using UA.Integration.Security;
using UA.Framework.Exceptions;
using UA.Framework.Constants;
using UA.Facade.Rules.Academic;
static void Check(bool assertion, string name)
{
    if (!assertion) throw new Exception("FAILED: " + name);
    Console.WriteLine("PASS " + name);
}
var policy = new PasswordPolicy();
Check(policy.Validate("abc") is not null, "Reject short passwords");
Check(policy.Validate("password123A") is not null, "Reject common passwords");
Check(policy.Validate("validComplex123A") is null, "Accept strong passwords");
var hasher = new Pbkdf2PasswordHasher();
string hash = hasher.Hash("validComplex123A");
Check(hasher.Verify("validComplex123A", hash), "Verify password hashes");
Check(!hasher.Verify("wrongPass123A", hash), "Reject invalid passwords");
Check(hasher.Hash("validComplex123A") != hash, "Generate unique salt per password");
var tokens = new SecureTokenGenerator();
Check(tokens.GenerateToken() != tokens.GenerateToken(), "Secure random reset tokens");
Check(tokens.GenerateCode().Length == 6, "Six-digit verification codes");
Check(tokens.HashToken("sample") == tokens.HashToken("sample"), "Deterministic token hashing");
var guard = new SelfApprovalGuard();
bool blocked = false; try { guard.Check(17, 17); } catch (BusinessRuleException) { blocked = true; }
Check(blocked, "Prevent self-approval");
Check(PermissionCodes.All.Distinct().Count() == PermissionCodes.All.Length, "No duplicate permission codes");
var sections = new SectionAllocator();
var plan = sections.Plan(120, 40);
Check(plan.Count == 3 && plan.All(x => x.Capacity == 40) && plan[0].Name == "A" && plan[2].Name == "C", "P2: 120 seats create sections A/B/C with 40 each");
Check(sections.IndexForMeritRoll(1, 40) == 0 && sections.IndexForMeritRoll(40, 40) == 0 &&
    sections.IndexForMeritRoll(41, 40) == 1 && sections.IndexForMeritRoll(120, 40) == 2, "P2: merit rolls allocate fixed section ranges");
Check(sections.ChooseRejoinSection([(35, 40), (29, 40), (32, 40)], 5) == 1, "P2: rejoin goes to least occupied eligible section");
bool full = false;
try { sections.ChooseRejoinSection([(45, 40), (46, 40)], 5); } catch (BusinessRuleException) { full = true; }
Check(full, "P2: capacity plus overflow cannot be exceeded");
var deadlines = new CalendarDeadlineService();
var reg = new DateTime(2027, 1, 20); var drop = new DateTime(2027, 1, 15); var retake = new DateTime(2027, 2, 15);
deadlines.ValidateTerm(2027, 1, reg, drop, retake);
Check(deadlines.Deadline(AcademicDeadline.Drop, reg, drop, retake) == drop &&
    deadlines.IsOpen(reg, new DateTime(2027, 1, 1), AcademicDeadline.Registration, reg, drop, retake) &&
    !deadlines.IsOpen(reg.AddDays(1), new DateTime(2027, 1, 1), AcademicDeadline.Registration, reg, drop, retake), "P2: inclusive term deadline boundary");
bool badDeadline = false;
try { deadlines.ValidateTerm(2027, 1, drop, reg, retake); } catch (BusinessRuleException) { badDeadline = true; }
Check(badDeadline, "P2: refuse drop deadline after registration");
var resolver = new CurriculumVersionResolver();
CurriculumCandidate[] versions = [new(11, 1, 2025, 1, "Retired"), new(12, 1, 2027, 2, "Active"), new(13, 1, 2029, 3, "Draft")];
Check(resolver.Resolve(1, 2026, versions) == 11 && resolver.Resolve(1, 2028, versions) == 12,
    "P2: historical batches retain retired version; future admissions use active version");
bool missingVersion = false;
try { resolver.Resolve(1, 2024, versions); } catch (BusinessRuleException) { missingVersion = true; }
Check(missingVersion, "P2: no invented curriculum before earliest approved version");
Console.WriteLine("Phase 0/1/2 pure rule checks passed.");

// Phase 3 — permanent identity and controlled status transitions
var studentIds = new UA.Facade.Rules.Student.StudentIdGenerator();
Check(studentIds.Generate(2025, "01", 1) == "25101001", "P3: permanent ID follows BR-04");
Check(studentIds.Generate(2027, "04", 120) == "27104120", "P3: ID carries admission year/department/roll");
bool invalidId = false;
try { studentIds.Generate(2027, "05", 1); } catch (BusinessRuleException) { invalidId = true; }
Check(invalidId, "P3: undefined department digit rejected");
invalidId = false;
try { studentIds.Generate(2027, "01", 0); } catch (BusinessRuleException) { invalidId = true; }
Check(invalidId, "P3: zero merit roll rejected");
var studentStatus = new UA.Facade.Rules.Student.StudentStatusTransitionPolicy();
Check(studentStatus.EligibleForNewAcademicActivity("Active") &&
    !studentStatus.EligibleForNewAcademicActivity("Dropped") &&
    !studentStatus.EligibleForNewAcademicActivity("Suspended"), "P3: non-Active academic activity guard");
studentStatus.EnsurePermitted("Active", "Suspended");
studentStatus.EnsurePermitted("Suspended", "Active");
bool refusedWorkflowBypass = false;
try { studentStatus.EnsurePermitted("Active", "Graduated"); }
catch (BusinessRuleException) { refusedWorkflowBypass = true; }
Check(refusedWorkflowBypass, "P3: graduation requires later dedicated workflow");
refusedWorkflowBypass = false;
try { studentStatus.EnsurePermitted("Dropped", "Active"); }
catch (BusinessRuleException) { refusedWorkflowBypass = true; }
Check(refusedWorkflowBypass, "P3: rejoin cannot bypass later workflow");
var importReader = new UA.Integration.Import.StudentImportReader();
var importCsv = System.Text.Encoding.UTF8.GetBytes("department_code,admission_year,roll_no,section,full_name,date_of_birth,gender,guardian_name,guardian_phone,phone,address,email\r\nCSE,2027,1,A,Test Student,2005-07-01,Male,Guardian,01711111111,01722222222,Dhaka,\r\n");
var importSheet = importReader.Read(importCsv, ".csv");
Check(importSheet.Rows.Count == 1 && importSheet.Rows[0].Values["full_name"] == "Test Student",
    "P3: UTF-8 CSV import parser");
Check(importSheet.Rows[0].Line == 2 && importSheet.Headers.Count == 12,
    "P3: CSV keeps header and row number for error reports");
bool importRejected = false;
try { importReader.Read(importCsv, ".xls"); } catch (BusinessRuleException) { importRejected = true; }
Check(importRejected, "P3: legacy/unsupported file format rejected");
// Make an in-memory OpenXML workbook with no external dependency.
using (var target = new MemoryStream())
{
    using (var z = new System.IO.Compression.ZipArchive(target, System.IO.Compression.ZipArchiveMode.Create, true))
    {
        using var writer = new StreamWriter(z.CreateEntry("xl/worksheets/sheet1.xml").Open());
        writer.Write("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>" +
            "<row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>full_name</t></is></c></row>" +
            "<row r=\"2\"><c r=\"A2\" t=\"inlineStr\"><is><t>Test Student</t></is></c></row>" +
            "</sheetData></worksheet>");
    }
    var xlsx = importReader.Read(target.ToArray(), ".xlsx");
    Check(xlsx.Headers[0] == "full_name" && xlsx.Rows[0].Values["full_name"] == "Test Student",
        "P3: Excel OpenXML import parser");
}
Console.WriteLine("Phase 3 additional pure rule and import-reader checks passed.");

// Phase 4 — credit-hour workload arithmetic is kept independent from persistence.
var workload = new UA.Facade.Rules.Faculty.WorkloadCalculator();
Check(workload.Calculate(Array.Empty<decimal>()) == 0m, "P4: empty faculty assignment has zero workload");
Check(workload.Calculate([3.0m, 1.5m, 3.0m]) == 7.5m, "P4: multiple offerings sum their credits");
Check(UA.Framework.Constants.RoleNames.GuestFaculty != UA.Framework.Constants.RoleNames.Faculty,
    "P4: guest faculty has a dedicated restricted role");

// Phase 5 — pure finance rules and receipt formatting. Run with dotnet run.
var feeCalc = new UA.Facade.Rules.Finance.FeeCalculator();
var feeRates = new UA.Entities.FeeType[] {
    new() { FeeTypeId=1, Name="Tuition", EffectiveFrom=new DateTime(2026,1,1), Amount=1000 },
    new() { FeeTypeId=2, Name="Tuition", EffectiveFrom=new DateTime(2026,7,1), Amount=1200 } };
Check(feeCalc.Rate(feeRates,"Tuition",new DateTime(2026,6,30)).FeeTypeId==1, "P5: historical term rate");
Check(feeCalc.Rate(feeRates,"Tuition",new DateTime(2026,7,1)).FeeTypeId==2, "P5: effective date inclusive");
Check(feeCalc.Calculate(1000,15)==15000 && feeCalc.Calculate(1.005m,1)==1.01m, "P5: tuition calculation and half-up rounding");
bool missingFee=false; try { feeCalc.Rate(feeRates,"Registration",new DateTime(2026,7,1)); } catch(UA.Framework.Exceptions.BusinessRuleException) { missingFee=true; }
Check(missingFee,"P5: missing rate rejected");
var feeLate = new UA.Facade.Rules.Finance.LateFeePolicy();
Check(!feeLate.IsLate(new DateTime(2026,7,15,23,59,0),new DateTime(2026,7,15)) && feeLate.IsLate(new DateTime(2026,7,16),new DateTime(2026,7,15)), "P5: deadline inclusive");
Check(UA.Facade.Rules.Finance.DueGenerationRules.ShouldCreate("Active",Array.Empty<string>(),"Tuition") && !UA.Facade.Rules.Finance.DueGenerationRules.ShouldCreate("Active",new[]{"Tuition"},"Tuition") && !UA.Facade.Rules.Finance.DueGenerationRules.ShouldCreate("Dropped",Array.Empty<string>(),"Tuition"), "P5: logical due idempotency and status guard");
var payAlloc=new UA.Facade.Rules.Finance.PaymentAllocator();
var parts=payAlloc.Allocate(3500,new[]{(1,3000m),(2,15000m)});
Check(parts.Count==2 && parts[0]==(1,3000m) && parts[1]==(2,500m),"P5: oldest first partial allocation");
bool overpay=false; try {payAlloc.Allocate(3001,new[]{(1,3000m)});} catch(UA.Framework.Exceptions.BusinessRuleException){overpay=true;}
Check(overpay,"P5: overpayment refused");
var dueRow=new UA.Facade.DTO.Finance.DueRow(1,"26101120","Demo",1,1,1,1,"Tuition",15000,500,new DateTime(2026,7,15),"Partially paid");
Check(!UA.Facade.Rules.Finance.FeeClearanceRules.IsClear(new[]{dueRow}) && UA.Facade.Rules.Finance.FeeClearanceRules.IsClear(new[]{dueRow},2) && UA.Facade.Rules.Finance.FeeClearanceRules.IsClear(new[]{dueRow with {Paid=15000,Status="Paid"}}), "P5: term and full-ledger clearance");
var receiptNumber=new UA.Facade.Rules.Finance.ReceiptNumberGenerator().Generate(42,new DateTime(2026,7,15));
Check(receiptNumber=="UAS-2026-0000000042", "P5: receipt number");
var receiptBytes=new UA.Integration.Reporting.ReceiptPdfWriter().Write(new UA.Facade.DTO.Finance.ReceiptDto(receiptNumber,"26101120",new DateTime(2026,7,15),3500,new[]{"Due 1: BDT 3000.00","Due 2: BDT 500.00"}));
var receiptText=System.Text.Encoding.ASCII.GetString(receiptBytes);
Check(receiptText.StartsWith("%PDF-1.4") && receiptText.Contains(receiptNumber) && receiptText.EndsWith("%%EOF\n"),"P5: PDF receipt envelope");
Console.WriteLine("Phase 5 pure rule checks passed.");

var phase5TestConnection = Environment.GetEnvironmentVariable("UAS_PHASE5_TEST_DB");
if (!string.IsNullOrWhiteSpace(phase5TestConnection)) await Phase5IntegrationChecks.RunAsync(phase5TestConnection);

// Phase 6 — academic operations pure rules (execute with the existing console test harness).
Check(AcademicOperationRules.Semester(2026,2026,2)==2 && AcademicOperationRules.Semester(2026,2029,2)==8,"P6: calendar semester from batch admission year");
Check(AcademicOperationRules.Overlaps(TimeSpan.FromHours(9),TimeSpan.FromHours(10),TimeSpan.FromHours(9.5),TimeSpan.FromHours(11)),"P6: overlapping timetable slots");
Check(!AcademicOperationRules.Overlaps(TimeSpan.FromHours(9),TimeSpan.FromHours(10),TimeSpan.FromHours(10),TimeSpan.FromHours(11)),"P6: adjacent classes do not clash");
Check(AcademicOperationRules.Weekday(new DateTime(2026,11,23))==1 && AcademicOperationRules.Weekday(new DateTime(2026,11,29))==7,"P6: Monday=1 and Sunday=7");
Check(AcademicOperationRules.Percentage(3,4)==75m && AcademicOperationRules.Percentage(0,0)==null,"P6: absence of classes does not imply zero attendance");
Check(!AcademicOperationRules.IsShort(75m,75m) && AcademicOperationRules.IsShort(74.99m,75m) && !AcademicOperationRules.IsShort(null,75m),"P6: shortage threshold is lower-exclusive");
var attendanceConverter=new AttendanceMarkConverter();
(decimal,decimal)[] bands6=[(90,100),(85,80),(80,60),(75,40),(70,20),(0,0)];
Check(attendanceConverter.Convert(90,5,bands6)==5 && attendanceConverter.Convert(89.99m,5,bands6)==4 && attendanceConverter.Convert(75,5,bands6)==2 && attendanceConverter.Convert(69.99m,5,bands6)==0,"P6: attendance mark boundaries");
Check(AcademicOperationRules.TryJob(AcademicOperationRules.JobType(int.MaxValue,int.MaxValue),out var jobBatch6,out var jobTerm6) && jobBatch6==int.MaxValue && jobTerm6==int.MaxValue && AcademicOperationRules.JobType(int.MaxValue,int.MaxValue).Length<=30,"P6: registration job payload fits existing schema");
Check(!AcademicOperationRules.TryJob("Reg:bad:5",out _,out _) && !AcademicOperationRules.TryJob("Import",out _,out _),"P6: malformed or unrelated job not accepted");
Console.WriteLine("Phase 6 pure rule checks passed.");
var phase6TestConnection=Environment.GetEnvironmentVariable("UAS_PHASE6_TEST_DB");
if(!string.IsNullOrWhiteSpace(phase6TestConnection))await Phase6IntegrationChecks.RunAsync(phase6TestConnection);

// Phase 7: pure grading rules (run in the user's environment).
Check(UA.Facade.Rules.Examination.ExamRules.Round(39.995m)==40m,"P7: half-up rounding");
Check(!UA.Facade.Rules.Examination.ExamRules.InBand(40m,0m,40m),"P7: F upper boundary exclusive");
Check(UA.Facade.Rules.Examination.ExamRules.InBand(100m,80m,100m),"P7: A+ includes 100");
Check(UA.Facade.Rules.Examination.ExamRules.Gpa(new (decimal,decimal)[]{(3m,4m),(1m,0m)})==3m,"P7: credit-weighted GPA including failures");

Check(UA.Facade.Rules.Examination.ExamRules.Cgpa(new (int,int,int,decimal,decimal)[]{(1,1,1,3m,0m),(1,2,1,3m,3m),(2,1,1,3m,4m)})==3.50m,"P7: latest attempt replaces failure in CGPA");

// Phase 7 finalization: parallel scheduling / room reservation scenarios (not executed here).
var examSlot = new ExamSlot(new DateTime(2026, 12, 1), TimeSpan.FromHours(9), TimeSpan.FromHours(11));
ExamHall[] examHalls = [new(1, 2), new(2, 2)];
ExamDemand examA = new(1, examSlot, true, [1], [11], ["A", "B"], []);
ExamDemand examB = new(2, examSlot, true, [2], [22], ["C", "D"], []);
Check(ExamScheduleRules.TryPlan([examA, examB], examHalls, out var parallelPlan, out _) &&
    !parallelPlan[1].Overlaps(parallelPlan[2]), "P7: independent exams run simultaneously in distinct halls");
Check(!ExamScheduleRules.TryPlan([examA, examB with { BatchIds = [1] }], examHalls, out _, out _), "P7: same-batch clash rejected");
Check(!ExamScheduleRules.TryPlan([examA, examB with { SectionIds = [11] }], examHalls, out _, out _), "P7: same-section clash rejected");
Check(!ExamScheduleRules.TryPlan([examA, examB with { StudentIds = ["B", "C"] }], examHalls, out _, out _), "P7: common regular examinee clash rejected");
Check(!ExamScheduleRules.TryPlan([examA, examB], [new(1, 4)], out _, out _), "P7: a hall cannot be shared by concurrent courses even if total seats fit");
Check(!ExamScheduleRules.TryPlan([examA, examB], [new(1, 2), new(2, 1)], out _, out _), "P7: concurrent hall shortage rejected");
var laterExam = examB with { Slot = examSlot with { Start = TimeSpan.FromHours(11), End = TimeSpan.FromHours(13) }, BatchIds = [1] };
Check(ExamScheduleRules.TryPlan([examA, laterExam], [new(1, 2)], out var sequentialPlan, out _) &&
    sequentialPlan[1].SetEquals(sequentialPlan[2]), "P7: adjacent slots reuse rooms and cohorts");
Check(!ExamScheduleRules.TryPlan([examA, laterExam with { Slot = laterExam.Slot with { Start = TimeSpan.FromHours(10) } }], examHalls, out _, out _), "P7: partially overlapping manual edit rejected");
var fixedExam = examA with { FixedSeats = [new("A", 2, "1")] };
Check(ExamScheduleRules.TryPlan([fixedExam, examB], examHalls, out var fixedPlan, out _) && fixedPlan[1].Contains(2) &&
    fixedPlan[2].Contains(1), "P7: preserve an existing manual seat and reserve remaining hall");
Check(!ExamScheduleRules.TryPlan([fixedExam, examB with { FixedSeats = [new("C", 2, "2")] }], examHalls, out _, out _), "P7: different seats in the same concurrent hall still conflict");
Check(!ExamScheduleRules.TryPlan([examA with { FixedSeats = [new("A", 1, "1"), new("B", 1, "1")] }], examHalls, out _, out _), "P7: duplicate fixed seats rejected");
Check(!ExamScheduleRules.TryPlan([examA with { FixedSeats = [new("A", 1, "3")] }], examHalls, out _, out _), "P7: invalid fixed seat number rejected");
Check(ExamScheduleRules.TryPlan([examA with { StudentIds = ["A", "B", "C"] }], examHalls, out var splitPlan, out _) &&
    splitPlan[1].Count == 2, "P7: one course may span multiple halls");
Check(ExamScheduleRules.TryPlan([examA, examB with { Regular = false, BatchIds = [1], StudentIds = ["A", "C"] }], examHalls, out _, out _), "P7: no automatic regular/retake clash detection (BR-15)");

// Phase 8: lifecycle and manual retake clash behavior (source checks, run locally).
Check(UA.Facade.Rules.Student.LifecycleRules.Limit(2025)==new DateTime(2031,12,31),"P8: original admission seven-year limit");
Check(UA.Facade.Rules.Student.LifecycleRules.Semester(2027,2027,2)==2,"P8: rejoin junior batch at same semester number");
Check(UA.Facade.Rules.Student.LifecycleRules.CanRetake(0)&&UA.Facade.Rules.Student.LifecycleRules.CanRetake(1)&&!UA.Facade.Rules.Student.LifecycleRules.CanRetake(2),"P8: two retake maximum");
Check(!UA.Facade.Rules.Student.LifecycleRules.Passed("Absent","F",0)&&UA.Facade.Rules.Student.LifecycleRules.Passed("Normal","D",2)&&!UA.Facade.Rules.Student.LifecycleRules.Passed("Withheld","-",0),"P8: graduation pass conditions");
Check(ExamScheduleRules.TryPlan([examA,examB with { RetakeStudentIds=["A"] }],[new(1,2),new(2,3)],out _,out _),"P8: capacity reserves retake students without automatic retake clash detection");
Check(!ExamScheduleRules.TryPlan([examA,examB with { RetakeStudentIds=["A"] }],examHalls,out _,out _),"P8: retake capacity counted");

// Phase 8 finalization: department retake schemes and independent special sittings (not executed here).
var retakeDate8=new DateTime(2027,1,1);
UA.Entities.MarkComponent[] retakeParts8=[new(){CourseType="R:Lab",Name="Practical",MaxMarks=60,VersionNo=1,EffectiveFrom=retakeDate8},new(){CourseType="R:Lab",Name="Viva",MaxMarks=40,VersionNo=1,EffectiveFrom=retakeDate8}];
Check(RetakeSchemeRules.Resolve(retakeParts8,retakeDate8,"Lab").Length==2,"P8: configurable non-theory components total 100");
Check(RetakeSchemeRules.IsRetake("R:Internship")&&!RetakeSchemeRules.IsRetake("Lab")&&RetakeSchemeRules.Key("Theory")=="TheoryRetake","P8: retake component namespace isolated");
bool invalidScheme8=false;try{RetakeSchemeRules.Resolve(retakeParts8.Take(1),retakeDate8,"Lab");}catch(BusinessRuleException){invalidScheme8=true;}Check(invalidScheme8,"P8: invalid 60-mark scheme rejected");
invalidScheme8=false;retakeParts8[0].IsAttendanceBased=true;try{RetakeSchemeRules.Resolve(retakeParts8,retakeDate8,"Lab");}catch(BusinessRuleException){invalidScheme8=true;}Check(invalidScheme8,"P8: retake attendance components rejected");
var specialA8=examA with {BatchIds=[],SectionIds=[],StudentIds=[],RetakeStudentIds=["A"],Special=true};
var specialB8=examB with {BatchIds=[],SectionIds=[],StudentIds=[],RetakeStudentIds=["A"],Special=true};
Check(!ExamScheduleRules.TryPlan([specialA8,specialB8],examHalls,out _,out _),"P8: two special exams for same student cannot overlap");
Check(ExamScheduleRules.TryPlan([specialA8,specialB8 with {RetakeStudentIds=["B"]}],examHalls,out _,out _),"P8: independent special exams can run in separate halls");

// Phase 11: restricted public HTML rendering (source tests; execute locally).
Check(UA.Facade.Rules.Website.PublicContent.Render("<script>alert(1)</script>").Contains("&lt;script&gt;"),"P11: plain text escaped");
var safeCms11=UA.Facade.Rules.Website.PublicContent.Render("html:\n<p onclick=\"evil()\"><strong>Welcome</strong><script>evil()</script><a href=\"javascript:evil()\">link</a><img src=\"/cms-assets/logo.png\" onerror=\"evil()\" /></p>");
Check(safeCms11.Contains("<strong>Welcome</strong>")&&!safeCms11.Contains("onclick")&&!safeCms11.Contains("javascript:")&&!safeCms11.Contains("<script")&&!safeCms11.Contains("onerror"),"P11: allowlisted markup strips executable content");
Check(!UA.Facade.Rules.Website.PublicContent.LocalImage("/cms-assets/../private.jpg")&&UA.Facade.Rules.Website.PublicContent.LocalImage("/cms-assets/logo.png"),"P11: local public image path validation");
