using Microsoft.Extensions.Logging;
using CloudStructures.Structures;
using CloudStructures;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using MatchServer.Models;

namespace MatchServer.Repository;

public class MemoryDb : IMemoryDb
{
    private readonly RedisConnection _redisConn;
    private readonly ILogger<MemoryDb> _logger;

    public MemoryDb(IOptions<DbConfig> dbConfig, ILogger<MemoryDb> logger)
    {
        _logger = logger;
        RedisConfig config = new RedisConfig("default", dbConfig.Value.RedisGameDBConnection);
        _redisConn = new RedisConnection(config);
    }

    public async Task StoreMatchResultAsync(string key, MatchResult matchResult, TimeSpan expiry) // key로 matchResult 저장
    {
        try
        {
            var redisString = new RedisString<MatchResult>(_redisConn, key, expiry); 
            _logger.LogInformation("Attempting to store match result: Key={Key}, MatchResult={MatchResult}", key, matchResult);
            await redisString.SetAsync(matchResult); // 결과 저장
            _logger.LogInformation("Stored match result: Key={Key}, MatchResult={MatchResult}", key, matchResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store match result: Key={Key}, MatchResult={MatchResult}", key, matchResult);
        }
    }

    public async Task StoreGameDataAsync(string key, byte[] rawData, TimeSpan expiry) // key로 OmokData 저장
    {
        try
        {
            var redisString = new RedisString<byte[]>(_redisConn, key, expiry);
            _logger.LogInformation("Attempting to store game info: Key={Key}, GamerawData={rawData}", key, rawData);
            await redisString.SetAsync(rawData); // 결과 저장
            _logger.LogInformation("Stored game info: Key={Key}, GamerawData={rawData}", key, rawData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store game info: Key={Key}, GamerawData={rawData}", key, rawData);
        }
    }

    public async Task StorePlayingUserInfoAsync(string key, PlayingUserInfo playingUserInfo, TimeSpan expiry) // key로 PlayingUserInfo 저장
    {
        try
        {
            var redisString = new RedisString<PlayingUserInfo>(_redisConn, key, expiry);
            _logger.LogInformation("Attempting to store playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);
            await redisString.SetAsync(playingUserInfo); // 결과 저장
            _logger.LogInformation("Stored playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store playing user info: Key={Key}", key);
        }
    }


    public async Task<MatchResult> GetMatchResultAsync(string key) // 매칭 결과 조회
    {
        try
        {
            var redisString = new RedisString<MatchResult>(_redisConn, key, null); // 조회 결과 저장할 객체 초기화
            _logger.LogInformation("Attempting to retrieve match result for Key={Key}", key);
            var matchResult = await redisString.GetAsync(); // GET

            if (matchResult.HasValue)
            {
                _logger.LogInformation("Retrieved match result for Key={Key}: MatchResult={MatchResult}", key, matchResult.Value);
                await redisString.DeleteAsync(); // 조회 후 삭제
                _logger.LogInformation("Deleted match result for Key={Key} from Redis", key);
                return matchResult.Value;
            }
            else
            {
                _logger.LogWarning("No match result found for Key={Key}", key);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve match result for Key={Key}", key);
            return null;
        }
    }



    public void Dispose()
    {
        // _redisConn?.Dispose(); // Redis 연결 해제
    }
}


//public class MemoryDb : IMemoryDb
//{
//    private readonly RedisConnection _redisConn;
//    private readonly ILogger<MemoryDb> _logger;

//    public MemoryDb(IOptions<DbConfig> dbConfig, ILogger<MemoryDb> logger)
//    {
//        _logger = logger;
//        RedisConfig config = new RedisConfig("default", dbConfig.Value.RedisGameDBConnection);
//        _redisConn = new RedisConnection(config);
//    }

//    public async Task StoreMatchResultAsync(string playerId, int roomNum)
//    {
//        try
//        {
//            var redisString = new RedisString<int>(_redisConn, playerId, null);
//            _logger.LogInformation("Checking if match result already exists for PlayerId={PlayerId}", playerId);
//            if (!await redisString.ExistsAsync())
//            {
//                _logger.LogInformation("Attempting to store match result: PlayerId={PlayerId}, RoomNum={RoomNum}", playerId, roomNum);
//                await redisString.SetAsync(roomNum);
//                _logger.LogInformation("Stored match result: PlayerId={PlayerId}, RoomNum={RoomNum}", playerId, roomNum);
//            }
//            else
//            {
//                _logger.LogWarning("Match result for PlayerId={PlayerId} already exists. Skipping store operation.", playerId);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to store match result: PlayerId={PlayerId}, RoomNum={RoomNum}", playerId, roomNum);
//        }
//    }

//    public async Task<int?> GetMatchResultAsync(string playerId)
//    {
//        try
//        {
//            var redisString = new RedisString<int>(_redisConn, playerId, null);
//            _logger.LogInformation("Attempting to retrieve match result for PlayerId={PlayerId}", playerId);
//            var roomNumResult = await redisString.GetAsync();

//            if (roomNumResult.HasValue)
//            {
//                var roomNum = roomNumResult.Value;
//                _logger.LogInformation("Retrieved match result for PlayerId={PlayerId}: RoomNum={RoomNum}", playerId, roomNum);
//                await redisString.DeleteAsync(); // 조회 후 삭제
//                _logger.LogInformation("Deleted match result for PlayerId={PlayerId} from Redis", playerId);
//                return roomNum;
//            }
//            else
//            {
//                _logger.LogWarning("No match result found for PlayerId={PlayerId}", playerId);
//                return null;
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to retrieve match result for PlayerId={PlayerId}", playerId);
//            return null;
//        }
//    }


//    public void Dispose()
//    {
//        // _redisConn?.Dispose(); // Redis 연결 해제
//    }
//}

////////////////

//public async Task<int?> PopRoomNumAsync()
//{
//    var redisList = new RedisList<string>(_redisConn, "RoomNumList", null);
//    _logger.LogInformation("Attempting to pop RoomNum from Redis.");
//    var roomNumResult = await redisList.RightPopAsync();
//    _logger.LogInformation("RoomNumResult: {RoomNumResult}", roomNumResult.HasValue ? roomNumResult.Value : "null");

//    // 값이 있는지 확인
//    if (roomNumResult.HasValue)
//    {
//        if (int.TryParse(roomNumResult.Value, out int roomNum))
//        {
//            _logger.LogInformation("Popped RoomNum {RoomNum} from Redis.", roomNum);
//            return roomNum;
//        }
//        else
//        {
//            _logger.LogError("Failed to parse RoomNum from Redis value.");
//            return null;
//        }
//    }
//    else
//    {
//        _logger.LogWarning("No RoomNum available in Redis.");
//        return null;
//    }
//}