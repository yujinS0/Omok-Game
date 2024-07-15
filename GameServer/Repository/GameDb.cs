using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using Microsoft.Extensions.Options;
using GameServer.Models;

namespace GameServer.Repository
{
    public class GameDb : IGameDb
    {
        private readonly IOptions<DbConfig> _dbConfig;
        private readonly ILogger<GameDb> _logger;
        private MySqlConnection _connection;
        readonly QueryFactory _queryFactory;

        public GameDb(IOptions<DbConfig> dbConfig, ILogger<GameDb> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;

            _connection = new MySqlConnection(_dbConfig.Value.MysqlGameDBConnection);
            _connection.Open();

            _queryFactory = new QueryFactory(_connection, new MySqlCompiler());
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        public async Task<CharInfo> CreateUserGameDataAsync(string playerId)
        {
            var newCharInfo = new CharInfo
            {
                HivePlayerId = playerId,
                CharName = string.Empty,
                Exp = 0,
                Level = 1,
                Win = 0,
                Lose = 0,
                Draw = 0
            };

            var insertId = await _queryFactory.Query("char_info").InsertGetIdAsync<int>(new
            {
                hive_player_id = newCharInfo.HivePlayerId,
                char_name = newCharInfo.CharName,
                char_exp = newCharInfo.Exp,
                char_level = newCharInfo.Level,
                char_win = newCharInfo.Win,
                char_lose = newCharInfo.Lose,
                char_draw = newCharInfo.Draw
            });

            newCharInfo.CharUid = insertId;

            return newCharInfo;
        }

        public async Task<CharInfo> GetUserGameDataAsync(string playerId)
        {
            var charInfo = await _queryFactory.Query("char_info")
                .Where("hive_player_id", playerId)
                .FirstOrDefaultAsync<CharInfo>();

            return charInfo;
        }

        public async Task UpdateCharNameAsync(string playerId, string newCharName)
        {
            await _queryFactory.Query("char_info")
                .Where("hive_player_id", playerId)
                .UpdateAsync(new { char_name = newCharName });
        }
    }
}


