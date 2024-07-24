using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface ICharacterService
{
    Task<(ErrorCode, CharSummary?)> GetCharInfoSummaryAsync(string playerId);
    Task<ErrorCode> UpdateCharacterNameAsync(string playerId, string newCharName);
}
