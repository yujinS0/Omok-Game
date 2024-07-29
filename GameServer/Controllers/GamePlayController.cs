using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using ServerShared;
using GameServer.Services;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class GamePlayController : ControllerBase
{
    private readonly ILogger<GamePlayController> _logger;
    private readonly IGameService _gameService;

    public GamePlayController(ILogger<GamePlayController> logger, IGameService gameService) 
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost("put-omok")]
    public async Task<PutOmokResponse> PutOmok([FromBody] PutOmokRequest request) 
    {
        //TODO: 버그. 미들웨어에서는 헤더에 있는 PlayerId를 사용하는데 여기서는 body(요청)에 있는 PlayerId를 사용하고 있습니다. 유저가 헤더와 보디의 PlayerId를 다르게 사용할 수도 있습니다.
        var (result, winner) = await _gameService.PutOmok(request.PlayerId, request.X, request.Y); 

        if (result != ErrorCode.None)
        {
            _logger.LogError($"[PutOmok] PlayerId: {request.PlayerId}, ErrorCode: {result}");
        }

        return new PutOmokResponse { Result = result, Winner = winner };
    }

    [HttpPost("giveup-put-omok")]
    public async Task<TurnChangeResponse> GiveUpPutOmok([FromBody] PlayerRequest request)
    {
        var (result, gameInfo) = await _gameService.GiveUpPutOmok(request.PlayerId);
        return new TurnChangeResponse
        {
            Result = result,
            GameInfo = gameInfo
        };
    }

    [HttpPost("turn-checking")]
    public async Task<PlayerResponse> TurnChecking([FromBody] PlayerRequest request)
    {
        var (result, currentTurnPlayer) = await _gameService.TurnChecking(request.PlayerId);

        if (result != ErrorCode.None)
        {
            return new PlayerResponse
            {
                Result = result
            };
        }

        return new PlayerResponse
        {
            Result = ErrorCode.None,
            PlayerId = currentTurnPlayer
        };
    }

    [HttpPost("omok-game-data")] // 게임 전체 데이터 가져오는 요청
    public async Task<BoardResponse> GetOmokGameData([FromBody] PlayerRequest request)
    {
        var (result, gameData) = await _gameService.GetGameRawData(request.PlayerId);

        if (result != ErrorCode.None)
        {
            return new BoardResponse
            {
                Result = result
            };
        }

        return new BoardResponse
        {
            Result = ErrorCode.None,
            Board = gameData
        };
    }
}
