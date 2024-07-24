using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IPlayerInfoService
{
    Task<(ErrorCode, CharSummary?)> GetCharInfoSummaryAsync(string playerId);
    Task<ErrorCode> UpdateCharacterNameAsync(string playerId, string newCharName);
}
