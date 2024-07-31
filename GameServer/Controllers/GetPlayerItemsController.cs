using GameServer.DTO;
using GameServer.Services;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ServerShared;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class GetPlayerItemsController : ControllerBase
{
    private readonly ILogger<GetPlayerItemsController> _logger;
    private readonly IItemService _itemService;

    public GetPlayerItemsController(ILogger<GetPlayerItemsController> logger, IItemService itemService)
    {
        _logger = logger;
        _itemService = itemService;
    }

    [HttpPost]
    public async Task<PlayerItemResponse> GetPlayerItem([FromBody] PlayerItemRequest request)
    {
        var (result, playerItemCode, itemCode, itemCnt) = await _itemService.GetPlayerItem(request.PlayerId, request.ItemPageNum);

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
}