using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using ServerShared;
using GameServer.Repository;
using GameServer.Models;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class PutOmokController : ControllerBase
{
    private readonly ILogger<PutOmokController> _logger;
    //private readonly IPutOmokControllerService _putOmokControllerService;
    private readonly IMemoryDb _memoryDb;

    public PutOmokController(ILogger<PutOmokController> logger, IMemoryDb memoryDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
    }

    [HttpPost]
    public async Task<PutOmokResponse> PutOmok([FromBody] PutOmokRequest request)
    {
        // GameRoomID 가져오는 로직
        string playingUserKey = KeyGenerator.PlayingUser(request.PlayerId);
        UserGameData userGameData = await _memoryDb.GetPlayingUserInfoAsync(playingUserKey);

        if (userGameData == null)
        {
            _logger.LogError("Failed to retrieve playing user info for PlayerId: {PlayerId}", request.PlayerId);
            return new PutOmokResponse { Result = ErrorCode.GameDataNotFound };
        }

        string gameRoomId = userGameData.GameRoomId;

        byte[] rawData = await _memoryDb.GetGameDataAsync(gameRoomId);
        if (rawData == null)
        {
            _logger.LogError("Failed to retrieve game data for RoomId: {RoomId}", gameRoomId);
            return new PutOmokResponse { Result = ErrorCode.GameDataNotFound };
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        // 흑돌 플레이어 이름을 가져와서 요청을 보낸 사람이 흑돌인지 확인
        string blackPlayerName = omokGameData.GetBlackPlayerName();
        bool isBlack = request.PlayerId == blackPlayerName;

        // 돌을 두는 로직을 수행
        byte[] updatedRawData;
        try
        {
            updatedRawData = omokGameData.SetStone(rawData, isBlack, request.X, request.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set stone at ({X}, {Y}) for PlayerId: {PlayerId}", request.X, request.Y, request.PlayerId);
            return new PutOmokResponse { Result = ErrorCode.SetStoneFailException };
        }

        // 업데이트된 데이터 Redis 저장
        bool updateResult = await _memoryDb.UpdateGameDataAsync(gameRoomId, updatedRawData, TimeSpan.FromHours(2));
        if (!updateResult)
        {
            _logger.LogError("Failed to update game data for RoomId: {RoomId}", gameRoomId);
            return new PutOmokResponse { Result = ErrorCode.UpdateGameDataFailException };
        }

        return new PutOmokResponse { Result = ErrorCode.None };
    }
}