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

    public ItemService(ILogger<ItemService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _memoryDb = memoryDb;
    }

    //TODO: (08.01) 반환하는 개수가 너무 많네요. 이정도는 클래스를 정의해서 반환하죠
    //=> 수정 완료했습니다. 기존에 정의하고 사용하던 PlayerItem 활용
    public async Task<(ErrorCode, List<PlayerItem>)> GetPlayerItems(Int64 playerUid, int itemPageNum)
    {
        try
        {
            var items = await _gameDb.GetPlayerItems(playerUid, itemPageNum, PageSize);

            if (items != null)
            {
                return (ErrorCode.None, items);
            }
            else
            {
                return (ErrorCode.None, new List<PlayerItem>());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting player items for playerUid: {PlayerUid}", playerUid);
            return (ErrorCode.GameDatabaseError, null);
        }
    }
}