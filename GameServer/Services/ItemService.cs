using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Services.Interfaces;
using ServerShared;
using StackExchange.Redis;
using GameServer.Repository.Interfaces;

namespace GameServer.Services;

public class ItemService : IItemService
{
    private readonly ILogger<ItemService> _logger;
    private readonly IGameDb _gameDb;
    private readonly IMemoryDb _memoryDb;
    private const int PageSize = 20; // 페이지 당 아이템 수

    public ItemService( ILogger<ItemService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _memoryDb = memoryDb;
    }

    public async Task<(ErrorCode, List<long>, List<int>, List<int>)> GetPlayerItem(string playerId, int itemPageNum)
    {
        //TODO: (08.01) 미들웨어에서 playerId를 로딩했으니 미들웨어에서 컨트룰러로 넘겨주도록 해야합니다.
          // 그러면 아래처럼 또 redis에서 플레이어 데이터 가져올 필요가 없습니다.
        // playerId로 MemoryDb에 있는 PlayerLoginInfo에서 PlayerUid 가져오기!
        var playerUid = await _memoryDb.GetPlayerUid(playerId);
        if(playerUid == -1)
        {
            return (ErrorCode.InValidPlayerUidError, null, null, null);
        }
        // 가져온 PlayerUid를 사용해서
        // GameDb에 있는 GetPlayerItems(playerUid, itemPageNum, PageSize);로 가져오기

        try
        {
            var items = await _gameDb.GetPlayerItems(playerUid, itemPageNum, PageSize);

            var playerItemCodes = new List<long>();
            var itemCodes = new List<int>();
            var itemCnts = new List<int>();

            //TODO: (08.01) items가 null인 경우에 문제가 없나요?
            foreach (var item in items)
            {
                playerItemCodes.Add(item.PlayerItemCode);
                itemCodes.Add(item.ItemCode);
                itemCnts.Add(item.ItemCnt);
            }

            return (ErrorCode.None, playerItemCodes, itemCodes, itemCnts);
        }
        catch (Exception ex)
        {
            // 예외 처리 및 로깅
            return (ErrorCode.GameDatabaseError, null, null, null);
        }
    }
}
