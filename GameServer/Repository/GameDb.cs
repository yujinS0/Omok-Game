using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using Microsoft.Extensions.Options;
using GameServer.Models;
using GameServer.DTO;
using ServerShared;
using GameServer.Repository.Interfaces;

namespace GameServer.Repository;

public class GameDb : IGameDb
{
    private readonly IOptions<DbConfig> _dbConfig;
    private readonly ILogger<GameDb> _logger;
    private MySqlConnection _connection;
    readonly QueryFactory _queryFactory;
    private readonly IMasterDb _masterDb;

    public GameDb(IOptions<DbConfig> dbConfig, ILogger<GameDb> logger, IMasterDb masterDb)
    {
        _dbConfig = dbConfig;
        _logger = logger;

        _connection = new MySqlConnection(_dbConfig.Value.MysqlGameDBConnection);
        _connection.Open();

        _queryFactory = new QueryFactory(_connection, new MySqlCompiler());
        _masterDb = masterDb;
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }

    public async Task<PlayerInfo> CreatePlayerInfoDataAndStartItems(string playerId)
    {
        using var transaction = await _connection.BeginTransactionAsync();
        try
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
            }, transaction);

            newPlayerInfo.PlayerUid = insertId;

            var addItemsResult = await AddFirstItemsForPlayer(newPlayerInfo.PlayerUid, transaction);
            if (addItemsResult != ErrorCode.None)
            {
                await transaction.RollbackAsync();
                return null;
            }

            await transaction.CommitAsync();
            return newPlayerInfo;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating player info for playerId: {PlayerId}", playerId);
            return null;
        }
    }

    private async Task<ErrorCode> AddFirstItemsForPlayer(long playerUid, MySqlTransaction transaction)
    {
        var firstItems = _masterDb.GetFirstItems();

        try
        {
            //TODO: (08.01) 기본으로 주는 아이템에 돈이 있는 경우는 어디에 저장하나요?
              // 돈도 아이템 슬롯에 저장하나요?
            foreach (var item in firstItems)
            {
                await _queryFactory.Query("player_item").InsertAsync(new
                {
                    player_uid = playerUid, // player_id를 player_uid로 변경
                    item_code = item.ItemCode,
                    item_cnt = item.Count
                }, transaction);

                _logger.LogInformation($"Added item for player_uid={playerUid}: ItemCode={item.ItemCode}, Count={item.Count}");
            }
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding initial items for playerUid: {PlayerUid}", playerUid);
            return ErrorCode.AddFirstItemsForPlayerFail;
        }
    }

    public async Task<PlayerInfo> GetPlayerInfoData(string playerId)
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

    public async Task UpdateGameResult(string winnerId, string loserId) // SYJ TODO 로직 분리하기? CheckForWinner
    {
        var winnerData = await GetPlayerInfoData(winnerId);
        var loserData = await GetPlayerInfoData(loserId);

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

    public async Task<bool> UpdateNickName(string playerId, string newNickName)
    {
        var affectedRows = await _queryFactory.Query("player_info")
            .Where("hive_player_id", playerId)
            .UpdateAsync(new { nickname = newNickName });

        return affectedRows > 0;
    }

    public async Task<PlayerBasicInfo> GetplayerBasicInfo(string playerId)
    {
        try
        {
            //TODO: (08.01) 플레이어의 게임머니(돈)에 대한 정보는 어떻게 가져오나요?
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
    public async Task<long> GetPlayerUidByPlayerId(string playerId)
    {
        try
        {
            var playerUid = await _queryFactory.Query("player_info")
                                                 .Where("hive_player_id", playerId)
                                                 .Select("player_uid")
                                                 .FirstOrDefaultAsync<long>();
            return playerUid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player UID for PlayerId: {PlayerId}", playerId);
            return -1; // 오류코드 추가?
        }
    }

    public async Task<List<PlayerItem>> GetPlayerItems(long playerUid, int page, int pageSize)
    {
        int skip = (page - 1) * pageSize;

        var rawItems = await _queryFactory.Query("player_item")
                                      .Where("player_uid", playerUid)
                                      .Select("player_item_code", "item_code", "item_cnt")
                                      .Skip(skip)
                                      .Limit(pageSize)
                                      .GetAsync();

        var items = rawItems.Select(item => new PlayerItem
        {
            PlayerItemCode = item.player_item_code,
            ItemCode = item.item_code,
            ItemCnt = item.item_cnt
        }).ToList();

        return items;
    }

}

