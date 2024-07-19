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
        try
        {
            var matchResponse = await _matchingService.RequestMatchingAsync(request);
            _logger.LogInformation($"[RequestMatching] PlayerId: {request.PlayerId}, Result: {matchResponse.Result}");
            return matchResponse;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error while calling match server");
            return new MatchResponse 
            { 
                Result = ErrorCode.ServerError 
            };
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error parsing JSON from match server");
            return new MatchResponse 
            { 
                Result = ErrorCode.JsonParsingError 
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred");
            return new MatchResponse 
            { 
                Result = ErrorCode.InternalError 
            };
        }
    }
}