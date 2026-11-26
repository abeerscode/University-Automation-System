using UA.DataAccess.Data;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Interface.Academic;
using UA.Entities;
namespace UA.DataAccess.Implementation.Academic;
public sealed class CurriculumRepository(UniversityDbContext db) : Repository<CurriculumVersion>(db), ICurriculumRepository { }
