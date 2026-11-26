using UA.DataAccess.Implementation.Faculty;
using UA.DataAccess.Interface.Faculty;
using UA.DataAccess.Implementation.Student;
using UA.DataAccess.Interface.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UA.DataAccess.Data;
using UA.DataAccess.Data.Interceptors;
using UA.DataAccess.Implementation.Administration;
using UA.DataAccess.Implementation.Academic;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Implementation.Website;
using UA.DataAccess.Interface.Administration;
using UA.DataAccess.Interface.Academic;
using UA.DataAccess.Interface.Common;
using UA.DataAccess.Interface.Website;
using UA.Entities;
namespace UA.DataAccess;
public static class DependencyInjection
{
    public static IServiceCollection AddUniversityDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddSingleton<ImmutableRecordInterceptor>();
        services.AddDbContext<UniversityDbContext>((sp, options) => options.UseSqlServer(connectionString)
            .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>(), sp.GetRequiredService<ImmutableRecordInterceptor>()));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
        services.AddScoped<IApprovalRecordRepository, ApprovalRecordRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IConfigParameterRepository, ConfigParameterRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
        services.AddScoped<IUploadedDocumentRepository, UploadedDocumentRepository>();
        services.AddScoped<INoticeRepository, NoticeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ICurriculumRepository, CurriculumRepository>();
        services.AddScoped<IAcademicTermRepository, AcademicTermRepository>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<ITeacherAssignmentRepository, TeacherAssignmentRepository>();
        services.AddScoped<IMentorAssignmentRepository, MentorAssignmentRepository>();
        services.AddScoped<UA.DataAccess.Interface.Finance.IFeeTypeRepository, UA.DataAccess.Implementation.Finance.FeeTypeRepository>();
        services.AddScoped<UA.DataAccess.Interface.Finance.IDueRepository, UA.DataAccess.Implementation.Finance.DueRepository>();
        services.AddScoped<UA.DataAccess.Interface.Finance.IPaymentRepository, UA.DataAccess.Implementation.Finance.PaymentRepository>();
        return services;
    }
}
