using GameServer.Models;

namespace GameServer.Repository;

public interface IGameDb : IDisposable
{
    Task<CharInfo> CreateUserGameDataAsync(string playerId);
    Task<CharInfo> GetUserGameDataAsync(string playerId);
    Task UpdateCharNameAsync(string playerId, string newCharName);
}