using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IItemService
{
    Task<(ErrorCode, List<long>, List<int>, List<int>)> GetPlayerItem(string playerId, int itemPageNum);
}
