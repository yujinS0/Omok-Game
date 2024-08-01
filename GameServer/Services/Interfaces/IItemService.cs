using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IItemService
{
    Task<(ErrorCode, List<long>, List<int>, List<int>)> GetPlayerItems(Int64 playerUid, int itemPageNum);
}
