using GameServer.DTO;
using GameServer.Services;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ServerShared;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerInfoController : ControllerBase
{
    private readonly ILogger<PlayerInfoController> _logger;
    private readonly IPlayerInfoService _playerInfoService;

    public PlayerInfoController(ILogger<PlayerInfoController> logger, IPlayerInfoService playerInfoService)
    {
        _logger = logger;
        _playerInfoService = playerInfoService;
    }

    [HttpPost("basic-player-data")]
    public async Task<CharacterSummaryResponse> GetCharacterInfoSummary([FromBody] CharacterSummaryRequest request)
    {
        var (error, charInfo) = await _playerInfoService.GetCharInfoSummaryAsync(request.PlayerId);

        if (error != ErrorCode.None)
        {
            return new CharacterSummaryResponse
            {
                Result = error,
                CharSummary = null
            };
        }

        return new CharacterSummaryResponse
        {
            Result = error,
            CharSummary = charInfo
        };
    }

    [HttpPost("update-nickname")]
    public async Task<UpdateCharacterNameResponse> UpdateCharacterName([FromBody] UpdateCharacterNameRequest request)
    {
        var result = await _playerInfoService.UpdateCharacterNameAsync(request.PlayerId, request.CharName);

        return new UpdateCharacterNameResponse
        {
            Result = result
        };
    }

    

}
