using GameServer.DTO;
using GameServer.Services;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ServerShared;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CharacterController : ControllerBase
{
    private readonly ILogger<CharacterController> _logger;
    private readonly ICharacterService _characterService;

    public CharacterController(ILogger<CharacterController> logger, ICharacterService characterService)
    {
        _logger = logger;
        _characterService = characterService;
    }

    [HttpPost("updatename")]
    public async Task<UpdateCharacterNameResponse> UpdateCharacterName([FromBody] UpdateCharacterNameRequest request)
    {
        var result = await _characterService.UpdateCharacterNameAsync(request.PlayerId, request.CharName);

        return new UpdateCharacterNameResponse
        {
            Result = result
        };
    }

    [HttpPost("getinfo")]
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

}
