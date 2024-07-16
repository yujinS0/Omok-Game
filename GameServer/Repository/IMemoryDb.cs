using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository;

public interface IMemoryDb : IDisposable
{
    Task<bool> SaveUserLoginInfo(string playerId, string token, string appVersion, string dataVersion);
    Task<MatchResult> GetMatchResultAsync(string key);
}
