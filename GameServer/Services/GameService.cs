using GameServer.DTO;
using GameServer.Repository;
using ServerShared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GameServer.Services.Interfaces;
using GameServer.Models;

namespace GameServer.Services;

public class GameService : IGameService
{
    private readonly IMemoryDb _memoryDb;
    private readonly IGameDb _gameDb;
    private readonly ILogger<GameService> _logger;

    public GameService(IMemoryDb memoryDb, IGameDb gameDb, ILogger<GameService> logger)
    {
        _memoryDb = memoryDb;
        _gameDb = gameDb;
        _logger = logger;
    }


    public async Task<OmokGameData> GetGameData(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomIdAsync(playerId);
        if (gameRoomId == null)
        {
            return null;
        }

        var rawData = await _memoryDb.GetGameDataAsync(gameRoomId);
        if (rawData == null)
        {
            return null;
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        return omokGameData;
    }

    public async Task<byte[]> GetBoard(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomIdAsync(playerId);
        if (gameRoomId == null)
        {
            return null;
        }

        var rawData = await _memoryDb.GetGameDataAsync(gameRoomId);
        if (rawData == null)
        {
            return null;
        }
        //Console.WriteLine($"rawData: {BitConverter.ToString(rawData)}");

        return rawData;
    }

    public async Task<string> GetBlackPlayer(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetBlackPlayerName();
    }

    public async Task<string> GetWhitePlayer(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetWhitePlayerName();
    }

    public async Task<OmokStone> GetCurrentTurn(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetCurrentTurn() ?? OmokStone.None;
    }

    public async Task<(ErrorCode, Winner)> GetWinnerAsync(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        if (omokGameData == null)
        {
            return (ErrorCode.GameDataNotFound, null);
        }

        var winner = omokGameData.GetWinnerStone();
        if (winner == OmokStone.None)
        {
            return (ErrorCode.None, null);
        }

        var winnerPlayerId = winner == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName();
        return (ErrorCode.None, new Winner { Stone = winner, PlayerId = winnerPlayerId });
    }

    public async Task<(ErrorCode, Winner)> PutOmokAsync(PutOmokRequest request)
    {
        string playingUserKey = KeyGenerator.PlayingUser(request.PlayerId);
        UserGameData userGameData = await _memoryDb.GetPlayingUserInfoAsync(playingUserKey);

        if (userGameData == null)
        {
            _logger.LogError("Failed to retrieve playing user info for PlayerId: {PlayerId}", request.PlayerId);
            return (ErrorCode.UserGameDataNotFound, null);
        }

        string gameRoomId = userGameData.GameRoomId;

        byte[] rawData = await _memoryDb.GetGameDataAsync(gameRoomId);
        if (rawData == null)
        {
            _logger.LogError("Failed to retrieve game data for RoomId: {RoomId}", gameRoomId);
            return (ErrorCode.GameRoomNotFound, null);
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        // Check if the game has already ended
        //var currentWinner = omokGameData.GetWinnerStone();
        //if (currentWinner != OmokStone.None)
        //{
        //    _logger.LogError("The game has already ended. PlayerId: {PlayerId}", request.PlayerId);
        //    return (ErrorCode.GameEnd, new Winner { Stone = currentWinner, PlayerId = currentWinner == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName() });
        //}

        string currentTurnPlayerName = omokGameData.GetCurrentTurnPlayerName();
        if (request.PlayerId != currentTurnPlayerName)
        {
            _logger.LogError("It is not the player's turn. PlayerId: {PlayerId}", request.PlayerId);
            return (ErrorCode.NotYourTurn, null);
        }

        try
        {
            byte[] updatedRawData = omokGameData.SetStone(rawData, request.PlayerId, request.X, request.Y);
            bool updateResult = await _memoryDb.UpdateGameDataAsync(gameRoomId, updatedRawData);

            if (!updateResult)
            {
                _logger.LogError("Failed to update game data for RoomId: {RoomId}", gameRoomId);
                return (ErrorCode.UpdateGameDataFailException, null);
            }

            // 오목 두고 승자 체크하기
            var winner = omokGameData.GetWinnerStone(); // TODO 체크 전 후로 다 하고 있음 이게 최선인가?
            if (winner != OmokStone.None)
            {
                var winnerPlayerId = winner == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName();
                var loserPlayerId = winner == OmokStone.Black ? omokGameData.GetWhitePlayerName() : omokGameData.GetBlackPlayerName();

                // 오목 결과 GameDb에 업데이트
                await _gameDb.UpdateGameResultAsync(winnerPlayerId, loserPlayerId); // 승자와 패자 업데이트

                return (ErrorCode.None, new Winner { Stone = winner, PlayerId = winnerPlayerId });
            }

            return (ErrorCode.None, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set stone at ({X}, {Y}) for PlayerId: {PlayerId}", request.X, request.Y, request.PlayerId);
            return (ErrorCode.SetStoneFailException, null);
        }
    }


    // WaitForTurnChangeAsync : 30초 간격 long polling을 처리
    //public async Task<(ErrorCode, GameInfo)> WaitForTurnChangeAsync(string playerId, CancellationToken cancellationToken)
    //{
    //    var initialTurn = await GetCurrentTurn(playerId);
    //    var initialTurnTime = DateTime.UtcNow;

    //    try
    //    {
    //        while (!cancellationToken.IsCancellationRequested) // 취소 요청 있을 때까지 반복
    //        {
    //            var currentTurn = await GetCurrentTurn(playerId);
    //            if (currentTurn != initialTurn)
    //            {
    //                return (ErrorCode.None, new GameInfo
    //                {
    //                    Board = await GetBoard(playerId),
    //                    CurrentTurn = currentTurn,
    //                });
    //            }
    //            await Task.Delay(500, cancellationToken); // 0.5초 대기
    //        }
    //        // 정상적인 상황에서는 여기 X
    //        // 타임아웃이 발생하지 않고 루프가 종료된 경우
    //        return (ErrorCode.RequestTurnEnd, null);
    //        //throw new InvalidOperationException("Loop terminated unexpectedly");
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        // 30초 타임아웃이 발생
    //        await AutoChangeTurn(playerId);
    //        return (ErrorCode.TurnChangedByTimeout, new GameInfo
    //        {
    //            Board = await GetBoard(playerId),
    //            CurrentTurn = await GetCurrentTurn(playerId),
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error in WaitForTurnChangeAsync.");
    //        return (ErrorCode.InternalError, null);
    //    }
    //}

    public async Task<(ErrorCode, GameInfo)> TurnChangeAsync(string playerId)
    {
        var initialTurn = await GetCurrentTurn(playerId);
        var initialTurnTime = DateTime.UtcNow;

        await AutoChangeTurn(playerId);

        return (ErrorCode.None, new GameInfo
        {
            Board = await GetBoard(playerId),
            CurrentTurn = await GetCurrentTurn(playerId)
        });
    }

    public async Task AutoChangeTurn(string playerId)
    {
        _logger.LogInformation("AutoChangeTurn 함수 호출");
        var gameRoomId = await _memoryDb.GetGameRoomIdAsync(playerId);
        var rawData = await _memoryDb.GetGameDataAsync(gameRoomId);

        var omokGameData = new OmokGameData();

        try
        {
            var updatedRawData = omokGameData.ChangeTurn(rawData, playerId);
            await _memoryDb.UpdateGameDataAsync(gameRoomId, updatedRawData);
            _logger.LogInformation("Turn changed successfully for player {PlayerId}", playerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change turn for player {PlayerId}", playerId);
        }
    }

    // CheckTurnAsync : 현재 턴 확인
    public async Task<OmokStone> CheckTurnAsync(string playerId) 
    {
        return await GetCurrentTurn(playerId);
    }

    
}
