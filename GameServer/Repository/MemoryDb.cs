using Microsoft.Extensions.Logging;
using CloudStructures.Structures;
using CloudStructures;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Repository
{
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

        public async Task<bool> SaveUserLoginInfo(string playerId, string token, string appVersion, string dataVersion)
        {
            var key = KeyGenerator.UserLogin(playerId);
            var playerLoginInfo = new PlayerLoginInfo
            {
                PlayerId = playerId,
                Token = token,
                AppVersion = appVersion,
                DataVersion = dataVersion
            };
            var redis = new RedisString<PlayerLoginInfo>(_redisConn, key, RedisExpireTime.UserLoginInfo);
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

        public async Task<string> GetUserLoginTokenAsync(string playerId)
        {
            var key = KeyGenerator.UserLogin(playerId);
            var redisString = new RedisString<PlayerLoginInfo>(_redisConn, key, RedisExpireTime.UserLoginInfo);
            var result = await redisString.GetAsync();

            if (result.HasValue)
            {
                _logger.LogInformation("Successfully retrieved token for UserId={UserId}", playerId);
                return result.Value.Token;
            }
            else
            {
                _logger.LogWarning("No token found for UserId={UserId}", playerId);
                return null;
            }
        }


        public async Task<string> GetGameRoomIdAsync(string playerId)
        {
            var key = KeyGenerator.PlayingUser(playerId);
            var userGameData = await GetPlayingUserInfoAsync(key);
            return userGameData?.GameRoomId;
        }

        public async Task<byte[]> GetGameDataAsync(string key)
        {
            try
            {
                var redisString = new RedisString<byte[]>(_redisConn, key, RedisExpireTime.GameData); // TODO : Magic number 사용 금지! ServerShared에 상수 정의해서 사용하도록
                var result = await redisString.GetAsync();

                if (result.HasValue)
                {
                    _logger.LogInformation("Successfully retrieved data for Key={Key}", key);
                    return result.Value;
                }
                else
                {
                    _logger.LogWarning("No data found for Key={Key}", key);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve data for Key={Key}", key);
                return null;
            }
        }

        public async Task<bool> UpdateGameDataAsync(string key, byte[] rawData) // key로 OmokData 값 업데이트
        {
            try
            {
                var redisString = new RedisString<byte[]>(_redisConn, key, RedisExpireTime.GameData);  // byte[]? OmokGameData?
                var result = await redisString.SetAsync(rawData);
                _logger.LogInformation("Update game info: Key={Key}, GamerawData={rawData}", key, rawData);
                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to update game info: Key={Key}, GamerawData={rawData}", key, rawData);
                return false;
            }
        }


        public async Task<MatchResult> GetMatchResultAsync(string key) // 매칭 결과 조회
        {
            try
            {
                var redisString = new RedisString<MatchResult>(_redisConn, key, RedisExpireTime.GameData); // 조회 결과 저장할 객체 초기화
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

        // 매칭 완료 후 게임중인 유저 게임 데이터 저장하는
        public async Task StorePlayingUserInfoAsync(string key, UserGameData playingUserInfo) // key로 PlayingUserInfo 저장
        {
            try
            {
                var redisString = new RedisString<UserGameData>(_redisConn, key, RedisExpireTime.PlayingUserInfo);
                _logger.LogInformation("Attempting to store playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);
                await redisString.SetAsync(playingUserInfo); // 결과 저장
                _logger.LogInformation("Stored playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store playing user info: Key={Key}", key);
            }
        }
        public async Task<UserGameData> GetPlayingUserInfoAsync(string key)
        {
            try
            {
                var redisString = new RedisString<UserGameData>(_redisConn, key, RedisExpireTime.PlayingUserInfo);
                var result = await redisString.GetAsync();

                if (result.HasValue)
                {
                    _logger.LogInformation("Successfully retrieved playing user info for Key={Key}", key);
                    return result.Value;
                }
                else
                {
                    _logger.LogWarning("No playing user info found for Key={Key}", key);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve playing user info for Key={Key}", key);
                return null;
            }
        }

        public void Dispose()
        {
            // _redisConn?.Dispose(); // Redis 연결 해제
        }
    }
}
