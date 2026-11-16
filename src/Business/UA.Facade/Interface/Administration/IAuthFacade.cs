using UA.Facade.DTO.Administration;
using UA.Framework.Common;
namespace UA.Facade.Interface.Administration;
public interface IAuthFacade
{
    Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct = default);
    Task RequestResetAsync(string username, string origin, CancellationToken ct = default);
    Task<OperationResult> ResetPasswordAsync(string username, string token, string password, CancellationToken ct = default);
    Task<OperationResult> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken ct = default);
    Task RequestVerificationAsync(int userId, CancellationToken ct = default);
    Task<OperationResult> VerifyCodeAsync(int userId, string code, CancellationToken ct = default);
}
