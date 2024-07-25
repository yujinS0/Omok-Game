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

    public async Task<ErrorCode> RequestMatchingAsync(MatchRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var matchRequestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(matchRequestJson, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5259/RequestMatching", content);
        if (!response.IsSuccessStatusCode)
        {
            return ErrorCode.InternalError;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var matchResponse = JsonSerializer.Deserialize<MatchResponse>(responseBody);

        return matchResponse?.Result ?? ErrorCode.InternalError;
    }


    public async Task<(ErrorCode, MatchResult)> CheckAndInitializeMatchAsync(string playerId)
    {
        var (errorCode, matchResult) = await GetMatchResultAsync(playerId);

        if (errorCode == ErrorCode.None && matchResult != null)
        {
            await InitializePlayingUserAsync(playerId, matchResult.GameRoomId);
        }

        return (errorCode, matchResult);
    }

    private async Task<(ErrorCode, MatchResult)> GetMatchResultAsync(string playerId)
    {
        var matchResultKey = KeyGenerator.MatchResult(playerId);
        var matchResult = await _memoryDb.GetMatchResultAsync(matchResultKey);

        return matchResult != null ? (ErrorCode.None, matchResult) : (ErrorCode.None, null);
    }

    private async Task<ErrorCode> InitializePlayingUserAsync(string playerId, string gameRoomId)
    {
        var userGameDataKey = KeyGenerator.PlayingUser(playerId);

        var userGameData = new UserGameData
        {
            GameRoomId = gameRoomId
            // CreatedAt Redis에 넣을 때 생성
        };

        var success = await _memoryDb.StorePlayingUserInfoAsync(userGameDataKey, userGameData);
        return success ? ErrorCode.None : ErrorCode.InternalError;
    }

}