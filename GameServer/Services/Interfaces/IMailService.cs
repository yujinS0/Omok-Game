using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IMailService
{
    Task<(ErrorCode, List<int>, List<string>, List<int>, List<DateTime>, List<long>, List<bool>)> GetPlayerMailBox(string playerId, int page, int pageSize);

}
