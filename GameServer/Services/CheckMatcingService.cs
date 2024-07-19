using GameServer;
using GameServer.DTO;
using GameServer.Repository;
using GameServer.Models;
using ServerShared;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using GameServer.Services;
using Microsoft.Extensions.Logging;

namespace MatchServer.Services;
public class CheckMatchingService : ICheckMatchingService
{
    private readonly IMemoryDb _memoryDb;
    private readonly ILogger<CheckMatchingService> _logger;

    public CheckMatchingService(IMemoryDb memoryDb, ILogger<CheckMatchingService> logger)
    {
        _memoryDb = memoryDb;
        _logger = logger;
    }

    public async Task<MatchCompleteResponse> CheckAndInitializeMatch(MatchRequest request)
    {
        var matchResultkey = KeyGenerator.MatchResult(request.PlayerId);
        var result = await _memoryDb.GetMatchResultAsync(matchResultkey);

        if (result == null)
        {
            return new MatchCompleteResponse
            {
                Result = ErrorCode.None,
                Success = 0
            };
        }

        // 매칭 성공 확인 시
        var userGameDatakey = KeyGenerator.PlayingUser(request.PlayerId); // ?

        var userGameData = new UserGameData
        {
            GameRoomId = result.GameRoomId
            // CreatedAt Redis에 넣을 때 생성
        };

        _memoryDb.StorePlayingUserInfoAsync(userGameDatakey, userGameData).Wait();


        // 매칭 성공 했으니 게임 시작 상태로 바꿔주기 (근데 여기서 처리하면 플레이어 별로 Update해서 총 두번씩 호출된다..)
        byte[] getGameRawData = await _memoryDb.GetGameDataAsync(result.GameRoomId);

        var omokGameData = new OmokGameData();
        byte[] gameRawData = omokGameData.StartGame(getGameRawData);

        var gameStartResult = await _memoryDb.UpdateGameDataAsync(result.GameRoomId, gameRawData); // 게임 시작 상태로 데이터 업데이트
        _logger.LogInformation("Update game info: GamerawData={gameStartResult}", gameStartResult);

        if (!gameStartResult)
        {
            _logger.LogError("Failed to update game info");
            return new MatchCompleteResponse
            {
                Result = ErrorCode.UpdateStartGameDataFailException,
                Success = 0
            };
        }

        return new MatchCompleteResponse
        {
            Result = ErrorCode.None,
            Success = 1
        };
    }
}