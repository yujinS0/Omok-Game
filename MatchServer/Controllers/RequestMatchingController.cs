using Microsoft.AspNetCore.Mvc;
using MatchServer.DTO;
using MatchServer.Services.Interfaces;

namespace MatchServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RequestMatchingController : ControllerBase
{
    private readonly ILogger<RequestMatchingController> _logger;
    private readonly IRequestMatchingService _matchService;

    public RequestMatchingController(ILogger<RequestMatchingController> logger, IRequestMatchingService matchService)
    {
        _logger = logger;
        _matchService = matchService;
    }

    [HttpPost]
    public MatchResponse Match([FromBody] MatchRequest request)
    {
        var response = _matchService.Matching(request);
        if (response.Result == ErrorCode.InvalidRequest)
        {
            return response;
        }
        return response;
    }
}