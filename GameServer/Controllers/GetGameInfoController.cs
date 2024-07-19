using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using ServerShared;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class GetGameInfoController : ControllerBase
{
    private readonly ILogger<GetGameInfoController> _logger;
    private readonly IGameInfoService _gameInfoService;

    public GetGameInfoController(ILogger<GetGameInfoController> logger, IGameInfoService gameInfoService)
    {
        _logger = logger;
        _gameInfoService = gameInfoService;
    }

    [HttpPost("board")]
    public async Task<BoardResponse> GetBoard([FromBody] PlayerRequest request)
    {
        var rawData = await _gameInfoService.GetBoard(request.PlayerId);
        if (rawData == null)
        {
            return new BoardResponse { Result = ErrorCode.GameBoardNotFound };
        }
        return new BoardResponse { Result = ErrorCode.None, Board = rawData };
    }


    [HttpPost("black")]
    public async Task<PlayerResponse> GetBlackPlayer([FromBody] PlayerRequest request)
    {
        var blackPlayer = await _gameInfoService.GetBlackPlayer(request.PlayerId);
        if (blackPlayer == null)
        {
            return new PlayerResponse { Result = ErrorCode.GameBlackNotFound };
        }

        return new PlayerResponse { Result = ErrorCode.None, PlayerId = blackPlayer };
    }

    [HttpPost("white")]
    public async Task<PlayerResponse> GetWhitePlayer([FromBody] PlayerRequest request)
    {
        var whitePlayer = await _gameInfoService.GetWhitePlayer(request.PlayerId);
        if (whitePlayer == null)
        {
            return new PlayerResponse { Result = ErrorCode.GameWhiteNotFound };
        }

        return new PlayerResponse { Result = ErrorCode.None, PlayerId = whitePlayer };
    }

    [HttpPost("turn")]
    public async Task<CurrentTurnResponse> GetCurrentTurn([FromBody] PlayerRequest request)
    {
        var currentTurn = await _gameInfoService.GetCurrentTurn(request.PlayerId);
        if (currentTurn == OmokStone.None)
        {
            return new CurrentTurnResponse { Result = ErrorCode.GameTurnNotFound };
        }

        return new CurrentTurnResponse { Result = ErrorCode.None, CurrentTurn = currentTurn };
    }

    [HttpPost("winner")]
    public async Task<WinnerResponse> GetWinner([FromBody] PlayerRequest request)
    {
        var winner = await _gameInfoService.GetWinner(request.PlayerId);
        if (winner == null)
        {
            return new WinnerResponse { Result = ErrorCode.None, Winner = null };
        }

        return new WinnerResponse { Result = ErrorCode.None, Winner = winner };
    }
}
