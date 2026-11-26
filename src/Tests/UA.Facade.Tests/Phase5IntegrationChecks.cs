using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UA.DataAccess.Data;
using UA.DataAccess.Data.Seed;
using UA.Facade;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Finance;
using UA.Facade.Interface.Integration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
using UA.Integration;

// Opt-in only: creates a fresh, explicitly named SQL Server test database; never uses appsettings.
internal static class Phase5IntegrationChecks
{
    private static void Check(bool success,string name)
    { if(!success) throw new Exception("FAILED: "+name); Console.WriteLine("PASS "+name); }
    public static async Task RunAsync(string connection)
    {
        var cs=new SqlConnectionStringBuilder(connection);
        if(!cs.InitialCatalog.StartsWith("UAS_Phase5_Test_",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Use a new database named UAS_Phase5_Test_<unique suffix>.");
        var services=new ServiceCollection(); services.AddSingleton<IConfiguration>(new EmptyConfiguration()); services.AddLogging(); services.AddUniversityBusiness(connection); services.AddUniversityIntegration();
        services.AddScoped<TestActor>(_=>new TestActor()); services.AddScoped<ICurrentUser>(p=>p.GetRequiredService<TestActor>());
        using var provider=services.BuildServiceProvider(new ServiceProviderOptions{ValidateScopes=true,ValidateOnBuild=true});
        await using var scope=provider.CreateAsyncScope(); var sp=scope.ServiceProvider;
        var db=sp.GetRequiredService<UniversityDbContext>();
        if(!await db.Database.EnsureCreatedAsync()) throw new InvalidOperationException("Test database is not fresh. Choose another unique database name. No database was deleted.");
        var clock=sp.GetRequiredService<IClock>();var hasher=sp.GetRequiredService<IPasswordHasher>();var actor=sp.GetRequiredService<TestActor>();
        async Task Seed(){await DbInitializer.SeedAsync(db,clock,hasher.Hash,"phase5.test.admin","UasTest!2026Admin",null);await Phase5DemoSeeder.SeedAsync(db,clock,hasher.Hash);}
        async Task<int[]> Counts()=>[await db.UserAccounts.CountAsync(),await db.UserRoles.CountAsync(),await db.FeeTypes.CountAsync(),await db.Students.CountAsync(),await db.Faculties.CountAsync(),await db.Dues.CountAsync(),await db.Payments.CountAsync(),await db.PaymentAllocations.CountAsync(),await db.Receipts.CountAsync(),await db.CurriculumCourses.CountAsync(),await db.MentorAssignments.CountAsync(),await db.Notices.CountAsync(),await db.Notifications.CountAsync(),await db.NotificationTemplates.CountAsync()];
        await Seed();var before=await Counts();await Seed();Check(before.SequenceEqual(await Counts()),"P5 SQL: repeated seed does not duplicate records");
        var auth=sp.GetRequiredService<IAuthFacade>();var access=sp.GetRequiredService<IUserAccessFacade>();
        foreach(var account in new (string User,string Password,string Role)[]{
            ("phase5.test.admin","UasTest!2026Admin",UA.Framework.Constants.RoleNames.SystemAdministrator),
            ("demo.registrar","UasReg!2026Demo",UA.Framework.Constants.RoleNames.Registrar),
            ("demo.head.cse","UasHead!2026Demo",UA.Framework.Constants.RoleNames.Head),
            ("demo.coord.cse","UasCoord!2026Demo",UA.Framework.Constants.RoleNames.Coordinator),
            ("demo.accounts","UasFin!2026Demo",UA.Framework.Constants.RoleNames.Accountant),
            ("demo.notice","UasNotice!2026Demo",UA.Framework.Constants.RoleNames.WebsiteEditor),
            ("DEMO-FAC-01","UasFac!2026Demo",UA.Framework.Constants.RoleNames.Faculty),
            ("DEMO-GUEST-01","UasGuest!2026Demo",UA.Framework.Constants.RoleNames.GuestFaculty),
            ("26101120","UasStu!2026Demo",UA.Framework.Constants.RoleNames.Student)})
        {
            var login=await auth.LoginAsync(account.User,account.Password);
            var accountRow=await db.UserAccounts.SingleAsync(u=>u.Username==account.User);
            Check(login.Succeeded && accountRow.LastLoginAt.HasValue && (await access.ForUserAsync(accountRow.UserId)).Assignments.Any(a=>a.RoleName==account.Role),"P5 SQL: login and role "+account.User);
        }
        actor.UserId=(await db.UserAccounts.SingleAsync(u=>u.Username=="demo.accounts")).UserId;
        var ledger=sp.GetRequiredService<ILedgerFacade>();var dues=sp.GetRequiredService<IDueFacade>();var pay=sp.GetRequiredService<IPaymentFacade>();var fees=sp.GetRequiredService<IFeeTypeFacade>();var clear=sp.GetRequiredService<IFeeClearanceChecker>();
        const string sid="26101120";var term=await db.AcademicTerms.SingleAsync(t=>t.Year==2026 && t.TermNo==2);var student=await db.Students.SingleAsync(s=>s.StudentId==sid);
        var initial=await ledger.StudentAsync(sid);Check(initial.Dues.Count==2 && initial.Payments.Count==1 && initial.Balance==15000,"P5 SQL: seeded ledger and receipt");
        await fees.AddRateAsync(student.DepartmentId,"Tuition",1200,new DateTime(2026,7,1));await dues.GenerateAsync(sid,term.TermId);await dues.GenerateAsync(sid,term.TermId);
        Check((await ledger.StudentAsync(sid)).Dues.Count==2 && (await ledger.StudentAsync(sid)).Balance==15000,"P5 SQL: existing dues unchanged after new rate and repeated generation");
        Check((await dues.DuesAsync(student.DepartmentId,student.CurrentBatchId,student.SectionId,"Unpaid")).Count==1,"P5 SQL: combined due filters");
        var paymentId=await pay.CounterAsync(sid,500,null);var after=await ledger.StudentAsync(sid);
        Check(after.Balance==14500 && after.Dues.Any(d=>d.Status=="Partially paid") && after.Payments.Count==2,"P5 SQL: partial counter settlement");
        var receipt=await pay.ReceiptAsync(paymentId,false);Check(receipt.Amount==500 && receipt.Allocations.Count==1,"P5 SQL: receipt and allocation");
        bool denied=false;try{await pay.CounterAsync(sid,14501,null);}catch(BusinessRuleException){denied=true;}Check(denied && (await ledger.StudentAsync(sid)).Balance==14500,"P5 SQL: overpayment rejected without ledger mutation");
        Check(!await clear.IsClearAsync(sid,term.TermId),"P5 SQL: outstanding balance fails clearance");
        await pay.CounterAsync(sid,14500,null);Check(await clear.IsClearAsync(sid,term.TermId),"P5 SQL: full settlement clears dues");
        actor.UserId=student.UserId;Check((await ledger.OwnAsync()).Balance==0 && (await pay.ReceiptAsync(paymentId,true)).Number==receipt.Number,"P5 SQL: student self-service and receipt");
        denied=false;try{await pay.CounterAsync(sid,1,null);}catch(ForbiddenException){denied=true;}Check(denied,"P5 SQL: student cannot record payment");
        actor.UserId=(await db.UserAccounts.SingleAsync(u=>u.Username=="DEMO-FAC-01")).UserId;denied=false;try{await ledger.OwnAsync();}catch(ForbiddenException){denied=true;}Check(denied,"P5 SQL: nonstudent cannot access student portal");
        Console.WriteLine("Phase 5 SQL integration checks completed. Test database retained for inspection: "+cs.InitialCatalog);
    }
    private sealed class EmptyConfiguration(string key="") : IConfigurationSection
    {
        public string Key=>key; public string Path=>key; public string? Value {get;set;}
        public string? this[string name] {get=>null;set{}}
        public IEnumerable<IConfigurationSection> GetChildren()=>Array.Empty<IConfigurationSection>();
        public IConfigurationSection GetSection(string name)=>new EmptyConfiguration(name);
        public Microsoft.Extensions.Primitives.IChangeToken GetReloadToken()=>new Microsoft.Extensions.Primitives.CancellationChangeToken(CancellationToken.None);
    }
    private sealed class TestActor : ICurrentUser { public int? UserId {get;set;} public bool IsAuthenticated=>UserId.HasValue; }
}
