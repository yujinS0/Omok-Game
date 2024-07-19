using GameServer;
using GameServer.DTO;
using GameServer.Repository;
using GameServer.Models;
using ServerShared;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using GameServer.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace MatchServer.Services;
public class MatchingService : IMatchingService
{
    private readonly IMemoryDb _memoryDb;
    private readonly ILogger<MatchingService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public MatchingService(IMemoryDb memoryDb, ILogger<MatchingService> logger, IHttpClientFactory httpClientFactory)
    {
        _memoryDb = memoryDb;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<MatchResponse> RequestMatchingAsync(MatchRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var matchRequestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(matchRequestJson, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5259/RequestMatching", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var matchResponse = JsonSerializer.Deserialize<MatchResponse>(responseBody);

        return matchResponse ?? new MatchResponse { Result = ErrorCode.InternalError };
    }

    public async Task<MatchResult> CheckAndInitializeMatch(string playerId)
    {
        var matchResultkey = KeyGenerator.MatchResult(playerId);
        var result = await _memoryDb.GetMatchResultAsync(matchResultkey);

        if (result == null)
        {
            return null;
        }

        // 매칭 성공 확인 시
        var userGameDatakey = KeyGenerator.PlayingUser(playerId);

        var userGameData = new UserGameData
        {
            GameRoomId = result.GameRoomId
            // CreatedAt Redis에 넣을 때 생성
        };

        await _memoryDb.StorePlayingUserInfoAsync(userGameDatakey, userGameData);

        // 매칭 성공 했으니 게임 시작 상태로 바꿔주기
        byte[] getGameRawData = await _memoryDb.GetGameDataAsync(result.GameRoomId);

        var omokGameData = new OmokGameData();
        byte[] gameRawData = omokGameData.StartGame(getGameRawData);

        var gameStartResult = await _memoryDb.UpdateGameDataAsync(result.GameRoomId, gameRawData);

        if (!gameStartResult)
        {
            _logger.LogError("Failed to update game info for RoomId: {RoomId}", result.GameRoomId);
            return null;
        }

        return result;
    }

}