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

    // TODO request 넘겨주기 X??
    public async Task<(ErrorCode, Winner)> PutOmokAsync(PutOmokRequest request) // TODO 함수 분리하기 
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
            var winner = omokGameData.GetWinnerStone();
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

    private Winner CheckForWinner(OmokGameData omokGameData)
    {
        var winnerStone = omokGameData.GetWinnerStone();
        if (winnerStone == OmokStone.None)
        {
            return null;
        }

        var winnerPlayerId = winnerStone == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName();
        return new Winner { Stone = winnerStone, PlayerId = winnerPlayerId };
    }


    public async Task<(ErrorCode, GameInfo)> GiveUpPutOmokAsync(string playerId)
    {
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

        var (errorCode, gameData) = await GetGameRawDataAsync(playerId);

        return (ErrorCode.None, new GameInfo
        {
            Board = gameData,
            CurrentTurn = await GetCurrentTurn(playerId)
        });
    }

    public async Task<(ErrorCode, string)> TurnCheckingAsync(string playerId)
    {
        var currentTurn = await GetCurrentTurn(playerId);

        if (currentTurn == OmokStone.None)
        {
            return (ErrorCode.GameTurnNotFound, null);
        }

        string currentTurnPlayer;
        if (currentTurn == OmokStone.Black)
        {
            currentTurnPlayer = await GetBlackPlayer(playerId);
        }
        else if (currentTurn == OmokStone.White)
        {
            currentTurnPlayer = await GetWhitePlayer(playerId);
        }
        else
        {
            return (ErrorCode.GameTurnPlayerNotFound, null);
        }

        if (string.IsNullOrEmpty(currentTurnPlayer))
        {
            return (ErrorCode.GameTurnPlayerNotFound, null);
        }

        return (ErrorCode.None, currentTurnPlayer);
    }

    public async Task<(ErrorCode, byte[]?)> GetGameRawDataAsync(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomIdAsync(playerId);
        if (gameRoomId == null)
        {
            _logger.LogWarning("Game room not found for player: {PlayerId}", playerId);
            return (ErrorCode.GameRoomNotFound, null);
        }

        var rawData = await _memoryDb.GetGameDataAsync(gameRoomId);
        if (rawData == null)
        {
            _logger.LogWarning("Game data not found for game room: {GameRoomId}", gameRoomId);
            return (ErrorCode.GameBoardNotFound, null);
        }

        return (ErrorCode.None, rawData);
    }

    private async Task<OmokGameData> GetGameData(string playerId)
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

    private async Task<string> GetBlackPlayer(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetBlackPlayerName();
    }

    private async Task<string> GetWhitePlayer(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetWhitePlayerName();
    }

    private async Task<OmokStone> GetCurrentTurn(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetCurrentTurn() ?? OmokStone.None;
    }
}
