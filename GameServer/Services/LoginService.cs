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

    public async Task<ErrorCode> SaveLoginInfoAsync(LoginRequest request)
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

    //public async Task<LoginResponse> Login(LoginRequest request)
    //{
    //    var client = _httpClientFactory.CreateClient();

    //    var verifyTokenRequest = new VerifyTokenRequest
    //    {
    //        hive_player_id = request.PlayerId,
    //        hive_token = request.Token
    //    };

    //    var content = new StringContent(JsonSerializer.Serialize(verifyTokenRequest), Encoding.UTF8, "application/json");
    //    _logger.LogInformation("Sending token validation request to external API with content: {Content}", await content.ReadAsStringAsync());

    //    try
    //    {
    //        var response = await client.PostAsync("http://localhost:5284/VerifyToken", content);
    //        var responseBody = await response.Content.ReadAsStringAsync();
    //        _logger.LogInformation("Received response with status {StatusCode}: {ResponseBody}", response.StatusCode, responseBody);

    //        int result;
    //        using (JsonDocument doc = JsonDocument.Parse(responseBody))
    //        {
    //            JsonElement root = doc.RootElement;
    //            result = root.GetProperty("result").GetInt32();
    //        }

    //        _logger.LogInformation("Validation result from API: {Result}", result);

    //        if (result != 0)
    //        {
    //            _logger.LogWarning("Token validation failed with result: {Result}", result);
    //            return new LoginResponse
    //            {
    //                Result = (ErrorCode)result
    //            };
    //        }

    //        // Redis에 사용자 로그인 정보 저장
    //        var saveResult = await _memoryDb.SaveUserLoginInfo(request.PlayerId, request.Token, request.AppVersion, request.DataVersion);
    //        if (!saveResult)
    //        {
    //            _logger.LogError("Failed to save login info to Redis for UserId: {UserId}", request.PlayerId);
    //            return new LoginResponse { Result = ErrorCode.InternalError };
    //        }

    //        _logger.LogInformation("Successfully authenticated user with token");

    //        var charInfo = await _gameDb.GetUserGameDataAsync(request.PlayerId);
    //        if (charInfo == null)
    //        {
    //            _logger.LogInformation("First login detected, creating new char_info for hive_player_id: {PlayerId}", request.PlayerId);
    //            charInfo = await _gameDb.CreateUserGameDataAsync(request.PlayerId);
    //        }

    //        return new LoginResponse
    //        {
    //            Result = ErrorCode.None
    //        };
    //    }
    //    catch (HttpRequestException e)
    //    {
    //        _logger.LogError(e, "HTTP request to token validation service failed.");
    //        return new LoginResponse { Result = ErrorCode.ServerError };
    //    }
    //    catch (JsonException e)
    //    {
    //        _logger.LogError(e, "Error parsing JSON from token validation service.");
    //        return new LoginResponse { Result = ErrorCode.JsonParsingError };
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogError(e, "Unexpected error occurred during login.");
    //        return new LoginResponse { Result = ErrorCode.InternalError };
    //    }
    //}

    // Q. Login() 코드가 너무 길어서, Redis에 사용자 로그인 정보를 저장하는 부분 분리하는 게 좋을까요?
    // -> 그런데 막성 구상해보니까 너무 작네요, 그냥 나중에 Service에서 응답구조 안만들고 Controller에서 처리하면 괜찮을 듯 합니다.
    //private async Task<bool> SaveLoginInfoToRedis(string playerId, string token, string appVersion, string dataVersion)
    //{
    //    var saveResult = await _memoryDb.SaveUserLoginInfo(playerId, token, appVersion, dataVersion);
    //    if (!saveResult)
    //    {
    //        _logger.LogError("Failed to save login info to Redis for UserId: {UserId}", playerId);
    //        return false;
    //    }
    //    return true;
    //}


