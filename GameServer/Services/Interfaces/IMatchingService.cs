using System.Threading.Tasks;
using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IMatchingService
{
    Task<ErrorCode> RequestMatchingAsync(MatchRequest request);
    Task<(ErrorCode, MatchResult)> CheckAndInitializeMatchAsync(string playerId);
}