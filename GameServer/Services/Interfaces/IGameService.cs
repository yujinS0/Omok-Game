using GameServer.DTO;
using ServerShared;
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
    Task<ErrorCode> PutOmokAsync(PutOmokRequest request);
}
