using GameServer.DTO;
using ServerShared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameServer.Services.Interfaces;

public interface IGameService
{
    Task<(ErrorCode, Winner)> PutOmokAsync(PutOmokRequest request);
    Task<(ErrorCode, GameInfo)> GiveUpPutOmokAsync(string playerId);
    Task<(ErrorCode, string)> TurnCheckingAsync(string playerId);
    Task<(ErrorCode, byte[]?)> GetGameRawDataAsync(string playerId);
}
