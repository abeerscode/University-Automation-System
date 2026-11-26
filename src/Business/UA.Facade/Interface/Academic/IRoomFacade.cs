using UA.Facade.DTO.Academic;
namespace UA.Facade.Interface.Academic;
public interface IRoomFacade
{
Task<IReadOnlyList<RoomDto>> ListAsync(CancellationToken ct = default);
Task<int> SaveAsync(RoomInput input, CancellationToken ct = default);
}
