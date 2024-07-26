using GameServer.DTO;
using ServerShared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameServer.Services.Interfaces;

public interface IGameService
{
    Task<(ErrorCode, Winner)> PutOmokAsync(string playerId, int x, int y);
    Task<(ErrorCode, GameInfo)> GiveUpPutOmokAsync(string playerId);
    Task<(ErrorCode, string)> TurnCheckingAsync(string playerId);
    Task<(ErrorCode, byte[]?)> GetGameRawDataAsync(string playerId);
}
