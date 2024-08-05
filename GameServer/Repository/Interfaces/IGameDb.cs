using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Repository.Interfaces;

public interface IGameDb : IDisposable
{
    Task<PlayerInfo> CreatePlayerInfoDataAndStartItems(string playerId);
    Task<PlayerInfo> GetPlayerInfoData(string playerId);
    Task<bool> UpdateGameResult(string winnerId, string loserId, int WinExp, int LoseExp);
    Task<bool> UpdateNickName(string playerId, string newNickName);
    Task<PlayerBasicInfo> GetplayerBasicInfo(string playerId);
    Task<long> GetPlayerUidByPlayerId(string playerId);
    Task<List<PlayerItem>> GetPlayerItems(long playerUid, int page, int pageSize);
    Task<(List<Int64>, List<string>, List<int>, List<DateTime>, List<long>, List<int>)> GetPlayerMailBox(long playerUid, int skip, int pageSize);
    Task<MailDetail> GetMailDetail(long playerUid, Int64 mailId);
    Task UpdateMailReceiveStatus(long playerUid, Int64 mailId);
    Task AddPlayerItem(long playerUid, int itemCode, int itemCnt);
    Task DeleteMail(long playerUid, Int64 mailId);
}