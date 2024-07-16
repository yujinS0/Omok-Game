using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class RequestMatchingController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RequestMatchingController> _logger;
    //private readonly IMatchService _MatchService;

    public RequestMatchingController(ILogger<RequestMatchingController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<MatchResponse> RequestMatching([FromBody] MatchRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var matchRequestJson = JsonSerializer.Serialize(request);
        var content = new StringContent(matchRequestJson, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("http://localhost:5259/RequestMatching", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var matchResponse = JsonSerializer.Deserialize<MatchResponse>(responseBody);

            _logger.LogInformation("Received response from match server: {Response}", responseBody);

            return matchResponse ?? new MatchResponse { Result = ErrorCode.InternalError };
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error while calling match server");
            return new MatchResponse { Result = ErrorCode.ServerError };
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error parsing JSON from match server");
            return new MatchResponse { Result = ErrorCode.JsonParsingError };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred");
            return new MatchResponse { Result = ErrorCode.InternalError };
        }
    }
}
