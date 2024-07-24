using GameServer.DTO;
using GameServer.Services;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ServerShared;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class PlayerBasicDataController : ControllerBase
{
    private readonly ILogger<PlayerBasicDataController> _logger;
    private readonly IPlayerInfoService _characterService;

    public PlayerBasicDataController(ILogger<PlayerBasicDataController> logger, IPlayerInfoService characterService)
    {
        _logger = logger;
        _characterService = characterService;
    }

    [HttpPost("basic-player-data")]
    public async Task<CharacterSummaryResponse> GetCharacterInfoSummary([FromBody] CharacterSummaryRequest request)
    {
        var (error, charInfo) = await _characterService.GetCharInfoSummaryAsync(request.PlayerId);

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
        var result = await _characterService.UpdateCharacterNameAsync(request.PlayerId, request.CharName);

        return new UpdateCharacterNameResponse
        {
            Result = result
        };
    }

    

}
