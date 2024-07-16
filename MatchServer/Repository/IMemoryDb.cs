using MatchServer.DTO;
using MatchServer.Models;

namespace MatchServer.Repository;

public interface IMemoryDb : IDisposable
{
    Task StoreMatchResultAsync(string key, MatchResult matchResult, TimeSpan expiry);
    Task<MatchResult> GetMatchResultAsync(string key);
    Task StoreGameDataAsync(string key, byte[] rawData, TimeSpan expiry);
    Task StorePlayingUserInfoAsync(string key, PlayingUserInfo playingUserInfo, TimeSpan expiry);
}
