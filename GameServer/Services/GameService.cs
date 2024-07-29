using GameServer.DTO;
using GameServer.Repository;
using ServerShared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GameServer.Services.Interfaces;
using GameServer.Models;
using StackExchange.Redis;

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

    public async Task<(ErrorCode, Winner)> PutOmok(string playerId, int x, int y)
    {
        var (validatePlayerTurn, omokGameData, rawData, gameRoomId) = await ValidatePlayerTurn(playerId);

        if (validatePlayerTurn != ErrorCode.None)
        {
            _logger.LogError("validatePlayerTurn : Fail");
            return (validatePlayerTurn, null);
        }

        try
        {
            byte[] updatedRawData = omokGameData.SetStone(rawData, playerId, x, y);
            bool updateResult = await _memoryDb.UpdateGameData(gameRoomId, updatedRawData);

            if (!updateResult)
            {
                _logger.LogError("Failed to update game data for RoomId: {RoomId}", gameRoomId);
                return (ErrorCode.UpdateGameDataFailException, null);
            }

            // 오목 두고 승자 체크하기
            var winner = await CheckForWinner(omokGameData);
            if (winner != null)
            {
                return (ErrorCode.None, winner);
            }

            return (ErrorCode.None, null);

        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation error occurred while setting stone at ({X}, {Y}) for PlayerId: {PlayerId}", x, y, playerId);
            return (ErrorCode.InvalidOperationException, null);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error occurred while updating game data for RoomId: {RoomId}", gameRoomId);
            return (ErrorCode.RedisException, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set stone at ({X}, {Y}) for PlayerId: {PlayerId}", x, y, playerId);
            return (ErrorCode.SetStoneFailException, null);
        }
    }

    private async Task<(ErrorCode, OmokGameData, byte[], string)> ValidatePlayerTurn(string playerId)
    {
        string playingUserKey = KeyGenerator.PlayingUser(playerId);
        UserGameData userGameData = await _memoryDb.GetPlayingUserInfo(playingUserKey);

        if (userGameData == null)
        {
            _logger.LogError("Failed to retrieve playing user info for PlayerId: {PlayerId}", playerId);
            return (ErrorCode.UserGameDataNotFound, null, null, null);
        }

        string gameRoomId = userGameData.GameRoomId;

        byte[] rawData = await _memoryDb.GetGameData(gameRoomId);
        if (rawData == null)
        {
            _logger.LogError("Failed to retrieve game data for RoomId: {RoomId}", gameRoomId);
            return (ErrorCode.GameRoomNotFound, null, null, null);
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        string currentTurnPlayerName = omokGameData.GetCurrentTurnPlayerName();
        if (playerId != currentTurnPlayerName)
        {
            _logger.LogError("It is not the player's turn. PlayerId: {PlayerId}", playerId);
            return (ErrorCode.NotYourTurn, null, null, null);
        }

        return (ErrorCode.None, omokGameData, rawData, gameRoomId);
    }

    private async Task<Winner> CheckForWinner(OmokGameData omokGameData)
    {
        var winnerStone = omokGameData.GetWinnerStone();
        if (winnerStone == OmokStone.None)
        {
            return null;
        }

        var winnerPlayerId = winnerStone == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName();
        var loserPlayerId = winnerStone == OmokStone.Black ? omokGameData.GetWhitePlayerName() : omokGameData.GetBlackPlayerName();

        await _gameDb.UpdateGameResult(winnerPlayerId, loserPlayerId); // GameDb에 결과 업데이트

        return new Winner { Stone = winnerStone, PlayerId = winnerPlayerId };
    }

    public async Task<(ErrorCode, GameInfo)> GiveUpPutOmok(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        var rawData = await _memoryDb.GetGameData(gameRoomId);

        var omokGameData = new OmokGameData();

        try
        {
            var updatedRawData = omokGameData.ChangeTurn(rawData, playerId);
            await _memoryDb.UpdateGameData(gameRoomId, updatedRawData);
            _logger.LogInformation("Turn changed successfully for player {PlayerId}", playerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change turn for player {PlayerId}", playerId);
        }

        var (errorCode, gameData) = await GetGameRawData(playerId);

        return (ErrorCode.None, new GameInfo
        {
            Board = gameData,
            CurrentTurn = await GetCurrentTurn(playerId)
        });
    }

    public async Task<(ErrorCode, string)> TurnChecking(string playerId)
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

    public async Task<(ErrorCode, byte[]?)> GetGameRawData(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        if (gameRoomId == null)
        {
            _logger.LogWarning("Game room not found for player: {PlayerId}", playerId);
            return (ErrorCode.GameRoomNotFound, null);
        }

        var rawData = await _memoryDb.GetGameData(gameRoomId);
        if (rawData == null)
        {
            _logger.LogWarning("Game data not found for game room: {GameRoomId}", gameRoomId);
            return (ErrorCode.GameBoardNotFound, null);
        }

        return (ErrorCode.None, rawData);
    }

    private async Task<OmokGameData> GetGameData(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        if (gameRoomId == null)
        {
            return null;
        }

        var rawData = await _memoryDb.GetGameData(gameRoomId);
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
