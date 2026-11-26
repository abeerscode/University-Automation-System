using UA.DataAccess.Data;
using UA.DataAccess.Implementation.Common;
using UA.DataAccess.Interface.Academic;
using UA.Entities;
namespace UA.DataAccess.Implementation.Academic;
public sealed class RoomRepository(UniversityDbContext db) : Repository<Room>(db), IRoomRepository { }
