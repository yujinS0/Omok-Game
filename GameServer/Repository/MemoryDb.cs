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

        public void Dispose()
        {
            // _redisConn?.Dispose(); // Redis 연결 해제
        }
    }
}
