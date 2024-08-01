using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Repository.Interfaces;

public interface IGameDb : IDisposable
{
    Task<PlayerInfo> CreatePlayerInfoDataAndStartItems(string playerId);
    Task<PlayerInfo> GetPlayerInfoData(string playerId);
    Task UpdateGameResult(string winnerId, string loserId);
    Task<bool> UpdateNickName(string playerId, string newNickName);
    Task<PlayerBasicInfo> GetplayerBasicInfo(string playerId);
    Task<long> GetPlayerUidByPlayerId(string playerId);
    Task<List<PlayerItem>> GetPlayerItems(long playerUid, int page, int pageSize);
    Task<(List<int>, List<string>, List<int>, List<DateTime>, List<long>, List<bool>)> GetPlayerMailBox(long playerUid, int skip, int pageSize);

}