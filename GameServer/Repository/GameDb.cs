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
                PlayerId = playerId,
                NickName = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 27), // 초기 닉네임 랜덤 생성
                Exp = 0,
                Level = 1,
                Win = 0,
                Lose = 0,
                Draw = 0
            };

            var insertId = await _queryFactory.Query("player_info").InsertGetIdAsync<int>(new
            {
                player_id = newPlayerInfo.PlayerId,
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
            foreach (var item in firstItems)
            {
                //TODO: (08.05) 매직넘버를 사용하면 안됩니다
                //=> 수정 완료했습니다.
                if (item.ItemCode == GameConstants.GameMoneyItemCode)
                {
                    await _queryFactory.Query("player_money").InsertAsync(new
                    {
                        player_uid = playerUid,
                        game_money = item.Count
                    }, transaction);
                }
                else if (item.ItemCode == GameConstants.DiamondItemCode)
                {
                    await _queryFactory.Query("player_money").InsertAsync(new
                    {
                        player_uid = playerUid,
                        diamond = item.Count
                    }, transaction);
                }
                else
                {
                    await _queryFactory.Query("player_item").InsertAsync(new
                    {
                        player_uid = playerUid,
                        item_code = item.ItemCode,
                        item_cnt = item.Count
                    }, transaction);
                }

                //TODO: (08.05) 아이템 넣다가 하나라도 실패가 발생하면 롤백해야 합니다.
                //=> 수정 완료했습니다.
                _logger.LogInformation($"Added item for player_uid={playerUid}: ItemCode={item.ItemCode}, Count={item.Count}");
            }
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding initial items for playerUid: {PlayerUid}", playerUid);
            await transaction.RollbackAsync();
            return ErrorCode.AddFirstItemsForPlayerFail;
        }
    }

    public async Task<PlayerInfo> GetPlayerInfoData(string playerId)
    {
        try
        {
            var result = await _queryFactory.Query("player_info")
                .Where("player_id", playerId)
                .Select("player_id", "nickname", "exp", "level", "win", "lose", "draw")
                .FirstOrDefaultAsync();

            if (result == null)
            {
                _logger.LogWarning("No data found for playerId: {PlayerId}", playerId);
                return null;
            }

            var playerInfo = new PlayerInfo
            {
                PlayerId = result.player_id,
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

    public async Task<bool> UpdateGameResult(string winnerId, string loserId, int WinExp, int LoseExp)
    {
        var winnerData = await GetPlayerInfoData(winnerId);
        var loserData = await GetPlayerInfoData(loserId);

        if (winnerData == null)
        {
            _logger.LogError("Winner data not found for PlayerId: {PlayerId}", winnerId);
            return false;
        }

        if (loserData == null)
        {
            _logger.LogError("Loser data not found for PlayerId: {PlayerId}", loserId);
            return false;
        }

        using (var transaction = await _connection.BeginTransactionAsync())
        {
            try
            {
                winnerData.Win++;
                winnerData.Exp += GameConstants.WinExp;

                loserData.Lose++;
                loserData.Exp += GameConstants.LoseExp;

                var winnerUpdateResult = await _queryFactory.Query("player_info")
                    .Where("player_id", winnerId)
                    .UpdateAsync(new { win = winnerData.Win, exp = winnerData.Exp }, transaction);

                var loserUpdateResult = await _queryFactory.Query("player_info")
                    .Where("player_id", loserId)
                    .UpdateAsync(new { lose = loserData.Lose, exp = loserData.Exp }, transaction);

                if (winnerUpdateResult == 0 || loserUpdateResult == 0)
                {
                    _logger.LogError("Database update failed for winner or loser. WinnerId: {WinnerId}, LoserId: {LoserId}", winnerId, loserId);
                    await transaction.RollbackAsync();
                    return false;
                }

                _logger.LogInformation("Updated game result. Winner: {WinnerId}, Wins: {Wins}, Exp: {WinnerExp}, Loser: {LoserId}, Losses: {Losses}, Exp: {LoserExp}",
                    winnerId, winnerData.Win, winnerData.Exp, loserId, loserData.Lose, loserData.Exp);

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating game result for winner: {WinnerId}, loser: {LoserId}", winnerId, loserId);
                await transaction.RollbackAsync();
                return false;
            }
        }
    }

    public async Task<bool> UpdateNickName(string playerId, string newNickName)
    {
        var affectedRows = await _queryFactory.Query("player_info")
            .Where("player_id", playerId)
            .UpdateAsync(new { nickname = newNickName });

        return affectedRows > 0;
    }

    public async Task<PlayerBasicInfo> GetplayerBasicInfo(string playerId)
    {
        try
        {
            var playerInfoResult = await _queryFactory.Query("player_info")
                .Where("player_id", playerId)
                .Select("player_uid", "nickname", "exp", "level", "win", "lose", "draw")
                .FirstOrDefaultAsync();

            if (playerInfoResult == null)
            {
                _logger.LogWarning("No data found for playerId: {PlayerId}", playerId);
                return null;
            }


            long playerUid = playerInfoResult.player_uid;

            var playerMoneyResult = await _queryFactory.Query("player_money")
                .Where("player_uid", playerUid)
                .Select("game_money", "diamond")
                .FirstOrDefaultAsync();

            if (playerMoneyResult == null)
            {
                _logger.LogWarning("No money data found for playerId: {PlayerId}", playerId);
                return null;
            }


            var playerBasicInfo = new PlayerBasicInfo
            {
                NickName = playerInfoResult.nickname,
                GameMoney = playerMoneyResult.game_money,
                Diamond = playerMoneyResult.diamond,
                Exp = playerInfoResult.exp,
                Level = playerInfoResult.level,
                Win = playerInfoResult.win,
                Lose = playerInfoResult.lose,
                Draw = playerInfoResult.draw
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
                                                 .Where("player_id", playerId)
                                                 .Select("player_uid")
                                                 .FirstOrDefaultAsync<long>();
            return playerUid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player UID for PlayerId: {PlayerId}", playerId);
            return -1; 
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

    // 리턴값이 너무 많음 이건 class로 묶어서 하기. item 처럼
    public async Task<MailBoxList> GetPlayerMailBoxList(long playerUid, int skip, int pageSize)
    {
        //TODO: (08.05) 우편 보낸날짜로 정렬되나요? 
        //TODO: (08.05) 우편 리스트를 보여주는 것인데 너무 많은 우편 정보를 보여주는 것 같습니다.
        // 최근에 보낸 우편이 위에오도록 가져와야한다!!!
        // "우편 제목, 첨부되어있는 아이템 종류, 받은 시간, 아이템 수령 여부"를 포함해야된다고 생각해 이런 식으로 가져왔습니다..!
        // 전제 조건으로 스케쥴러가 매일 특정 시간에 유효기간이 지난 메일은 삭제했다고 하시죠 -> 넵!
        // expire_dt 확인을 하지 않아도 됩니다.
        //=> 수정 완료했습니다!
        var results = await _queryFactory.Query("mailbox")
                                          .Where("player_uid", playerUid)
                                          .OrderByDesc("send_dt") // 최신 순으로 정렬
                                          .Select("mail_id", "title", "item_code", "send_dt", "receive_yn")
                                          .Skip(skip)
                                          .Limit(pageSize)
                                          .GetAsync();

        var mailBoxList = new MailBoxList();

        foreach (var result in results)
        {
            long mailId = Convert.ToInt64(result.mail_id);
            string title = Convert.ToString(result.title);
            int itemCode = Convert.ToInt32(result.item_code);
            DateTime sendDate = Convert.ToDateTime(result.send_dt);
            int receiveYn = Convert.ToInt32(result.receive_yn);

            mailBoxList.MailIds.Add(mailId);
            mailBoxList.MailTitles.Add(title);
            mailBoxList.ItemCodes.Add(itemCode);
            mailBoxList.SendDates.Add(sendDate);
            mailBoxList.ReceiveYns.Add(receiveYn);
        }

        return mailBoxList;
    }


    public async Task<MailDetail> GetMailDetail(long playerUid, Int64 mailId) // GET 함수명만 수정하기. 의미 전달이 제대로 안된다. 
    {
        //TODO: (08.05) 여기서 주는 것과 우편함 리스트에서 받는 것과 별로 내용 차이가 없네요 -> 질문 이후 추가적 피드백 내용 바탕으로 수정
        //=>
        // 아래 피드백처럼 여기서도 그냥 mailId로 읽어오도록 수정하겠습니다.

        // 처음에 player_uid로 이 메일 아이디가 이 사람의 메일이 맞는지만 확인하는 로직 추가하기

        var result = await _queryFactory.Query("mailbox")
                                    .Where("player_uid", playerUid) // 여기서 where 절에 player_uid는 필요없 
                                    .Where("mail_id", mailId)
                                    .FirstOrDefaultAsync();

        if (result == null)
        {
            return null;
        }

        var mailDetail = new MailDetail
        {
            MailId = result.mail_id,
            Title = result.title,
            Content = result.content,
            ItemCode = result.item_code,
            ItemCnt = result.item_cnt,
            SendDate = result.send_dt,
            ExpireDate = result.expire_dt,
            ReceiveDate = result.receive_dt,
            ReceiveYn = result.receive_yn
        };

        return mailDetail;
    }

    public async Task<(int, int, int)> GetMailItemInfo(long playerUid, long mailId)
    {
        var result = await _queryFactory.Query("mailbox")
            .Where("player_uid", playerUid)
            .Where("mail_id", mailId)
            .Select("receive_yn", "item_code", "item_cnt")
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return (-1, -1, -1); // 해당 메일이 없는 경우
        }

        return (result.receive_yn, result.item_code, result.item_cnt);
    }

    public async Task<bool> UpdateMailReceiveStatus(long playerUid, long mailId, MySqlTransaction transaction)
    {
        var updateResult = await _queryFactory.Query("mailbox")
            .Where("player_uid", playerUid)
            .Where("mail_id", mailId)
            .UpdateAsync(new { receive_yn = true, receive_dt = DateTime.Now }, transaction);

        return updateResult > 0;
    }

    public async Task<bool> AddPlayerItem(long playerUid, int itemCode, int itemCnt, MySqlTransaction transaction)
    {
        //TODO: (08.05) 아이템으로 돈과 다이아몬드가 있을 수 있습니다
        //=> 이 부분은 서비스에서 처리하는 게 맞는 것 같아 서비스에 추가
        // 그리고 겹칠 수 있는 아이템이라면 겹쳐야 합니다
        // => 일단 지금 존재하는 아이템들은 모두 겹칠 수 있도록,
        //  masterData item 테이블의 필드에 겹칠 수 있는지에 대한 정보 추가

        if (itemCode == GameConstants.GameMoneyItemCode)
        {
            var result = await _queryFactory.Query("player_money")
                .Where("player_uid", playerUid)
                .IncrementAsync("game_money", itemCnt, transaction);
            return result > 0;
        }
        else if (itemCode == GameConstants.DiamondItemCode)
        {
            var result = await _queryFactory.Query("player_money")
                .Where("player_uid", playerUid)
                .IncrementAsync("diamond", itemCnt, transaction);
            return result > 0;
        }
        else
        {
            var itemInfo = _masterDb.GetItems().FirstOrDefault(i => i.ItemCode == itemCode);
            if (itemInfo?.Countable == GameConstants.Countable) // 합칠 수 있는 아이템
            {
                var existingItem = await _queryFactory.Query("player_item")
                    .Where("item_code", itemCode)
                    .FirstOrDefaultAsync(transaction);

                if (existingItem != null)
                {
                    var results = await _queryFactory.Query("player_item")
                        .Where("item_code", itemCode)
                        .IncrementAsync("item_cnt", itemCnt, transaction);
                    return results > 0;
                }
            }

            var result = await _queryFactory.Query("player_item").InsertAsync(new
            {
                player_uid = playerUid,
                item_code = itemCode,
                item_cnt = itemCnt
            }, transaction);

            return result > 0;
        }
    }
    public async Task<(bool, int?)> ReceiveMailItemTransaction(long playerUid, long mailId)
    {
        var (receiveYn, itemCode, itemCnt) = await GetMailItemInfo(playerUid, mailId);

        if (receiveYn == -1)
        {
            return (false, null); // Mail not found
        }

        if (receiveYn == 1) // 이미 수령한 경우
        {
            return (true, receiveYn);
        }

        using (var transaction = await _connection.BeginTransactionAsync())
        {
            try
            {
                var updateStatus = await UpdateMailReceiveStatus(playerUid, mailId, transaction);
                if (!updateStatus)
                {
                    await transaction.RollbackAsync();
                    return (false, null);
                }

                var addItemResult = await AddPlayerItem(playerUid, itemCode, itemCnt, transaction);
                if (!addItemResult)
                {
                    await transaction.RollbackAsync();
                    return (false, null);
                }

                await transaction.CommitAsync();
                return (true, 0); // 첫수령
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction failed while receiving mail item for playerUid: {PlayerUid}, mailId: {MailId}", playerUid, mailId);
                return (false, null);
            }
        }
    }

    public async Task DeleteMail(long playerUid, Int64 mailId)
    {
        // TODO SYJ : 플레이어의 메일이 맞는지 확인하는 코드 추가 필요

        //TODO: (08.05) mail_id가 유니크하므로 이것만 검색하면 됩니다
        //=> 수정 완료했습니다.
        await _queryFactory.Query("mailbox")
                           .Where("mail_id", mailId)
                           .DeleteAsync();
    }


    // 인자 너무 많으니까 나중에 수정하기.
    public async Task SendMail(long playerUid, string title, string content, int itemCode, int itemCnt, DateTime expireDt)
    {
        await _queryFactory.Query("mailbox").InsertAsync(new
        {
            player_uid = playerUid,
            title = title,
            content = content,
            item_code = itemCode,
            item_cnt = itemCnt,
            send_dt = DateTime.Now,
            expire_dt = expireDt,
            receive_yn = 0
        });
    }

}

