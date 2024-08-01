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

    //TODO: (08.01) 복수의 아이템을 가져오면 GetPlayerItems로 이름을 변경해주세요. 시퀸스다이얼그램과 이름이 일치해야 합니다.
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