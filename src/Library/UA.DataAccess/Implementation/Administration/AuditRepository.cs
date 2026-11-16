using UA.DataAccess.Data;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Interface.Administration;
using UA.Entities;
namespace UA.DataAccess.Implementation.Administration;
public sealed class AuditRepository(UniversityDbContext db) : Repository<AuditEntry>(db), IAuditRepository { }
