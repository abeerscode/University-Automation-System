using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UA.DataAccess.Data;
using UA.DataAccess.Data.Seed;
using UA.Facade;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Integration;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
using UA.Integration;

// Opt-in test database only; never reads or mutates appsettings or deletes a database.
internal static class Phase6IntegrationChecks
{
    private static void Check(bool ok,string label){if(!ok)throw new Exception("FAILED: "+label);Console.WriteLine("PASS "+label);}
    public static async Task RunAsync(string connection)
    {
        var cs=new SqlConnectionStringBuilder(connection);
        if(!cs.InitialCatalog.StartsWith("UAS_Phase6_Test_",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Use a new database named UAS_Phase6_Test_<unique suffix>.");
        var services=new ServiceCollection();services.AddSingleton<IConfiguration>(new EmptyConfiguration());services.AddLogging();services.AddUniversityBusiness(connection);services.AddUniversityIntegration();
        services.AddSingleton<IClock>(_=>new FixedClock());services.AddScoped<Actor>(_=>new Actor());services.AddScoped<ICurrentUser>(p=>p.GetRequiredService<Actor>());
        using var provider=services.BuildServiceProvider(new ServiceProviderOptions{ValidateOnBuild=true,ValidateScopes=true});
        await using var scope=provider.CreateAsyncScope();var sp=scope.ServiceProvider;var db=sp.GetRequiredService<UniversityDbContext>();
        if(!await db.Database.EnsureCreatedAsync())throw new InvalidOperationException("Test database must be new. No data was deleted.");
        var clock=sp.GetRequiredService<IClock>();var hasher=sp.GetRequiredService<IPasswordHasher>();var actor=sp.GetRequiredService<Actor>();
        async Task Seed(){await DbInitializer.SeedAsync(db,clock,hasher.Hash,"phase6.test.admin","UasTest!2026Admin",null);await Phase5DemoSeeder.SeedAsync(db,clock,hasher.Hash);}
        await Seed();var permissionCount=await db.RolePermissions.CountAsync();var accountCount=await db.UserAccounts.CountAsync();await Seed();Check(permissionCount==await db.RolePermissions.CountAsync()&&accountCount==await db.UserAccounts.CountAsync(),"P6 SQL: seed permissions/accounts are idempotent");
        var auth=sp.GetRequiredService<IAuthFacade>();Check((await auth.LoginAsync("demo.coord.cse","UasCoord!2026Demo")).Succeeded,"P6 SQL: existing coordinator login");
        actor.UserId=(await db.UserAccounts.SingleAsync(u=>u.Username=="demo.coord.cse")).UserId;
        var operations=sp.GetRequiredService<IAcademicOperationsFacade>();var processor=sp.GetRequiredService<IRegistrationJobProcessor>();
        var student=await db.Students.SingleAsync(s=>s.StudentId=="26101120");var batch=student.CurrentBatchId;var term=(await db.AcademicTerms.SingleAsync(t=>t.Year==2026&&t.TermNo==2)).TermId;
        await operations.GenerateAsync(batch,term);var count=await db.Offerings.CountAsync();await operations.GenerateAsync(batch,term);Check(count>0&&count==await db.Offerings.CountAsync(),"P6 SQL: offering generation is idempotent");
        var dueCount=await db.Dues.CountAsync();var job=await operations.QueueRegistrationAsync(batch,term);await processor.ProcessAsync(job);Check((await operations.JobAsync(job)).Status=="Completed","P6 SQL: registration completed");
        var enrollCount=await db.Enrollments.CountAsync();Check(enrollCount>0&&await db.Dues.CountAsync()==dueCount,"P6 SQL: existing semester dues preserved");
        job=await operations.QueueRegistrationAsync(batch,term);await processor.ProcessAsync(job);Check(await db.Enrollments.CountAsync()==enrollCount&&await db.Dues.CountAsync()==dueCount,"P6 SQL: repeated registration does not duplicate enrollment or dues");
        await operations.ConfirmAsync(batch,term);Check(await db.Enrollments.AllAsync(e=>e.Status=="Confirmed"),"P6 SQL: bulk confirmation");
        var offering=await db.Offerings.FirstAsync(o=>o.SectionId==student.SectionId);var teacher=await db.Faculties.SingleAsync(f=>f.EmployeeRef=="DEMO-FAC-01");var room=await db.Rooms.SingleAsync(r=>r.Name=="DEMO-CSE-101");
        await operations.AssignAsync(offering.OfferingId,teacher.FacultyId,false);await operations.SaveRoutineAsync(offering.OfferingId,0,room.RoomId,5,TimeSpan.FromHours(9),TimeSpan.FromHours(10));
        var other=await db.Offerings.FirstAsync(o=>o.SectionId==student.SectionId&&o.OfferingId!=offering.OfferingId);await operations.AssignAsync(other.OfferingId,teacher.FacultyId,false);
        bool denied=false;try{await operations.SaveRoutineAsync(other.OfferingId,0,room.RoomId,5,TimeSpan.FromHours(9),TimeSpan.FromHours(10));}catch(BusinessRuleException){denied=true;}Check(denied,"P6 SQL: university routine clash refused");
        actor.UserId=teacher.UserId;var detail=await operations.DetailAsync(offering.OfferingId,true);var ids=detail.Students.Where(e=>e.Status=="Confirmed").Select(e=>e.Id).ToArray();var slot=detail.Routine.Single().Id;
        await operations.RecordAttendanceAsync(offering.OfferingId,slot,clock.Today,ids,ids);Check((await operations.DetailAsync(offering.OfferingId,true)).Students.All(e=>e.Percent==100),"P6 SQL: present attendance and percentage");
        await operations.RecordAttendanceAsync(offering.OfferingId,slot,clock.Today,[],ids);Check((await operations.DetailAsync(offering.OfferingId,true)).Students.All(e=>e.Percent==0&&e.Shortage),"P6 SQL: correction and shortage");
        Check(await db.AttendanceRecords.CountAsync()==ids.Length,"P6 SQL: correction does not duplicate attendance");
        var unassigned=await db.Offerings.FirstAsync(o=>o.OfferingId!=offering.OfferingId&&o.OfferingId!=other.OfferingId);denied=false;try{await operations.DetailAsync(unassigned.OfferingId,true);}catch(ForbiddenException){denied=true;}Check(denied,"P6 SQL: unassigned faculty forbidden");
        actor.UserId=student.UserId;Check((await operations.OwnCoursesAsync()).Count>0,"P6 SQL: student sees own courses");denied=false;try{await operations.DetailAsync(offering.OfferingId,true);}catch(ForbiddenException){denied=true;}Check(denied,"P6 SQL: student cannot open faculty attendance");
        Console.WriteLine("Phase 6 integration checks completed; database retained: "+cs.InitialCatalog);
    }
    private sealed class EmptyConfiguration(string key="") : IConfigurationSection
    {
        public string Key=>key;public string Path=>key;public string? Value{get;set;}
        public string? this[string name]{get=>null;set{}}
        public IEnumerable<IConfigurationSection> GetChildren()=>Array.Empty<IConfigurationSection>();
        public IConfigurationSection GetSection(string name)=>new EmptyConfiguration(name);
        public Microsoft.Extensions.Primitives.IChangeToken GetReloadToken()=>new Microsoft.Extensions.Primitives.CancellationChangeToken(CancellationToken.None);
    }
    private sealed class FixedClock : IClock{public DateTime Today=>new(2026,7,10);public DateTime Now=>Today.AddHours(12);}
    private sealed class Actor : ICurrentUser{public int? UserId{get;set;}public bool IsAuthenticated=>UserId.HasValue;}
}
