using System.Threading.Tasks;
using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Services.Interfaces;

public interface ICheckMatchingService
{
    Task<MatchResult> CheckAndInitializeMatch(string playerId);
}