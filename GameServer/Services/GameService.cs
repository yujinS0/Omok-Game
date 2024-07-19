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
    private readonly ILogger<GameService> _logger;

    public GameService(IMemoryDb memoryDb, ILogger<GameService> logger)
    {
        _memoryDb = memoryDb;
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

    //public async Task<Winner> GetWinner(string playerId)
    //{
    //    var omokGameData = await GetGameData(playerId);
    //    if (omokGameData == null)
    //    {
    //        return null;
    //    }

    //    var winner = omokGameData.GetWinner();
    //    if (winner == OmokStone.None)
    //    {
    //        return null;
    //    }

    //    var winnerPlayerId = winner == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName();
    //    return new Winner { Stone = winner, PlayerId = winnerPlayerId };
    //}

    public async Task<ErrorCode> PutOmokAsync(PutOmokRequest request)
    {
        string playingUserKey = KeyGenerator.PlayingUser(request.PlayerId);
        UserGameData userGameData = await _memoryDb.GetPlayingUserInfoAsync(playingUserKey);

        if (userGameData == null)
        {
            _logger.LogError("Failed to retrieve playing user info for PlayerId: {PlayerId}", request.PlayerId);
            return ErrorCode.GameDataNotFound;
        }

        string gameRoomId = userGameData.GameRoomId;

        byte[] rawData = await _memoryDb.GetGameDataAsync(gameRoomId);
        if (rawData == null)
        {
            _logger.LogError("Failed to retrieve game data for RoomId: {RoomId}", gameRoomId);
            return ErrorCode.GameDataNotFound;
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        string currentTurnPlayerName = omokGameData.GetCurrentTurnPlayerName();
        if (request.PlayerId != currentTurnPlayerName)
        {
            _logger.LogError("It is not the player's turn. PlayerId: {PlayerId}", request.PlayerId);
            return ErrorCode.NotYourTurn;
        }

        try
        {
            byte[] updatedRawData = omokGameData.SetStone(rawData, request.PlayerId, request.X, request.Y);
            bool updateResult = await _memoryDb.UpdateGameDataAsync(gameRoomId, updatedRawData);

            if (!updateResult)
            {
                _logger.LogError("Failed to update game data for RoomId: {RoomId}", gameRoomId);
                return ErrorCode.UpdateGameDataFailException;
            }

            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set stone at ({X}, {Y}) for PlayerId: {PlayerId}", request.X, request.Y, request.PlayerId);
            return ErrorCode.SetStoneFailException;
        }
    }
}
