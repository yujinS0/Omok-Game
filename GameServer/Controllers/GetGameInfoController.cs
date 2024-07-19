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
public class GetGameInfoController : ControllerBase
{
    private readonly ILogger<GetGameInfoController> _logger;
    private readonly IGameService _gameService;

    public GetGameInfoController(ILogger<GetGameInfoController> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost("board")]
    public async Task<BoardResponse> GetBoard([FromBody] PlayerRequest request)
    {
        var rawData = await _gameService.GetBoard(request.PlayerId);
        if (rawData == null)
        {
            return new BoardResponse 
            { 
                Result = ErrorCode.GameBoardNotFound 
            };
        }
        return new BoardResponse 
        { 
            Result = ErrorCode.None, 
            Board = rawData 
        };
    }


    [HttpPost("black")]
    public async Task<PlayerResponse> GetBlackPlayer([FromBody] PlayerRequest request)
    {
        var blackPlayer = await _gameService.GetBlackPlayer(request.PlayerId);
        if (blackPlayer == null)
        {
            return new PlayerResponse 
            { 
                Result = ErrorCode.GameBlackNotFound 
            };
        }

        return new PlayerResponse 
        { 
            Result = ErrorCode.None, 
            PlayerId = blackPlayer 
        };
    }

    [HttpPost("white")]
    public async Task<PlayerResponse> GetWhitePlayer([FromBody] PlayerRequest request)
    {
        var whitePlayer = await _gameService.GetWhitePlayer(request.PlayerId);
        if (whitePlayer == null)
        {
            return new PlayerResponse 
            { 
                Result = ErrorCode.GameWhiteNotFound 
            };
        }

        return new PlayerResponse 
        { 
            Result = ErrorCode.None, 
            PlayerId = whitePlayer 
        };
    }

    [HttpPost("turn")]
    public async Task<CurrentTurnResponse> GetCurrentTurn([FromBody] PlayerRequest request)
    {
        var currentTurn = await _gameService.GetCurrentTurn(request.PlayerId);
        if (currentTurn == OmokStone.None)
        {
            return new CurrentTurnResponse 
            { 
                Result = ErrorCode.GameTurnNotFound 
            };
        }

        return new CurrentTurnResponse 
        { 
            Result = ErrorCode.None, 
            CurrentTurn = currentTurn 
        };
    }

    [HttpPost("winner")]
    public async Task<WinnerResponse> GetWinner([FromBody] PlayerRequest request)
    {
        var (result, winner) = await _gameService.GetWinnerAsync(request.PlayerId);

        if (result != ErrorCode.None)
        {
            _logger.LogError($"[GetWinner] PlayerId: {request.PlayerId}, ErrorCode: {result}");
            return new WinnerResponse { Result = result, Winner = null };
        }

        return new WinnerResponse
        {
            Result = ErrorCode.None,
            Winner = winner
        };
    }
}
