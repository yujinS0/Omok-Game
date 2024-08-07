using GameServer.DTO;
using ServerShared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GameServer.Services.Interfaces;
using GameServer.Models;
using StackExchange.Redis;
using GameServer.Repository.Interfaces;

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
        var (validatePlayerTurn, omokGameData, gameRoomId) = await ValidatePlayerTurn(playerId);

        if (validatePlayerTurn != ErrorCode.None)
        {
            _logger.LogError("validatePlayerTurn : Fail");
            return (validatePlayerTurn, null);
        }

        try
        {
            omokGameData.SetStone(playerId, x, y);
            bool updateResult = await _memoryDb.UpdateGameData(gameRoomId, omokGameData.GetRawData());

            if (!updateResult)
            {
                _logger.LogError("Failed to update game data for RoomId: {RoomId}", gameRoomId);
                return (ErrorCode.UpdateGameDataFailException, null);
            }

            // 오목 두고 승자 체크하기
            var (result, winner) = await CheckForWinner(omokGameData);
            if (result != ErrorCode.None)
            {
                return (result, null);
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

    private async Task<(ErrorCode, OmokGameData, string)> ValidatePlayerTurn(string playerId)
    {
        string playingUserKey = KeyGenerator.PlayingUser(playerId);
        UserGameData userGameData = await _memoryDb.GetPlayingUserInfo(playingUserKey);

        if (userGameData == null)
        {
            _logger.LogError("Failed to retrieve playing user info for PlayerId: {PlayerId}", playerId);
            return (ErrorCode.UserGameDataNotFound, null, null);
        }

        string gameRoomId = userGameData.GameRoomId;

        byte[] rawData = await _memoryDb.GetGameData(gameRoomId);
        if (rawData == null)
        {
            _logger.LogError("Failed to retrieve game data for RoomId: {RoomId}", gameRoomId);
            return (ErrorCode.GameRoomNotFound, null, null);
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        // 게임이 끝난 상태인지 체크
        OmokStone winnerStone = omokGameData.GetWinnerStone();
        if (winnerStone != OmokStone.None)
        {
            _logger.LogError("Game End. PlayerId: {PlayerId}", playerId);
            return (ErrorCode.GameAlreadyEnd, null, null);
        }

        string currentTurnPlayerId = omokGameData.GetCurrentTurnPlayerId();
        if (playerId != currentTurnPlayerId)
        {
            _logger.LogError("It is not the player's turn. PlayerId: {PlayerId}", playerId);
            return (ErrorCode.NotYourTurn, null, null);
        }
        
        return (ErrorCode.None, omokGameData, gameRoomId);
    }

    private async Task<(ErrorCode, Winner)> CheckForWinner(OmokGameData omokGameData)
    {
        var winnerStone = omokGameData.GetWinnerStone();
        if (winnerStone == OmokStone.None)
        {
            return (ErrorCode.None, null);
        }

        //TODO: (08.07) GetWinnerAndLoser 이 함수는 omokGameData 객체의 메서드로 있는 것이 더 자연스럽습니다
        var (winnerPlayerId, loserPlayerId) = GetWinnerAndLoser(winnerStone, omokGameData);

        try
        {
            var updateResult = await _gameDb.UpdateGameResult(winnerPlayerId, loserPlayerId, GameConstants.WinExp, GameConstants.LoseExp);
            if (!updateResult)
            {
                return (ErrorCode.UpdateGameResultFail, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game result for winner: {WinnerId}, loser: {LoserId}", winnerPlayerId, loserPlayerId);
            return (ErrorCode.UpdateGameResultFail, null);
        }

        return (ErrorCode.None, new Winner { Stone = winnerStone, PlayerId = winnerPlayerId });
    }

    private (string winnerPlayerId, string loserPlayerId) GetWinnerAndLoser(OmokStone winnerStone, OmokGameData omokGameData)
    {
        string winnerPlayerId;
        string loserPlayerId;

        if (winnerStone == OmokStone.Black)
        {
            winnerPlayerId = omokGameData.GetBlackPlayerId();
            loserPlayerId = omokGameData.GetWhitePlayerId();
        }
        else
        {
            winnerPlayerId = omokGameData.GetWhitePlayerId();
            loserPlayerId = omokGameData.GetBlackPlayerId();
        }

        return (winnerPlayerId, loserPlayerId);
    }

    public async Task<(ErrorCode, GameInfo)> GiveUpPutOmok(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        if (gameRoomId == null)
        {
            return (ErrorCode.GameRoomNotFound, null);
        }
        var rawData = await _memoryDb.GetGameData(gameRoomId);

        var omokGameData = new OmokGameData();

        try
        {
            var (result, updatedRawData) = omokGameData.ChangeTurn(rawData, playerId);
            if (result != ErrorCode.None)
            {
                return (result, null);
            }
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
            CurrentTurn = await GetCurrentTurnStone(playerId)
        });
    }

    public async Task<(ErrorCode, bool)> TurnChecking(string playerId)
    {
        var currentTurnPlayerId = await GetCurrentTurnPlayerId(playerId);

        if (string.IsNullOrEmpty(currentTurnPlayerId))
        {
            return (ErrorCode.GameTurnPlayerNotFound, false);
        }

        if (playerId == currentTurnPlayerId)
        {
            return (ErrorCode.None, true);
        }
        else
        {
            return (ErrorCode.None, false);
        }
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

    private async Task<string> GetCurrentTurnPlayerId(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetCurrentTurnPlayerId();
    }

    private async Task<OmokStone> GetCurrentTurnStone(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetCurrentTurn() ?? OmokStone.None;
    }
}
