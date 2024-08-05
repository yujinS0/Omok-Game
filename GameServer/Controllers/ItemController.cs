using GameServer.DTO;
using GameServer.Services;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ServerShared;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private readonly ILogger<ItemController> _logger;
    private readonly IItemService _itemService;

    public ItemController(ILogger<ItemController> logger, IItemService itemService)
    {
        _logger = logger;
        _itemService = itemService;
    }

    [HttpPost("get-list")]
    public async Task<PlayerItemResponse> GetPlayerItems([FromBody] PlayerItemRequest request)
    {

        if (HttpContext.Items.TryGetValue("PlayerUid", out var playerUidObj) && playerUidObj is long playerUid)
        {
            var (result, playerItemCode, itemCode, itemCnt) = await _itemService.GetPlayerItems(playerUid, request.ItemPageNum);

            if (result != ErrorCode.None)
            {
                return new PlayerItemResponse
                {
                    Result = result,
                    PlayerItemCode = null,
                    ItemCode = null,
                    ItemCnt = null
                };
            }

            return new PlayerItemResponse
            {
                Result = result,
                PlayerItemCode = playerItemCode,
                ItemCode = itemCode,
                ItemCnt = itemCnt
            };
        }
        else
        {
            return new PlayerItemResponse
            {
                Result = ErrorCode.PlayerUidNotFound,
                PlayerItemCode = null,
                ItemCode = null,
                ItemCnt = null
            };
        }
    }
}