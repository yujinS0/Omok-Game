using GameServer;
using GameServer.DTO;
using GameServer.Repository;
using GameServer.Models;
using ServerShared;
using GameServer.Services.Interfaces;

namespace MatchServer.Services;
public class CheckMatchingService : ICheckMatchingService
{
    private readonly IMemoryDb _memoryDb;

    public CheckMatchingService(IMemoryDb memoryDb)
    {
        _memoryDb = memoryDb;
    }

    public async Task<MatchCompleteResponse> IsMatched(MatchRequest request)
    {
        var matchResultkey = KeyGenerator.GenerateMatchResultKey(request.PlayerId);
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
        var userGameDatakey = KeyGenerator.GeneratePlayingUserKey(request.PlayerId); // ?

        var userGameData = new UserGameData
        {
            PlayerId = request.PlayerId,
            GameRoomId = result.GameRoomId
        };

        _memoryDb.StorePlayingUserInfoAsync(userGameDatakey, userGameData, TimeSpan.FromHours(2)).Wait();


        return new MatchCompleteResponse
        {
            Result = ErrorCode.None,
            Success = 1
        };
    }
}