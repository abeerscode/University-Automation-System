using Microsoft.Extensions.DependencyInjection;
using UA.DataAccess;
using UA.Facade.Implementation.Administration;
using UA.Facade.Implementation.Website;
using UA.Facade.Interface.Administration;
using UA.Facade.Interface.Website;
using UA.Facade.Rules.Administration;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade;
public static class DependencyInjection
{
    public static IServiceCollection AddUniversityBusiness(this IServiceCollection services, string connectionString)
    {
        services.AddUniversityDataAccess(connectionString);
        services.AddSingleton<IClock, DhakaClock>();
        services.AddScoped<ICurrentUser, SystemCurrentUser>(); // Web replaces with HttpCurrentUser
        services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
        services.AddSingleton<ISelfApprovalGuard, SelfApprovalGuard>();
        services.AddScoped<IUserAccessFacade, UserAccessFacade>();
        services.AddScoped<IAuthFacade, AuthFacade>();
        services.AddScoped<IUserFacade, UserFacade>();
        services.AddScoped<IRoleFacade, RoleFacade>();
        services.AddScoped<IApprovalFacade, ApprovalFacade>();
        services.AddScoped<IAuditFacade, AuditFacade>();
        services.AddScoped<IConfigurationFacade, ConfigurationFacade>();
        services.AddScoped<INotificationFacade, NotificationFacade>();
        services.AddScoped<IBackgroundJobFacade, BackgroundJobFacade>();
        services.AddScoped<IImportFacade, ImportFacade>();
        services.AddScoped<IUploadedDocumentFacade, UploadedDocumentFacade>();
        services.AddScoped<INoticeFacade, NoticeFacade>();
        services.AddScoped<IUniversityBootstrapper, UniversityBootstrapper>();
        services.AddScoped<IPlatformOperationsFacade, PlatformOperationsFacade>();
        services.AddScoped<IManualAuditService, ManualAuditService>();
        services.AddScoped<IAcademicRulesFacade, AcademicRulesFacade>();
        return services;
    }
}
