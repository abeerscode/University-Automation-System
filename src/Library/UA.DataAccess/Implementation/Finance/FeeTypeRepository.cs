using UA.DataAccess.Data;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Interface.Finance;
using UA.Entities;
namespace UA.DataAccess.Implementation.Finance;
public sealed class FeeTypeRepository(UniversityDbContext db) : Repository<FeeType>(db), IFeeTypeRepository { }
