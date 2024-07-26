using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using ServerShared;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers.Matching;

[ApiController]
[Route("[controller]")]
public class RequestMatchingController : ControllerBase
{
    private readonly ILogger<RequestMatchingController> _logger;
    private readonly IMatchingService _matchingService;

    public RequestMatchingController(ILogger<RequestMatchingController> logger, IMatchingService matchingService)
    {
        _logger = logger;
        _matchingService = matchingService;
    }

    [HttpPost]
    public async Task<MatchResponse> RequestMatching([FromBody] MatchRequest request)
    {
        var errorCode = await _matchingService.RequestMatchingAsync(request.PlayerId);

        return new MatchResponse
        {
            Result = errorCode
        };
    }
}