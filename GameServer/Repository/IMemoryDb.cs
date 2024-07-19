using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository;

public interface IMemoryDb : IDisposable
{
    Task<bool> SaveUserLoginInfo(string playerId, string token, string appVersion, string dataVersion);
    Task<MatchResult> GetMatchResultAsync(string key);
    Task StorePlayingUserInfoAsync(string key, UserGameData playingUserInfo);
    Task<byte[]> GetGameDataAsync(string key);
    Task<bool> UpdateGameDataAsync(string key, byte[] rawData);
    Task<UserGameData> GetPlayingUserInfoAsync(string key);
    Task<string> GetGameRoomIdAsync(string playerId);
}
