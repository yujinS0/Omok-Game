using Microsoft.Extensions.Logging;
using CloudStructures.Structures;
using CloudStructures;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Repository
{
    public class MemoryDb : IMemoryDb
    {
        private readonly RedisConnection _redisConn;
        private readonly ILogger<MemoryDb> _logger;
        private readonly int _redisExpiryHours;

        public MemoryDb(IOptions<DbConfig> dbConfig, ILogger<MemoryDb> logger)
        {
            _logger = logger;
            RedisConfig config = new RedisConfig("default", dbConfig.Value.RedisGameDBConnection);
            _redisConn = new RedisConnection(config);
            _redisExpiryHours = dbConfig.Value.RedisExpiryHours;
        }

        public async Task<bool> SaveUserLoginInfo(string playerId, string token, string appVersion, string dataVersion)
        {
            var key = KeyGenerator.GeneratePlayerLoginKey(playerId);
            var playerLoginInfo = new PlayerLoginInfo
            {
                PlayerId = playerId,
                Token = token,
                AppVersion = appVersion,
                DataVersion = dataVersion
            };

            var defaultExpiry = TimeSpan.FromHours(_redisExpiryHours); 
            var redis = new RedisString<PlayerLoginInfo>(_redisConn, key, defaultExpiry);
            bool result = await redis.SetAsync(playerLoginInfo);
            if (result)
            {
                _logger.LogInformation("Successfully saved login info for UserId: {UserId}", playerId);
            }
            else
            {
                _logger.LogWarning("Failed to save login info for UserId: {UserId}", playerId);
            }
            return result;
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

        // 매칭 완료 후 유저 게임 데이터 저장하는
        public async Task StorePlayingUserInfoAsync(string key, UserGameData playingUserInfo, TimeSpan expiry) // key로 PlayingUserInfo 저장
        {
            try
            {
                var redisString = new RedisString<UserGameData>(_redisConn, key, expiry);
                _logger.LogInformation("Attempting to store playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);
                await redisString.SetAsync(playingUserInfo); // 결과 저장
                _logger.LogInformation("Stored playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store playing user info: Key={Key}", key);
            }
        }

        public void Dispose()
        {
            // _redisConn?.Dispose(); // Redis 연결 해제
        }
    }
}
