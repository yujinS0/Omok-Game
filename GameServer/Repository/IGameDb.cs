using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository;

public interface IGameDb : IDisposable
{
    Task<PlayerInfo> CreatePlayerInfoDataAsync(string playerId);
    Task<PlayerInfo> GetPlayerInfoDataAsync(string playerId);
    Task UpdateGameResultAsync(string winnerId, string loserId);
    Task<bool> UpdateNickNameAsync(string playerId, string newNickName);
    Task<PlayerBasicInfo> GetplayerBasicInfoAsync(string playerId);
}