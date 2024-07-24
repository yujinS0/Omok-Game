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

    public async Task<(ErrorCode Result, string ResponseBody)> VerifyTokenAsync(VerifyTokenRequest verifyTokenRequest)
    {
        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(verifyTokenRequest), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5284/VerifyToken", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return (ErrorCode.None, responseBody);
    }

    public async Task<ErrorCode> SaveLoginInfoAsync(GameLoginRequest request)
    {
        var saveResult = await _memoryDb.SaveUserLoginInfo(request.PlayerId, request.Token, request.AppVersion, request.DataVersion);
        if (!saveResult)
        {
            _logger.LogError("Failed to save login info to Redis for UserId: {UserId}", request.PlayerId);
            return ErrorCode.InternalError;
        }
        return ErrorCode.None;
    }

    public async Task<ErrorCode> InitializeUserDataAsync(string playerId)
    {
        var charInfo = await _gameDb.GetCharInfoDataAsync(playerId);
        if (charInfo == null)
        {
            _logger.LogInformation("First login detected, creating new char_info for hive_player_id: {PlayerId}", playerId);
            charInfo = await _gameDb.CreateUserGameDataAsync(playerId);
        }
        return ErrorCode.None;
    }
}

