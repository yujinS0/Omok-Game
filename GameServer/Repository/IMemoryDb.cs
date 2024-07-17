using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository;

public interface IMemoryDb : IDisposable
{
    Task<bool> SaveUserLoginInfo(string playerId, string token, string appVersion, string dataVersion);
    Task<MatchResult> GetMatchResultAsync(string key);
    Task StorePlayingUserInfoAsync(string key, UserGameData playingUserInfo, TimeSpan expiry);
    Task UpdateGameDataAsync(string key, byte[] rawData, TimeSpan expiry);
}
