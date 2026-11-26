using UA.DataAccess.Interface.Common;
using UA.Entities;
namespace UA.DataAccess.Interface.Finance;
public record PaymentProjection(int Id,decimal Amount,DateTime? PaidAt,string Method,string ReceiptNo);
public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<PaymentProjection>> HistoryAsync(string studentId,CancellationToken ct=default);
}
