Phase 1 verification cases (automated test harness pending):
- Reject short/common password; accept 10+ uppercase/lowercase/digit.
- Wrong password 5 times locks for 15 minutes; correct password resets counter.
- Reset link expires after 30 minutes, can be used only once.
- Verification code expires after 10 minutes and permits no more than 5 attempts.
- Department-scoped role cannot authorize data from another department.
- No unprivileged login can access any /Administration route.
- A notice author cannot approve their own notice.
- Correcting an approval creates a new row, never overwrites the original.
- Audit_Entry edit/delete is rejected; a password or reset token never appears in audits.
- CSV import rejects missing headers and malformed rows.
- Seed is idempotent on existing DB. Existing passwords and connections never change.

## Phase 3 additional integration checks (requires .NET 10 SDK and a disposable SQL Server)

- Seed the ordinary roles, four departments, a current term, curriculum, 2027 batches and sections using the existing configuration. Confirm no original connection string was rewritten.
- As an administrator, upload the `students-import-template.csv` with one valid row; preview must contain zero errors and the database must remain unchanged until Commit. Commit should atomically insert User_Account, User_Role, Student, Student_Status_History and Student_Batch_History. Credentials must be delivered only once as CSV.
- Re-upload same rows: preview reports duplicate student IDs and Commit saves no records. Upload mixed valid/invalid rows: zero records are saved. Verify .xlsx input with shared strings and native Excel date values.
- Log in with the generated student ID and temporary password. The portal profile is limited to the student's account. Attempt a second student's profile by guessing an ID; no endpoint should accept another student's ID.
- Upload a valid PNG/JPEG <=1 MB, verify it is in FileStorage:PrivateRoot and the new uploaded-document record exists. Reject malformed MIME/magic and >1 MB. Verify phone/address can be edited; identity/status/guardian cannot.
- As a department coordinator, move an Active student to a different section in the same batch with a Department Head-issued, approved SectionChange reference. Confirm the previous open Student_Batch_History entry is closed, the new entry is open and the student section changes in one transaction. Reject full section beyond configurable overflow and another department's students.
- As Registrar, record an approved StudentStatus reference, change Active to Suspended, then confirm a new Student_Status_History entry, correct changed_by and audit trail. Reject a self-approval, reused reference, cross-department reference and workflow-only statuses such as Dropped/Graduated.
- Confirm Phase 1 login/roles/audit/notifications and Phase 2 department/course/curriculum/calendar/batch/room actions still work. Run `dotnet build UA.slnx` and `dotnet run --project Tests/UA.Facade.Tests`.
- O-5: prior results for imported historical/mid-program cohorts remain unresolved until a manual historical import policy is agreed; do not imply valid CGPA/graduation from student-only records.
