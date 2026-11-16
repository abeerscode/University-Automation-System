using Microsoft.Extensions.DependencyInjection;
using UA.Facade.Interface.Integration;
using UA.Integration.Security;
using UA.Integration.Messaging;
using UA.Integration.Storage;
using UA.Integration.Reporting;
namespace UA.Integration;
public static class DependencyInjection
{
    public static IServiceCollection AddUniversityIntegration(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IExcelWriter, CsvWriter>();
        return services;
    }
}
