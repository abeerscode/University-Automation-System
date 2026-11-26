using UA.DataAccess.Interface.Academic;
using UA.DataAccess.Interface.Common;
using UA.Entities;
using UA.Facade.DTO.Academic;
using UA.Facade.Interface.Academic;
using UA.Facade.Interface.Administration;
using UA.Facade.Rules.Academic;
using UA.Framework.Exceptions;
using UA.Framework.Security;
using UA.Framework.Time;
namespace UA.Facade.Implementation.Academic;
public sealed class RoomFacade(IRoomRepository rooms, IUserAccessFacade access, IUnitOfWork work) : IRoomFacade
{
    private static RoomDto Map(Room x) => new(x.RoomId, x.Name, x.Capacity, x.RoomType);
    public async Task<IReadOnlyList<RoomDto>> ListAsync(CancellationToken ct = default)
    {
        await access.RequireAsync("rooms.manage", ct: ct);
        return (await rooms.ListAsync(ct: ct)).OrderBy(x => x.Name).Select(Map).ToArray();
    }
    public async Task<int> SaveAsync(RoomInput input, CancellationToken ct = default)
    {
        await access.RequireGlobalAsync("rooms.manage", ct);
        var name = input.Name.Trim();
        if (name.Length is < 1 or > 50 || input.Capacity is < 1 or > 1000 ||
            input.Type is not ("Classroom" or "Lab" or "Exam hall"))
            throw new BusinessRuleException("Enter room name (max 50), capacity and a supported type.");
        if (await rooms.FirstAsync(x => x.Name == name && x.RoomId != input.Id, ct) is not null)
            throw new BusinessRuleException("Room name already exists.");
        var old = input.Id == 0 ? null : await rooms.FirstAsync(x => x.RoomId == input.Id, ct) ?? throw new NotFoundException();
        var room = old ?? new Room(); room.Name = name; room.Capacity = input.Capacity; room.RoomType = input.Type;
        if (old is null) await rooms.AddAsync(room, ct);
        await work.SaveChangesAsync(ct); return room.RoomId;
    }
}
