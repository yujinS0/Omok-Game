using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository;

public interface IGameDb : IDisposable
{
    Task<CharInfo> CreateUserGameDataAsync(string playerId);
    Task<CharInfo> GetCharInfoDataAsync(string playerId);
    Task UpdateCharNameAsync(string playerId, string newCharName);
    Task UpdateGameResultAsync(string winnerId, string loserId);
    Task<bool> UpdateCharacterNameAsync(string playerId, string newCharName);
    Task<CharInfoDTO> GetCharInfoSummaryAsync(string playerId);
}