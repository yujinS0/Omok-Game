using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository;

public interface IMemoryDb : IDisposable
{
    Task<bool> SavePlayerLoginInfo(string playerId, string token, string appVersion, string dataVersion);
    Task<bool> DeletePlayerLoginInfo(string playerId);
    Task<string> GetUserLoginToken(string playerId);
    Task<MatchResult> GetMatchResult(string key);
    Task<bool> StorePlayingUserInfo(string key, UserGameData playingUserInfo);
    Task<byte[]> GetGameData(string key);
    Task<bool> UpdateGameData(string key, byte[] rawData);
    Task<UserGameData> GetPlayingUserInfo(string key);
    Task<string> GetGameRoomId(string playerId);
    Task<bool> SetUserReqLock(string key);
    Task<bool> DelUserReqLock(string key);
}
