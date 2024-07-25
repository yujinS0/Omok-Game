﻿using Microsoft.AspNetCore.Mvc;
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
        var (result, winner) = await _gameService.PutOmokAsync(request);

        if (result != ErrorCode.None)
        {
            _logger.LogError($"[PutOmok] PlayerId: {request.PlayerId}, ErrorCode: {result}");
        }

        return new PutOmokResponse { Result = result, Winner = winner };
    }

    [HttpPost("giveup-put-omok")]
    public async Task<TurnChangeResponse> TurnChange([FromBody] PlayerRequest request)
    {
        var (result, gameInfo) = await _gameService.TurnChangeAsync(request.PlayerId);
        return new TurnChangeResponse
        {
            Result = result,
            GameInfo = gameInfo
        };
    }

    [HttpPost("turn-checking")] // check-turn? turn-checking?
    public async Task<PlayerResponse> GetCurrentTurnPlayer([FromBody] PlayerRequest request)
    {
        var currentTurn = await _gameService.GetCurrentTurn(request.PlayerId);

        if (currentTurn == OmokStone.None)
        {
            return new PlayerResponse
            {
                Result = ErrorCode.GameTurnNotFound
            };
        }

        string currentTurnPlayer;
        if (currentTurn == OmokStone.Black)
        {
            currentTurnPlayer = await _gameService.GetBlackPlayer(request.PlayerId);
        }
        else if (currentTurn == OmokStone.White)
        {
            currentTurnPlayer = await _gameService.GetWhitePlayer(request.PlayerId);
        }
        else
        {
            return new PlayerResponse
            {
                Result = ErrorCode.GameTurnPlayerNotFound
            };
        }

        if (string.IsNullOrEmpty(currentTurnPlayer))
        {
            return new PlayerResponse
            {
                Result = ErrorCode.GameTurnPlayerNotFound
            };
        }

        return new PlayerResponse
        {
            Result = ErrorCode.None,
            PlayerId = currentTurnPlayer
        };
    }

    // TODO : OmokGameData : 게임 데이터 가져오는 요청
    [HttpPost("omok-game-data")]
    public async Task<BoardResponse> GetOmokGameData([FromBody] PlayerRequest request)
    {
        // TODO 구현 수정해야 함
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


    [HttpPost("black-player")]
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

    [HttpPost("white-player")]
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

    [HttpPost("current-turn")]
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