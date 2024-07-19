using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using ServerShared;

namespace GameServer.Controllers;

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
        var result = await _matchingService.CheckAndInitializeMatch(request.PlayerId);

        if (result == null)
        {
            return new MatchCompleteResponse
            {
                Result = ErrorCode.None,
                Success = 0
            };
        }

        return new MatchCompleteResponse
        {
            Result = ErrorCode.None,
            Success = 1
        };
    }
}
// Redis에서 Player의 매칭 결과 확인
// 없으면 아직 매칭 중이라고 클라이언트에게 통보

// 있으면
// 매칭 데이터를 가져와서 이 사람의 게임 데이터를 레디스에 만들어주기
// 게임 데이터는 매칭 당시 시간과 GameRoomId