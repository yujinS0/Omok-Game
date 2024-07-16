using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GameServer.DTO;
using GameServer.Services.Interfaces;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckMatchingController : ControllerBase
{
    private readonly ILogger<CheckMatchingController> _logger;
    private readonly ICheckMatchingService _checkMatchingService;

    public CheckMatchingController(ILogger<CheckMatchingController> logger, ICheckMatchingService checkMatchingService)
    {
        _logger = logger;
        _checkMatchingService = checkMatchingService;
    }

    [HttpPost]
    public async Task<MatchCompleteResponse> IsMatched([FromBody] MatchRequest request)
    {
        return await _checkMatchingService.IsMatched(request);
    }
}

//[ApiController]
//[Route("[controller]")]
//public class CheckMatchingController : ControllerBase
//{
//    private readonly IHttpClientFactory _httpClientFactory;
//    private readonly ILogger<CheckMatchingController> _logger;

//    public CheckMatchingController(ILogger<CheckMatchingController> logger, IHttpClientFactory httpClientFactory)
//    {
//        _logger = logger;
//        _httpClientFactory = httpClientFactory;
//    }

//    [HttpPost]
//    public async Task<MatchCompleteResponse> CheckMatching([FromBody] MatchRequest request)
//    {

//        // Redis에서 Player의 매칭 결과 확인
//        // 없으면 아직 매칭 중이라고 클라이언트에게 통보

//        // 있으면
//        // 매칭 데이터를 가져와서 이 사람의 게임 데이터를 레디스에 만들어주기
//        // 게임 데이터는 매칭 당시 시간과 GameRoomId

//        var client = _httpClientFactory.CreateClient();
//        var matchRequestJson = JsonSerializer.Serialize(request);
//        var content = new StringContent(matchRequestJson, Encoding.UTF8, "application/json");



//        // 수정하기 : 현재 매칭 서버의 CheckMatching을 호출하고 있는데, -> 구현 자체를 게임 서버에서 해야한다!
//        try
//        {
//            var response = await client.PostAsync("http://localhost:5259/CheckMatching", content);
//            response.EnsureSuccessStatusCode();

//            var responseBody = await response.Content.ReadAsStringAsync();
//            _logger.LogInformation("Received response from match server [responseBody]: {Response}", responseBody);

//            // 파싱 오류남
//            //var options = new JsonSerializerOptions
//            //{
//            //    PropertyNameCaseInsensitive = true
//            //};
//            //var matchResponse = JsonSerializer.Deserialize<CheckMatchingResponse>(responseBody, options);

//            //// 상세 로그 추가
//            //if (matchResponse != null)
//            //{
//            //    _logger.LogInformation("Deserialized response from match server: Result={Result}, Success={Success}, RoomId={RoomId}, Opponent={Opponent}",
//            //        matchResponse.Result, matchResponse.Success, matchResponse.RoomId, matchResponse.Opponent);
//            //}
//            //else
//            //{
//            //    _logger.LogWarning("Deserialization returned null");
//            //}

//            //if (matchResponse == null)
//            //{
//            //    return new MatchCompleteResponse { Result = ErrorCode.InternalError, Success = 0 };
//            //}

//            //if (matchResponse.Success == 1)
//            //{
//            //    _logger.LogInformation("Match successful for PlayerId: {PlayerId}", request.PlayerId);
//            //}

//            //return new MatchCompleteResponse
//            //{
//            //    Result = matchResponse.Result,
//            //    Success = matchResponse.Success
//            //};

//            // JSON 응답을 수동으로 파싱
//            using (JsonDocument doc = JsonDocument.Parse(responseBody))
//            {
//                var root = doc.RootElement;
//                var matchResponse = new CheckMatchingResponse
//                {
//                    Result = (ErrorCode)root.GetProperty("result").GetInt32(),
//                    Success = root.GetProperty("success").GetInt32(),
//                    GameRoomId = root.TryGetProperty("roomId", out var roomId) ? roomId.GetString() : null,
//                    Opponent = root.TryGetProperty("opponent", out var opponent) ? opponent.GetString() : null
//                };

//                // 상세 로그 추가
//                _logger.LogInformation("Deserialized response from match server: Result={Result}, Success={Success}, RoomId={RoomId}, Opponent={Opponent}",
//                    matchResponse.Result, matchResponse.Success, matchResponse.GameRoomId, matchResponse.Opponent);

//                if (matchResponse.Success == 1) // 매칭 완료
//                {
//                    _logger.LogInformation("Match successful for PlayerId: {PlayerId}", request.PlayerId);



//                }

//                return new MatchCompleteResponse
//                {
//                    Result = matchResponse.Result,
//                    Success = matchResponse.Success
//                };
//            }
//        }
//        catch (HttpRequestException e)
//        {
//            _logger.LogError(e, "Error while calling match server");
//            return new MatchCompleteResponse { Result = ErrorCode.ServerError, Success = 0 };
//        }
//        catch (JsonException e)
//        {
//            _logger.LogError(e, "Error parsing JSON from match server");
//            return new MatchCompleteResponse { Result = ErrorCode.JsonParsingError, Success = 0 };
//        }
//        catch (Exception e)
//        {
//            _logger.LogError(e, "Unexpected error occurred");
//            return new MatchCompleteResponse { Result = ErrorCode.InternalError, Success = 0 };
//        }
//    }
//}



//public class CheckMatchingResponse
//{
//    public ErrorCode Result { get; set; }
//    public int Success { get; set; }
//    public string GameRoomId { get; set; }
//    public string Opponent { get; set; }
//}