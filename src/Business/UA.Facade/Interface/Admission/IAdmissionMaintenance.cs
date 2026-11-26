namespace UA.Facade.Interface.Admission;
public interface IAdmissionMaintenance { Task RemindAsync(CancellationToken ct); Task AnonymizeAsync(CancellationToken ct); }
