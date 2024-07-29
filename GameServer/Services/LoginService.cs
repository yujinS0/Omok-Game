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

    public async Task<ErrorCode> login(string playerId, string token, string appVersion, string dataVersion)
    {
        //TODO: 52라인까지를 하나의 함수로 만들어주세요. 34라인에서 53라인까지는 VerifyTokenAsync 에 들어가야 합니다.
        //=> 수정 완료했습니다
        var result = await VerifyToken(playerId, token);


        //TODO: 이름을 메모리디비에 플레이어 기본 정보를 저장한다는 뜻이 들어가면 좋겠습니다.
        //=> 수정완료했습니다.
        var saveResult = await SavePlayerLoginInfoToMemoryDb(playerId, token, appVersion, dataVersion);
        if (saveResult != ErrorCode.None)
        {
            return saveResult;
        }

        //TODO 실패를 하는 경우 위에 Redis에 저장한 것 삭제해야 합니다.
        //=> 수정 완료했습니다
        var initializeResult = await InitializeUserData(playerId);
        if (initializeResult != ErrorCode.None)
        {
            await _memoryDb.DeletePlayerLoginInfo(playerId); // 실패 시 Redis 데이터 삭제
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

