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

        public async Task<bool> SavePlayerLoginInfo(string playerId, string token, string appVersion, string dataVersion)
        {
            var key = KeyGenerator.UserLogin(playerId);
            var playerLoginInfo = new PlayerLoginInfo
            {
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

        public async Task<bool> DeletePlayerLoginInfo(string playerId)
        {
            var key = KeyGenerator.UserLogin(playerId);
            var redisString = new RedisString<PlayerLoginInfo>(_redisConn, key, RedisExpireTime.UserLoginInfo);
            bool result = await redisString.DeleteAsync();
            if (result)
            {
                _logger.LogInformation("Successfully deleted login info for UserId: {UserId}", playerId);
            }
            else
            {
                _logger.LogWarning("Failed to delete login info for UserId: {UserId}", playerId);
            }
            return result;
        }

        public async Task<string> GetUserLoginToken(string playerId)
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


        public async Task<string> GetGameRoomId(string playerId)
        {
            var key = KeyGenerator.PlayingUser(playerId);
            var userGameData = await GetPlayingUserInfo(key);
            
            if (userGameData == null)
            {
                _logger.LogWarning("No game room found for PlayerId: {PlayerId}", playerId);
                return null;
            }

            return userGameData.GameRoomId;
        }

        public async Task<byte[]> GetGameData(string key)
        {
            try
            {
                var redisString = new RedisString<byte[]>(_redisConn, key, RedisExpireTime.GameData);
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

        public async Task<bool> UpdateGameData(string key, byte[] rawData) // key로 OmokData 값 업데이트
        {
            try
            {
                var redisString = new RedisString<byte[]>(_redisConn, key, RedisExpireTime.GameData);
                var result = await redisString.SetAsync(rawData);
                _logger.LogInformation("Update game info: Key={Key}, GamerawData={rawData}", key, rawData);
                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to update game info: Key={Key}, GamerawData={rawData}", key, rawData);
                return false;
            }
        }

        public async Task<MatchResult> GetMatchResult(string key) // 매칭 결과 조회
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
        public async Task<bool> StorePlayingUserInfo(string key, UserGameData playingUserInfo) // key로 PlayingUserInfo 저장
        {
            try
            {
                var redisString = new RedisString<UserGameData>(_redisConn, key, RedisExpireTime.PlayingUserInfo);
                _logger.LogInformation("Attempting to store playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);

                await redisString.SetAsync(playingUserInfo);
                _logger.LogInformation("Stored playing user info: Key={Key}, GameInfo={playingUserInfo}", key, playingUserInfo);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store playing user info: Key={Key}", key);
                return false;
            }
        }
        public async Task<UserGameData> GetPlayingUserInfo(string key)
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

        public async Task<bool> SetUserReqLock(string key)
        {
            try
            {
                var redisString = new RedisString<string>(_redisConn, key, RedisExpireTime.LockTime); // 30초 동안 락 설정
                
                var result = await redisString.SetAsync(key, RedisExpireTime.LockTime, StackExchange.Redis.When.NotExists);
                if (result)
                {
                    _logger.LogInformation("Successfully set lock for Key={Key}", key);
                }
                else
                {
                    _logger.LogWarning("Failed to set lock for Key={Key}", key);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting lock for Key={Key}", key);
                return false;
            }
        }

        public async Task<bool> DelUserReqLock(string key)
        {
            try
            {
                var redisString = new RedisString<string>(_redisConn, key, RedisExpireTime.LockTime);
                var result = await redisString.DeleteAsync();
                if (result)
                {
                    _logger.LogInformation("Successfully deleted lock for Key={Key}", key);
                }
                else
                {
                    _logger.LogWarning("Failed to delete lock for Key={Key}", key);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lock for Key={Key}", key);
                return false;
            }
        }

        public void Dispose()
        {
            // _redisConn?.Dispose(); // Redis 연결 해제
        }

    }
}
