using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using Microsoft.Extensions.Options;
using GameServer.Models;
using GameServer.DTO;

namespace GameServer.Repository;

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

    public async Task<PlayerInfo> CreatePlayerInfoDataAsync(string playerId)
    {
        var newPlayerInfo = new PlayerInfo
        {
            HivePlayerId = playerId,
            NickName = playerId,
            Exp = 0,
            Level = 1,
            Win = 0,
            Lose = 0,
            Draw = 0
        };

        var insertId = await _queryFactory.Query("player_info").InsertGetIdAsync<int>(new
        {
            hive_player_id = newPlayerInfo.HivePlayerId,
            nickname = newPlayerInfo.NickName,
            exp = newPlayerInfo.Exp,
            level = newPlayerInfo.Level,
            win = newPlayerInfo.Win,
            lose = newPlayerInfo.Lose,
            draw = newPlayerInfo.Draw
        });

        newPlayerInfo.PlayerUid = insertId;

        return newPlayerInfo;
    }

    public async Task<PlayerInfo> GetPlayerInfoDataAsync(string playerId)
    {
        try
        {
            var result = await _queryFactory.Query("player_info")
                .Where("hive_player_id", playerId)
                .Select("hive_player_id", "nickname", "exp", "level", "win", "lose", "draw")
                .FirstOrDefaultAsync();

            if (result == null)
            {
                _logger.LogWarning("No data found for playerId: {PlayerId}", playerId);
                return null;
            }

            var playerInfo = new PlayerInfo
            {
                HivePlayerId = result.hive_player_id,
                NickName = result.nickname,
                Exp = result.exp,
                Level = result.level,
                Win = result.win,
                Lose = result.lose,
                Draw = result.draw
            };

            _logger.LogInformation("GetPlayerInfoDataAsync succeeded for playerId: {PlayerId}", playerId);
            return playerInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting player info data for playerId: {PlayerId}", playerId);
            throw;
        }
    }

    public async Task UpdateGameResultAsync(string winnerId, string loserId)
    {
        var winnerData = await GetPlayerInfoDataAsync(winnerId);
        var loserData = await GetPlayerInfoDataAsync(loserId);

        if (winnerData == null)
        {
            _logger.LogError("Winner data not found for PlayerId: {PlayerId}", winnerId);
            return;
        }

        if (loserData == null)
        {
            _logger.LogError("Loser data not found for PlayerId: {PlayerId}", loserId);
            return;
        }

        winnerData.Win++;
        winnerData.Exp += GameConstants.WinExp;

        loserData.Lose++;
        loserData.Exp += GameConstants.LoseExp;

        await _queryFactory.Query("player_info")
            .Where("hive_player_id", winnerId)
            .UpdateAsync(new { win = winnerData.Win, exp = winnerData.Exp });

        await _queryFactory.Query("player_info")
            .Where("hive_player_id", loserId)
            .UpdateAsync(new { lose = loserData.Lose, exp = loserData.Exp });

        _logger.LogInformation("Updated game result. Winner: {WinnerId}, Wins: {Wins}, Exp: {WinnerExp}, Loser: {LoserId}, Losses: {Losses}, Exp: {LoserExp}",
            winnerId, winnerData.Win, winnerData.Exp, loserId, loserData.Lose, loserData.Exp);
    }

    public async Task<bool> UpdateNickNameAsync(string playerId, string newNickName)
    {
        var affectedRows = await _queryFactory.Query("player_info")
            .Where("hive_player_id", playerId)
            .UpdateAsync(new { nickname = newNickName });

        return affectedRows > 0;
    }

    public async Task<PlayerBasicInfo> GetplayerBasicInfoAsync(string playerId)
    {
        try
        {
            var result = await _queryFactory.Query("player_info")
                .Where("hive_player_id", playerId)
                .Select("nickname", "exp", "level", "win", "lose", "draw")
                .FirstOrDefaultAsync();

            if (result == null)
            {
                _logger.LogWarning("No data found for playerId: {PlayerId}", playerId);
                return null;
            }

            var playerBasicInfo = new PlayerBasicInfo
            {
                NickName = result.nickname,
                Exp = result.exp,
                Level = result.level,
                Win = result.win,
                Lose = result.lose,
                Draw = result.draw
            };

            return playerBasicInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting player info summary for playerId: {PlayerId}", playerId);
            throw;
        }
    }



}

