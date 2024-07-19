using GameServer.DTO;
using GameServer.Repository;
using ServerShared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GameServer.Services.Interfaces;

namespace GameServer.Services;

public class GameInfoService : IGameInfoService
{
    private readonly IMemoryDb _memoryDb;
    private readonly ILogger<GameInfoService> _logger;

    public GameInfoService(IMemoryDb memoryDb, ILogger<GameInfoService> logger)
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

        Console.WriteLine($"1. rawData: {rawData}");
        Console.WriteLine($"2. rawData length: {rawData.Length}");
        Console.WriteLine($"3. rawData: {BitConverter.ToString(rawData)}");

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

    public async Task<Winner> GetWinner(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        if (omokGameData == null)
        {
            return null;
        }

        var winner = omokGameData.GetWinner();
        if (winner == OmokStone.None)
        {
            return null;
        }

        var winnerPlayerId = winner == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName();
        return new Winner { Stone = winner, PlayerId = winnerPlayerId };
    }
}
