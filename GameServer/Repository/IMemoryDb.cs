using GameServer.DTO;

namespace GameServer.Repository;

public interface IMemoryDb : IDisposable
{
    Task<bool> SaveUserLoginInfo(string playerId, string token, string appVersion, string dataVersion);
}
