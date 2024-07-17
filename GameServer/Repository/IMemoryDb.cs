using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository;

public interface IMemoryDb : IDisposable
{
    Task<bool> SaveUserLoginInfo(string playerId, string token, string appVersion, string dataVersion);
    Task<MatchResult> GetMatchResultAsync(string key);
    Task StorePlayingUserInfoAsync(string key, UserGameData playingUserInfo, TimeSpan expiry);
    Task<byte[]> GetGameDataAsync(string key);
    Task<bool> UpdateGameDataAsync(string key, byte[] rawData, TimeSpan expiry);
    Task<UserGameData> GetPlayingUserInfoAsync(string key);
}
