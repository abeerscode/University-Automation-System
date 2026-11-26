using Microsoft.EntityFrameworkCore;
using UA.DataAccess.Data;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Interface.Finance;
using UA.Entities;
namespace UA.DataAccess.Implementation.Finance;
public sealed class PaymentRepository(UniversityDbContext db) : Repository<Payment>(db), IPaymentRepository
{
    public async Task<IReadOnlyList<PaymentProjection>> HistoryAsync(string studentId,CancellationToken ct=default) => await db.Payments.AsNoTracking().Where(p=>p.StudentId==studentId && p.Status=="Successful").OrderByDescending(p=>p.PaidAt).ThenByDescending(p=>p.PaymentId).Select(p=>new PaymentProjection(p.PaymentId,p.Amount,p.PaidAt,p.Method,p.Receipt==null?"":p.Receipt.ReceiptNo)).ToListAsync(ct);
}
