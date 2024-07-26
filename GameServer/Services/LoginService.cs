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

    //TODO: 게임서버에서는 외부 서비스 호출, DB 호출은 모두가 비동기 호출을 합니다. 그래서 외부 서비스 호출이나 DB 호출을 하는 함수에 async 이름을 붙이지 마세요. 불필요합니다.

    public async Task<ErrorCode> VerifyTokenAndInitializePlayerDataAsync(VerifyTokenRequest verifyTokenRequest, GameLoginRequest request)
    {
        //TODO: 52라인까지를 하나의 함수로 만들어주세요. 34라인에서 53라인까지는 VerifyTokenAsync 에 들어가야 합니다.
        ///
        var (result, responseBody) = await VerifyTokenAsync(verifyTokenRequest);

        if (result != ErrorCode.None)
        {
            _logger.LogWarning("Token validation failed with result: {Result}", result);
            return result;
        }

        int validationResult;
        using (JsonDocument doc = JsonDocument.Parse(responseBody))
        {
            JsonElement root = doc.RootElement;
            validationResult = root.GetProperty("result").GetInt32();
        }

        if (validationResult != 0)
        {
            _logger.LogWarning("Token validation failed with result: {Result}", validationResult);
            return (ErrorCode)validationResult;
        }
        /////


        //TODO: 이름을 메모리디비에 플레이어 기본 정보를 저장한다는 뜻이 들어가면 좋겠습니다.
        var saveResult = await SaveLoginInfoAsync(request.PlayerId, request.Token, request.AppVersion, request.DataVersion);
        if (saveResult != ErrorCode.None)
        {
            return saveResult;
        }

        //TODO 실패를 하는 경우 위에 Redis에 저장한 것 삭제해야 합니다.
        var initializeResult = await InitializeUserDataAsync(request.PlayerId);
        if (initializeResult != ErrorCode.None)
        {
            return initializeResult;
        }

        _logger.LogInformation("Successfully authenticated user with token");

        return ErrorCode.None;
    }

    private async Task<(ErrorCode, string)> VerifyTokenAsync(VerifyTokenRequest verifyTokenRequest)
    {
        var client = _httpClientFactory.CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(verifyTokenRequest), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://localhost:5284/VerifyToken", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return (ErrorCode.None, responseBody);
    }

    private async Task<ErrorCode> SaveLoginInfoAsync(string playerId, string token, string appVersion, string dataVersion)
    {
        var saveResult = await _memoryDb.SaveUserLoginInfo(playerId, token, appVersion, dataVersion);
        if (!saveResult)
        {
            _logger.LogError("Failed to save login info to Redis for UserId: {UserId}", playerId);
            return ErrorCode.InternalError;
        }
        return ErrorCode.None;
    }

    private async Task<ErrorCode> InitializeUserDataAsync(string playerId)
    {
        var playerInfo = await _gameDb.GetPlayerInfoDataAsync(playerId);
        if (playerInfo == null)
        {
            _logger.LogInformation("First login detected, creating new player_info for hive_player_id: {PlayerId}", playerId);
            playerInfo = await _gameDb.CreatePlayerInfoDataAsync(playerId);
        }
        return ErrorCode.None;
    }

}

