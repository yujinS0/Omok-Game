using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository;
using GameServer.Services.Interfaces;
using ServerShared;
using StackExchange.Redis;

namespace GameServer.Services;

public class LoginService : ILoginService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoginService> _logger;
    private readonly IGameDb _gameDb;
    private readonly IMemoryDb _memoryDb;

    public LoginService(IHttpClientFactory httpClientFactory, ILogger<LoginService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _gameDb = gameDb;
        _memoryDb = memoryDb;
    }

    
    public async Task<ErrorCode> login(string playerId, string token, string appVersion, string dataVersion)
    {
        //TODO: 버그. result을 평가하는 코드가 없습니다.
        var result = await VerifyToken(playerId, token);


        var saveResult = await SavePlayerLoginInfoToMemoryDb(playerId, token, appVersion, dataVersion);
        if (saveResult != ErrorCode.None)
        {
            return saveResult;
        }

        var initializeResult = await InitializeUserData(playerId);
        if (initializeResult != ErrorCode.None)
        {
            await _memoryDb.DeletePlayerLoginInfo(playerId); 
            return initializeResult;
        }

        _logger.LogInformation("Successfully authenticated user with token");

        return ErrorCode.None;
    }

    private async Task<ErrorCode> VerifyToken(string playerId, string token)
    {
        var client = _httpClientFactory.CreateClient();

        var verifyTokenRequest = new VerifyTokenRequest
        {
            HiveUserId = playerId,
            HiveToken = token
        };

        var response = await client.PostAsJsonAsync("http://localhost:5284/VerifyToken", verifyTokenRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            return ErrorCode.InternalError;
        }
        
        var responseBody = await response.Content.ReadFromJsonAsync<VerifyTokenResponse>();

        if (responseBody != null)
        {
            return responseBody.Result;
        }
        else
        {
            _logger.LogError("Failed to parse VerifyTokenResponse.");
            return ErrorCode.InternalError;
        }
    }

    private async Task<ErrorCode> SavePlayerLoginInfoToMemoryDb(string playerId, string token, string appVersion, string dataVersion)
    {
        var saveResult = await _memoryDb.SavePlayerLoginInfo(playerId, token, appVersion, dataVersion);
        if (!saveResult)
        {
            _logger.LogError("Failed to save login info to Redis for UserId: {UserId}", playerId);
            return ErrorCode.InternalError;
        }
        return ErrorCode.None;
    }
    private async Task<ErrorCode> InitializeUserData(string playerId)
    {
        var playerInfo = await _gameDb.GetPlayerInfoData(playerId);
        if (playerInfo == null)
        {
            _logger.LogInformation("First login detected, creating new player_info for hive_player_id: {PlayerId}", playerId);
            playerInfo = await _gameDb.CreatePlayerInfoData(playerId);
        }
        return ErrorCode.None;
    }

}

