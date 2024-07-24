using GameServer.DTO;
using ServerShared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameServer.Services.Interfaces;

public interface IGameService
{
    Task<OmokGameData> GetGameData(string playerId);
    Task<byte[]> GetBoard(string playerId);
    Task<string> GetBlackPlayer(string playerId);
    Task<string> GetWhitePlayer(string playerId);
    Task<OmokStone> GetCurrentTurn(string playerId);
    Task<(ErrorCode, Winner)> GetWinnerAsync(string playerId);
    Task<(ErrorCode, Winner)> PutOmokAsync(PutOmokRequest request);
    Task<OmokStone> CheckTurnAsync(string playerId);
    Task<(ErrorCode, GameInfo)> TurnChangeAsync(string playerId);
    Task AutoChangeTurn(string playerId);
}
