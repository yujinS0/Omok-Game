using System.Threading.Tasks;
using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Services.Interfaces;

public interface IMatchingService
{
    Task<MatchResponse> RequestMatchingAsync(MatchRequest request);
    Task<MatchResult> CheckAndInitializeMatch(string playerId);
}