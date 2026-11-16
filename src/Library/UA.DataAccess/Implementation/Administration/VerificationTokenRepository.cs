using UA.DataAccess.Data;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Interface.Administration;
using UA.Entities;
namespace UA.DataAccess.Implementation.Administration;
public sealed class VerificationTokenRepository(UniversityDbContext db) : Repository<VerificationToken>(db), IVerificationTokenRepository { }
