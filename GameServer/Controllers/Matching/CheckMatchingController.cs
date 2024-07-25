using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using ServerShared;

namespace GameServer.Controllers.Matching;

[ApiController]
[Route("[controller]")]
public class CheckMatchingController : ControllerBase
{
    private readonly ILogger<CheckMatchingController> _logger;
    private readonly IMatchingService _matchingService;

    public CheckMatchingController(ILogger<CheckMatchingController> logger, IMatchingService matchingService)
    {
        _logger = logger;
        _matchingService = matchingService;
    }

    [HttpPost]
    public async Task<MatchCompleteResponse> CheckAndInitializeMatch([FromBody] MatchRequest request)
    {
        var (result, matchResult) = await _matchingService.CheckAndInitializeMatchAsync(request.PlayerId);

        return new MatchCompleteResponse
        {
            Result = result,
            Success = matchResult != null ? 1 : 0
        };
    }
}