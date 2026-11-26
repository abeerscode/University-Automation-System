using System.Globalization;
using System.Net.Mail;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Student;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Facade.Interface.Student;
using UA.Facade.Rules.Student;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Student;
/// <summary>Preflight before any database write; commit revalidates in a serializable transaction.</summary>
public sealed class StudentImportFacade(IStudentImportReader reader, IRepository<Department> departments,
    IRepository<Batch> batches, IRepository<Section> sections, IRepository<UA.Entities.Student> students,
    IRepository<UserAccount> accounts, IUserAccessFacade access, ICurrentUser current,
    IStudentIdGenerator generator, IStudentRecordCreator creator, IUnitOfWork work, IClock clock) : IStudentImportFacade
{
    private static readonly string[] RequiredHeaders = ["department_code", "admission_year", "roll_no", "section",
        "full_name", "date_of_birth", "gender", "guardian_name", "guardian_phone", "phone", "address"];
    private sealed record Plan(IReadOnlyList<StudentImportRow> Rows, IReadOnlyList<StudentImportIssue> Issues);

    public async Task<StudentImportPreview> PreviewAsync(byte[] data, string filename, CancellationToken ct = default)
    {
        await access.RequireGlobalAsync("imports.run", ct);
        var plan = await ValidateAsync(data, filename, ct);
        return new StudentImportPreview(plan.Rows.Count, plan.Issues);
    }
    public async Task<StudentImportResult> CommitAsync(byte[] data, string filename, CancellationToken ct = default)
    {
        await access.RequireGlobalAsync("imports.run", ct);
        var credentials = new List<StudentCreatedCredential>();
        await work.ExecuteTransactionAsync(async token =>
        {
            // Revalidate against database state under SERIALIZABLE isolation; never trust a stale preview.
            var plan = await ValidateAsync(data, filename, token);
            if (plan.Issues.Count > 0) throw new BusinessRuleException("Import validation failed. Preview the file again; nothing was saved. " +
                string.Join("; ", plan.Issues.Take(3).Select(x => $"Row {x.Line}: {x.Message}")));
            if (current.UserId is not int actor) throw new ForbiddenException();
            foreach (var row in plan.Rows)
                credentials.Add(await creator.AddAsync(row.Data, actor, token));
        }, ct);
        return new StudentImportResult(credentials.Count, credentials);
    }
    private async Task<Plan> ValidateAsync(byte[] data, string filename, CancellationToken ct)
    {
        var sheet = reader.Read(data, Path.GetExtension(filename));
        var issues = new List<StudentImportIssue>();
        var actualHeaders = sheet.Headers.Select(x => x.Trim()).ToArray();
        foreach (var h in RequiredHeaders.Except(actualHeaders, StringComparer.OrdinalIgnoreCase))
            issues.Add(new StudentImportIssue(1, "Missing column: " + h));
        foreach (var group in actualHeaders.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1 || string.IsNullOrEmpty(g.Key)))
            issues.Add(new StudentImportIssue(1, "Duplicate or empty header: " + group.Key));
        if (issues.Count > 0) return new Plan([], issues);
        if (sheet.Rows.Count == 0) return new Plan([], [new StudentImportIssue(1, "No student rows found.")]);
        var depts = await departments.ListAsync(ct: ct);
        var batchesList = await batches.ListAsync(ct: ct);
        var sectionsList = await sections.ListAsync(ct: ct);
        var knownStudents = await students.ListAsync(ct: ct);
        var knownAccounts = await accounts.ListAsync(ct: ct);
        var existingIds = knownStudents.Select(s => s.StudentId).ToHashSet(StringComparer.Ordinal);
        var existingLogins = knownAccounts.Select(s => s.Username).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingEmails = knownAccounts.Where(x => !string.IsNullOrWhiteSpace(x.Email))
            .Select(x => x.Email!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var idsInFile = new HashSet<string>(StringComparer.Ordinal);
        var emailsInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var newSectionCounts = new Dictionary<int,int>();
        var newBatchCounts = new Dictionary<int,int>();
        var valid = new List<StudentImportRow>();
        foreach (var row in sheet.Rows)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                if (row.Values.TryGetValue("__error", out var err)) throw new BusinessRuleException(err);
                string Get(string key) => row.Values.TryGetValue(key, out var v) ? v.Trim() : "";
                var dept = depts.FirstOrDefault(d => d.Code.Equals(Get("department_code"), StringComparison.OrdinalIgnoreCase) && d.Status == "Active")
                    ?? throw new BusinessRuleException("Unknown or inactive department code.");
                if (!short.TryParse(Get("admission_year"), out short year) || year is < 2000 or > 2099)
                    throw new BusinessRuleException("Admission year must be between 2000 and 2099.");
                var batch = batchesList.FirstOrDefault(b => b.DepartmentId == dept.DepartmentId && b.AdmissionYear == year)
                    ?? throw new BusinessRuleException("Create this department/year batch in Academic Management first.");
                if (!short.TryParse(Get("roll_no"), out short roll) || roll < 1 || roll > batch.SeatLimit)
                    throw new BusinessRuleException("Roll must fit the configured admission seat limit.");
                var byBatch = sectionsList.Where(x => x.BatchId == batch.BatchId).OrderBy(x => x.Name).ToArray();
                int cumulative = 0; Section? expected = null;
                foreach (var s in byBatch)
                {
                    cumulative += s.Capacity;
                    if (roll <= cumulative) { expected = s; break; }
                }
                if (expected is null || !expected.Name.Equals(Get("section"), StringComparison.OrdinalIgnoreCase))
                    throw new BusinessRuleException("Section must match the merit-roll allocation of this batch (BR-03).");
                int existingBatch = knownStudents.Count(x => x.AdmissionBatchId == batch.BatchId);
                int existingSection = knownStudents.Count(x => x.SectionId == expected.SectionId && x.Status == "Active");
                newBatchCounts.TryGetValue(batch.BatchId, out int batchAdded);
                newSectionCounts.TryGetValue(expected.SectionId, out int sectionAdded);
                if (existingBatch + batchAdded >= batch.SeatLimit || existingSection + sectionAdded >= expected.Capacity)
                    throw new BusinessRuleException("Batch or section has reached its initial-import capacity.");
                var studentId = generator.Generate(year, dept.IdCode, roll);
                if (!idsInFile.Add(studentId) || existingIds.Contains(studentId) || existingLogins.Contains(studentId))
                    throw new BusinessRuleException("Duplicate student ID, roll, or account: " + studentId);
                DateTime birthday = ParseDate(Get("date_of_birth"));
                var email = Get("email");
                if (!string.IsNullOrWhiteSpace(email))
                {
                    if (email.Length > 100 || !MailAddress.TryCreate(email, out var address) || address.Address != email)
                        throw new BusinessRuleException("Invalid email address.");
                    if (!emailsInFile.Add(email) || existingEmails.Contains(email))
                        throw new BusinessRuleException("Duplicate email address.");
                }
                StudentValidation.Ensure(Get("full_name"), birthday, Get("gender"),
                    Get("guardian_name"), Get("guardian_phone"), Get("phone"), Get("address"), clock.Today);
                var input = new StudentRegistrationInput(null, dept.DepartmentId, batch.BatchId, expected.SectionId,
                    roll, Get("full_name"), birthday, Get("gender"), Get("guardian_name"),
                    Get("guardian_phone"), Get("phone"), Get("address"), string.IsNullOrEmpty(email) ? null : email);
                valid.Add(new StudentImportRow(row.Line, input));
                newSectionCounts[expected.SectionId] = sectionAdded + 1;
                newBatchCounts[batch.BatchId] = batchAdded + 1;
            }
            catch (BusinessRuleException ex) { issues.Add(new StudentImportIssue(row.Line, ex.Message)); }
        }
        return new Plan(valid, issues);
    }
    private static DateTime ParseDate(string value)
    {
        if (DateTime.TryParseExact(value, ["yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd"], CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed)) return parsed.Date;
        if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double serial) && serial is > 0 and < 100000)
        { try { return DateTime.FromOADate(serial).Date; } catch (ArgumentException) { } }
        throw new BusinessRuleException("Date of birth must be yyyy-MM-dd (Excel dates also supported).");
    }
}
