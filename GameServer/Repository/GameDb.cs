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

    public async Task<CharInfo> GetCharInfoDataAsync(string playerId) // 오류 있음
    {
        var charInfo = await _queryFactory.Query("char_info")
            .Where("hive_player_id", playerId)
            .FirstOrDefaultAsync<CharInfo>(); // 이 부분에서 DTO 매핑 오류?
        _logger.LogInformation("GetCharInfoDataAsync");
        return charInfo;
    }
    //public async Task<CharInfo> GetCharInfoDataAsync(string playerId)
    //{
    //    try
    //    {
    //        var result = await _queryFactory.Query("char_info")
    //            .Where("hive_player_id", playerId)
    //            .FirstOrDefaultAsync<IDictionary<string, object>>(); // Dictionary로 결과 가져오기

    //        if (result == null)
    //        {
    //            _logger.LogWarning("No data found for playerId: {PlayerId}", playerId);
    //            return null;
    //        }

    //        var charInfo = new CharInfo
    //        {
    //            HivePlayerId = (string)result["hive_player_id"],
    //            CharName = (string)result["char_name"],
    //            Exp = Convert.ToInt32(result["char_exp"]),
    //            Level = Convert.ToInt32(result["char_level"]),
    //            Win = Convert.ToInt32(result["char_win"]),
    //            Lose = Convert.ToInt32(result["char_lose"]),
    //            Draw = Convert.ToInt32(result["char_draw"])
    //        };

    //        _logger.LogInformation("GetCharInfoDataAsync succeeded for playerId: {PlayerId}", playerId);
    //        return charInfo;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while getting char info data for playerId: {PlayerId}", playerId);
    //        throw;
    //    }
    //}



    public async Task UpdateCharNameAsync(string playerId, string newCharName)
    {
        await _queryFactory.Query("char_info")
            .Where("hive_player_id", playerId)
            .UpdateAsync(new { char_name = newCharName });
    }

    public async Task UpdateGameResultAsync(string winnerId, string loserId)
    {
        var winnerData = await GetCharInfoDataAsync(winnerId);
        var loserData = await GetCharInfoDataAsync(loserId);

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

        await _queryFactory.Query("char_info")
            .Where("hive_player_id", winnerId)
            .UpdateAsync(new { char_win = winnerData.Win, char_exp = winnerData.Exp });

        await _queryFactory.Query("char_info")
            .Where("hive_player_id", loserId)
            .UpdateAsync(new { char_lose = loserData.Lose, char_exp = loserData.Exp });

        _logger.LogInformation("Updated game result. Winner: {WinnerId}, Wins: {Wins}, Exp: {WinnerExp}, Loser: {LoserId}, Losses: {Losses}, Exp: {LoserExp}",
            winnerId, winnerData.Win, winnerData.Exp, loserId, loserData.Lose, loserData.Exp);
    }

    public async Task<bool> UpdateCharacterNameAsync(string playerId, string newCharName)
    {
        var affectedRows = await _queryFactory.Query("char_info")
            .Where("hive_player_id", playerId)
            .UpdateAsync(new { char_name = newCharName });

        return affectedRows > 0;
    }
    // 새로운 함수 추가
    //public async Task<CharInfoDTO> GetCharInfoSummaryAsync(string playerId)
    //{
    //    var query = "SELECT char_name, char_exp, char_level, char_win, char_lose, char_draw FROM char_info WHERE hive_player_id = @playerId";

    //    try
    //    {
    //        using var command = new MySqlCommand(query, _connection);
    //        command.Parameters.AddWithValue("@playerId", playerId);

    //        using var reader = await command.ExecuteReaderAsync();
    //        if (await reader.ReadAsync())
    //        {
    //            var charInfoSummary = new CharInfoDTO
    //            {
    //                CharName = reader.GetString("char_name"),
    //                Exp = reader.GetInt32("char_exp"),
    //                Level = reader.GetInt32("char_level"),
    //                Win = reader.GetInt32("char_win"),
    //                Lose = reader.GetInt32("char_lose"),
    //                Draw = reader.GetInt32("char_draw")
    //            };

    //            return charInfoSummary;
    //        }
    //        else
    //        {
    //            _logger.LogWarning("No data found for playerId: {PlayerId}", playerId);
    //            return null;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "An error occurred while getting char info summary for playerId: {PlayerId}", playerId);
    //        throw;
    //    }
    //}

    public async Task<CharInfoDTO> GetCharInfoSummaryAsync(string playerId)
    {
        try
        {
            var result = await _queryFactory.Query("char_info")
                .Where("hive_player_id", playerId)
                .Select("char_name", "char_exp", "char_level", "char_win", "char_lose", "char_draw")
                .FirstOrDefaultAsync();

            if (result == null)
            {
                _logger.LogWarning("No data found for playerId: {PlayerId}", playerId);
                return null;
            }

            var charInfoSummary = new CharInfoDTO
            {
                CharName = result.char_name,
                Exp = result.char_exp,
                Level = result.char_level,
                Win = result.char_win,
                Lose = result.char_lose,
                Draw = result.char_draw
            };

            return charInfoSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting char info summary for playerId: {PlayerId}", playerId);
            throw;
        }
    }



}

