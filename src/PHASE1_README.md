# UAS — Administration & Security core (Phase 1)

This delivery starts from the supplied `src.zip` and follows the Phase 1 roadmap in
`UAS_Development_Blueprint.pdf`. The 61 existing entity classes, existing EF Core
migration and all original connection-string settings were preserved.

## Main implementation

- Layered dependencies: hosts → Facade/Integration/Framework; Facade → DataAccess → Entities.
- Database access: generic/unit-of-work repository base plus Administration aggregate repositories.
- Seed: departments 01–04, current academic term, roles, Appendix-B permission grants, first administrator,
  university configuration defaults, UGC grades, mark schemes, attendance bands, notification templates.
- Passwords: PBKDF2-SHA256 with random salts; password policy; login five failures/15-minute lock;
  30-minute sliding Secure/HttpOnly cookie; email reset/verification tokens stored as SHA-256 hashes.
- Per-request permission checks from the database, with department-specific authorization in facades.
- Users, roles and grants, approvals and corrections, config version history, audit browse + CSV export,
  grade/mark/attendance rule versions, notice drafting and independent publication approval.
- In-app inbox; SMTP email dispatcher worker with three retries at 5/15/60 minutes.
- Private file download with permission check, path confinement and access auditing.
- CSV template/validation preview, job records/progress views, API database health check.
- Antiforgery validation, login rate limiting, security headers, request error pages.

## How to start (Windows/Linux, .NET 10 SDK)

1. Verify **your existing database** has `20261111081233_InitialCreate` in `__EFMigrationsHistory`.
   Do **not** run the initial migration against tables that were created manually already.
2. Keep your original connection strings as supplied. Their bytes were not rewritten.
   For a deployment, prefer the environment variable `UNIVERSITY_DB` without modifying the files.
3. Set these environment variables **for initial setup only** (do not commit the values):
   `UAS_BOOTSTRAP_ADMIN_PASSWORD` (strong unique password),
   `UAS_BOOTSTRAP_ADMIN_USERNAME` (defaults to `sysadmin`),
   `UAS_BOOTSTRAP_ADMIN_EMAIL` (email for resets). The password is hashed, never stored in seed files.
4. Opt into **non-destructive initial seeding** via `Uas__SeedOnStartup=true` on the MVC host.
   Seeding assumes the database schema is already present. It does NOT migrate the database.
   Once it succeeds, turn this switch off and clear `UAS_BOOTSTRAP_ADMIN_PASSWORD`.
5. Configure SMTP using `Smtp__Host`, `Smtp__From`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`
   and canonical HTTPS reset URL `UAS_PUBLIC_BASE_URL=https://your-domain`.
   Without SMTP, reset emails/verification and outbound notices cannot be delivered.
6. Configure private storage outside `wwwroot` with `FileStorage__PrivateRoot`.
7. Run `dotnet restore UA.slnx`, `dotnet build UA.slnx`, then
   `dotnet run --project Presentation/UA.Web.UI` and
   `dotnet run --project API/UA.API` from the `src` folder.
8. Run the executable checks:
   `dotnet run --project Tests/UA.Facade.Tests`
   and `dotnet run --project Tests/UA.DataAccess.Tests`.
   Run real SQL Server transaction tests against a **separate** test database before production.

## What is intentionally incomplete / requires a decision

- **No live build or database test was possible in the packaging environment** (.NET SDK and SQL Server
  were unavailable). Run compilation, smoke tests, real SQL Server tests and security review before deployment.
- PDF generation, real Excel/XLSX parsing and writes, reliable bulk data import, and backup/restore
  are **not implemented**. CSV validation is a preview; student imports arrive with Phase 3.
- Background-job table and status views exist; generic payload dispatch, file-backed error reports,
  worker idempotency and full progress reporting need the DB-2 schema decision.
- Notifications use stored template text; personalization requires DB-3. SMTP must be configured.
  SMS and MFA remain deferred to Phase 12 per the roadmap.
- Approval numbers use a max+1 allocation inside a serializable SQL transaction; review
  distributed concurrency and unique-constraint retry behavior before high-concurrency use. Physical paper reference requires DB-6.
  A-13 self-approval validation is currently based on an entered requestor ID; the authoritative
  request-owner linkage must be enforced in each later workflow.
- The audit interceptor logs additions before SQL generates identity keys, so some created-row
  IDs appear as `0`/temporary. Use a post-insert correlation strategy for full production traceability.
- Role-permission changes take effect immediately; existing sessions are checked for active status
  but there is no full session revocation/versioning table.
- Account verification currently focuses on email; mobile verification needs the SMS provider.
- NFR-SEC-04 encryption at rest, DB-1 optimistic concurrency tokens, and DB-4 applicant phone
  require infrastructure/schema decisions. No schema migrations were created in this ZIP.
- Grade, mark, attendance and configuration edits append versions, but require institutional
  decisions about exact effective dates, version locking, and concurrency.
- Notification template changes are audited but schema has no template-version column.
- The first academic term's three deadlines are provisional values for setup and need review.
- Many lists are capped in memory for an initial staff-sized deployment; replace with SQL paging.
- This is development code, **not** a security-audited release or a claim of complete Phase 1.

See the supplied blueprint sections 1.7, 2.4, 4.13, 5, 6 and 7.2 for scope and decisions.
