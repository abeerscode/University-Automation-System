namespace UA.DataAccess.Interface.Common;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task ExecuteTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default);
}
